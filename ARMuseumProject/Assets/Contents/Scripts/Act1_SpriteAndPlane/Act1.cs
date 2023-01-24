using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class Act1 : MonoBehaviour
{  
    public DialogGenerator dialogGenerator;
    public GameController_S2 gameController;
    public GlowingOrb glowingOrb;
    public Progress progressUI;
    public AudioClip handEntrySound;
    public AudioClip handStableSound;
    public AudioClip handActiveSound;

    private AudioSource handEntryPlayer;
    private AudioSource handStablePlayer;
    private AudioSource handActivePlayer;
    private float handEntryBaseVolum = 0.3f;
    private float activationRange = 0.16f;
    private bool isActivationTiming;
    private bool isAnchored;
    private Vector3 prePosition;
    private int motionCount;
    private RaycastHit hitResult;
    private bool canDetectRange;
    private HandRange currentRange;
    private enum HandRange
    {
        outRange,
        inRange,
        activationRange,
    }

    private int planeMask = 1 << 8;
    private float maxRayDistance = 0.5f;

    void Start()
    {
        handEntryPlayer = gameObject.AddComponent<AudioSource>();
        handStablePlayer = gameObject.AddComponent<AudioSource>();
        handActivePlayer = gameObject.AddComponent<AudioSource>();
        handEntryPlayer.clip = handEntrySound;
        handEntryPlayer.loop = true;
        handEntryPlayer.volume = handEntryBaseVolum;
        handStablePlayer.clip = handStableSound;
        handActivePlayer.clip = handActiveSound;

        ResetAll();
    }

    public void ResetAll()
    {
        isActivationTiming = false;
        canDetectRange = false;
        isAnchored = false;
        motionCount = -1;
        currentRange = HandRange.outRange;
    }

    public void StartAct()
    {
        StartCoroutine("OpeningScene");
    }

    private IEnumerator OpeningScene()
    {
        float delayBeforeDialog = 2f;
        float delayAfterDialog = 2f;

        glowingOrb.InitOrb(new Vector3(0, 0, 1)); // 由上一个场景传入参数
        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);

        yield return new WaitForSeconds(delayBeforeDialog);

        dialogGenerator.GenerateDialog("你收集了远古碎片");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + delayAfterDialog);

        dialogGenerator.GenerateDialog("现在，触碰远古的痕迹......\n唤醒大地的回忆......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration);

        glowingOrb.ActiveOrb();
        handEntryPlayer.Play();

        yield return new WaitForSeconds(delayAfterDialog);

        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.handJoint);
        canDetectRange = true;
    }

    private IEnumerator EndingScene()
    {
        handStablePlayer.Play();
        isAnchored = true;
        UpdateHandEntryVolume(handEntryBaseVolum);

        yield return new WaitForSeconds(2f);

        dialogGenerator.GenerateDialog("历史已被唤醒......");
        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.spaceAnchor);

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 3f);

        glowingOrb.FadeOut();
        UpdateHandEntryVolume(0.2f);

        yield return new WaitForSeconds(8.5f); // 等待动画结束

        UpdateHandEntryVolume(0);
        gameController.SetStoryAnchor(hitResult);
    }

    public void EndAct()
    {
        glowingOrb.DestoryOrb();
        canDetectRange = false;
    }

    private void StartActivationTiming()
    {
        if (!isActivationTiming)
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
        HandState domainHandState = gameController.GetDomainHandState();
        Vector3 motionAnchor = domainHandState.GetJointPose(HandJointID.Palm).position;

        if (motionCount == -1)
        {
            progressUI.HideProgress();
            motionCount++;
        } else if (motionCount == 3)
        {
            progressUI.StartRadialProgress();
            progressUI.ShowProgress();
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.planeAnchor);
            motionCount++;
        } else if (motionCount == 7) {
            glowingOrb.SetPlaneAnchor(hitResult.point, true);
            motionCount++;
        } else if (motionCount == 9)
        {
            StopActivationTiming();
            StartCoroutine("EndingScene");
        } else
        {
            if (Vector3.Distance(motionAnchor, prePosition) <= 0.01f) // 在0.5s内运动距离小于1cm，视为静止
            {
                motionCount++;
            }
            else
            {
                if (motionCount > 3)
                {
                    motionCount = 3;
                } else
                {
                    motionCount = 0;
                }

                progressUI.HideProgress();
                glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.handJoint);
            }
        }

        Debug.Log("count: " + motionCount);
        glowingOrb.SetPlaneAnchor(hitResult.point, false);
        prePosition = motionAnchor;
    }

    private void UpdateHandEntryVolume(float volume)
    {
        float finalVolume = Mathf.Round(volume * 100) / 100;

        if (handEntryPlayer.volume < finalVolume)
        {
            handEntryPlayer.volume += 0.005f;
        }
        else if (handEntryPlayer.volume > finalVolume)
        {
            handEntryPlayer.volume -= 0.005f;
        }
    }

     void Update()
    {
        if (!canDetectRange)
        {
            return; // 停止检测状态下，直接退出循环
        }

        HandState domainHandState = gameController.GetDomainHandState();
        float volume = handEntryBaseVolum;

        if (!domainHandState.isTracked)
        {
            StopActivationTiming();
            UpdateHandEntryVolume(volume);
            return;
        }

        Pose jointPose = domainHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 laserAnchor = jointPose.position + jointPose.up * 0.025f;
        
        Debug.DrawRay(laserAnchor, Vector3.down, Color.blue); // 画一条debug线，模拟射线

        if (Physics.Raycast(new Ray(laserAnchor, Vector3.down), out hitResult, maxRayDistance, planeMask))
        {
            if(hitResult.distance <= activationRange)
            {
                if(currentRange != HandRange.activationRange)
                {
                    currentRange = HandRange.activationRange;
                    StartActivationTiming();
                }
            } else
            {
                currentRange = HandRange.inRange;
                StopActivationTiming();
            }

            volume += (maxRayDistance - hitResult.distance) * 3;
        }
        else
        {
            currentRange = HandRange.outRange;
            StopActivationTiming();
        }

        UpdateHandEntryVolume(volume);   
    }
}
