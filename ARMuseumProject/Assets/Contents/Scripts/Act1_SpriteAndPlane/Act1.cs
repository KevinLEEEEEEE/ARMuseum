using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NRKernal;
using TMPro;

public class Act1 : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI text_mc;
    public GlowingOrb glowingOrb;
    public PlaneDetector planeDetector;
    public Transform anchoredTest;

    private HandEnum domainHand = HandEnum.RightHand;
    private float activationRange = 0.1f;
    private bool isActivationTiming;
    private bool isAnchored;
    private Vector3 prePosition;
    private int motionCount;
    private RaycastHit hitResult;
    private actState currentActState;
    private enum actState
    {
        Suspend,
        outRange,
        inRange,
        activationRange,
    }

    private int planeMask = 1 << 8;
    private float maxRayDistance = 1f;

    void Start()
    {
        ResetAll();
        NRInput.RaycastersActive = false;
    }

    public void ResetAll()
    {
        isActivationTiming = false;
        isAnchored = false;
        motionCount = -1;
        currentActState = actState.Suspend;
    }

    public void StartAct()
    {
        PlaneDetectionStart();
        glowingOrb.InitOrb(new Vector3(0, 0, 1), new Quaternion(0, 0, 0, 0), 0.03f);
        glowingOrb.ShowOrb();
    }

    public void EndAct()
    {
        PlaneDetectionStop();
        glowingOrb.ResetAll();
    }

    public void PlaneDetectionStart()
    {
        currentActState = actState.outRange;
    }

    public void PlaneDetectionStop()
    {
        currentActState = actState.Suspend;
    }

    private void StartActivationTiming()
    {
        if (!isActivationTiming && !isAnchored)
        {
            isActivationTiming = true;
            InvokeRepeating("MotionDetection", 0, 0.5f);
        }
    }

    private void StopActivationTiming()
    {
        if (isActivationTiming)
        {
            isActivationTiming = false;
            motionCount = -1;
            CancelInvoke("MotionDetection");
        }
    }

    private void MotionDetection()
    {
        HandState domainHandState = NRInput.Hands.GetHandState(domainHand);
        Vector3 motionAnchor = domainHandState.GetJointPose(HandJointID.Palm).position;

        if (motionCount == -1)
        { 
            motionCount++;
        } else if (motionCount == 5)
        {   
            StopActivationTiming();
            planeDetector.LockTargetPlane(hitResult.collider.gameObject);
            ConfirmAnchoredPlane(hitResult.point);
        } else
        {
            if (Vector3.Distance(motionAnchor, prePosition) <= 0.025f) // 在0.5s内运动距离小于1cm，视为静止
            {
                motionCount++;
            }
            else
            {
                motionCount = 0; // 否则重新计数
            }
        }


        prePosition = motionAnchor;
        text_mc.text = "count: " + motionCount.ToString();
    }

    private void ConfirmAnchoredPlane(Vector3 anchor)
    {
        //isAnchored = true;
        anchoredTest.position = anchor;
        anchoredTest.gameObject.SetActive(true);  
    }

    void Update()
    {
        if (currentActState == actState.Suspend)
        {
            return; // 停止检测状态下，直接退出循环
        }

        HandState domainHandState = NRInput.Hands.GetHandState(domainHand);

        if (!domainHandState.isTracked)
        {
            StopActivationTiming();
            return;
        }

        Pose middleTipPose = domainHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 laserAnchor = middleTipPose.position + middleTipPose.up * 0.025f;

        Debug.DrawRay(laserAnchor, Vector3.down, Color.blue); // 画一条debug线，模拟射线

        if (Physics.Raycast(new Ray(laserAnchor, Vector3.down), out hitResult, maxRayDistance, planeMask))
        {
            if(hitResult.distance <= activationRange)
            {
                currentActState = actState.activationRange;
                StartActivationTiming();
            } else
            {
                currentActState = actState.inRange;
                StopActivationTiming();
            }
        }
        else
        {
            currentActState = actState.outRange;
            StopActivationTiming();
        }

        text.text = "state: " + currentActState.ToString();
    }
}
