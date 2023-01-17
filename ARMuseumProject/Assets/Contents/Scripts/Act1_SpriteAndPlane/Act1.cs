using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NRKernal;
using TMPro;

public class Act1 : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GlowingOrb glowingOrb;
    public GameObject detectionHitVisualizer;
    public GameObject detectionLine;

    private const int layerMask = ~(1 << 12);
    private actState[] showVisualizerStates = { actState.proximityRange, actState.activationRange };
    private LineRenderer detectLineRenderer;
    private HandEnum domainHand = HandEnum.RightHand;
    private float detectionRange = 0.15f;
    private float activationRange = 0.04f;
    private enum actState
    {
        Suspend,
        outRange,
        inRange,
        proximityRange,
        activationRange,
    }
    private actState currentActState = actState.Suspend;

    void Start()
    {
        detectLineRenderer = detectionLine.GetComponent<LineRenderer>();
    }

    public void StartAct()
    {
        PlaneDetectionStart();
        glowingOrb.InitOrb(new Vector3(0, 0, 1), new Quaternion(0, 0, 0, 0), 0.03f);
        glowingOrb.ShowOrb();
        glowingOrb.StartFollow();
    }

    public void EndAct()
    {
        PlaneDetectionStop();
        glowingOrb.ResetAll();
    }

    public void PlaneDetectionStart()
    {
        UpdateActState(actState.outRange);
    }

    public void PlaneDetectionStop()
    {
        UpdateActState(actState.Suspend);
    }

    private void UpdateDetectionVisualizer(Vector3 start, Vector3 end)
    {
        detectLineRenderer.SetPosition(0, start);
        detectLineRenderer.SetPosition(1, end);
        detectionHitVisualizer.transform.position = end;
    }

    private void ShowDetectionVisualizer()
    {
        detectionLine.SetActive(true);
        detectionHitVisualizer.SetActive(true);
    }

    private void HideDetectionVisualizer()
    {
        detectionLine.SetActive(false);
        detectionHitVisualizer.SetActive(false);
    }

    private void UpdateActState(actState state)
    {
        if(state != currentActState)
        {
            bool isVisualizingLastFrame = showVisualizerStates.Contains(currentActState);
            bool isVisualizingThisFrame = showVisualizerStates.Contains(state);

            if (isVisualizingLastFrame && !isVisualizingThisFrame)
            {
                HideDetectionVisualizer();
            } else if (!isVisualizingLastFrame && isVisualizingThisFrame)
            {
                ShowDetectionVisualizer();
            }

            Debug.Log("[Player] Switch to state: " + state);
            text.text = state.ToString();

            currentActState = state;
        }
    }


    void Update()
    {
        if (currentActState == actState.Suspend)
        {
            return; // 停止检测状态下，直接退出循环
        }

        HandState domainHandState = NRInput.Hands.GetHandState(domainHand);
        Vector3 laserAnchor = domainHandState.GetJointPose(HandJointID.IndexTip).position;
        RaycastHit hitResult;

        Debug.DrawRay(laserAnchor, Vector3.down, Color.blue); // 画一条debug线，模拟射线

        if (domainHandState.currentGesture != HandGesture.Point || 
            !Physics.Raycast(new Ray(laserAnchor, Vector3.down), out hitResult, 2, layerMask))
        {
            UpdateActState(actState.outRange); // 手部未在Point状态，或射线未碰撞平面，均进入[超出范围]状态
            return;
        }

        GameObject hit = hitResult.collider.gameObject;

        Debug.Log(laserAnchor);

        if (hit == null || hit.GetComponent<NRTrackableBehaviour>() == null)
        {
            UpdateActState(actState.outRange); // 射线未撞击物体或撞击物非目标平面，则进入[超出范围]状态 
        } else
        {
            if (hitResult.distance <= detectionRange && hitResult.distance > activationRange)
            {  
                UpdateActState(actState.proximityRange);
                UpdateDetectionVisualizer(laserAnchor, hitResult.point);
            } else if (hitResult.distance <= activationRange)
            {
                UpdateActState(actState.activationRange);
                UpdateDetectionVisualizer(laserAnchor, hitResult.point);
            } else
            {
                UpdateActState(actState.inRange);
            }
        }
    }
}
