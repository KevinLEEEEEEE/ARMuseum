using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using Cysharp.Threading.Tasks;

public class EventAnchor
{
    private RaycastHit hitResult;
    private Vector3 hitDirection;
    private float offset;

    public EventAnchor(RaycastHit res, Vector3 dir, float distanceFromCenter, float handModelOffset)
    {

        hitResult = res;
        hitDirection = dir;
        offset = distanceFromCenter + handModelOffset;
    }

    public GameObject GetHitObject()
    {
        return hitResult.collider.gameObject;
    }

    public Vector3 GetCorrectedHitPoint()
    {
        Vector3 point = hitResult.point + hitDirection * offset;
        Vector3 planeNormal = hitResult.collider.transform.up;
        Vector3 planeCenter = hitResult.point;
        Plane plane = new(planeNormal, planeCenter);

        return plane.ClosestPointOnPlane(point);
    }

    public Vector3 GetHitPoint()
    {
        return hitResult.point;
    }

    public Vector3 GetHitDirection()
    {
        Vector3 planeNormal = hitResult.collider.transform.up;

        return Vector3.ProjectOnPlane(hitDirection, planeNormal);
    }
}

public class AnchorController : MonoBehaviour
{
    public DialogGenerator dialogGenerator;
    public InstructionGenerator instructionGenerator;
    public GameController_Historical gameController;
    public GlowingOrb glowingOrb;
    public Progress progressUI;
    public ParticleSystem groundEffect_orbHit;
    public ParticleSystem groundEffect_planeActive;
    public GameObject effectLayer_planeActive;
    public float distanceFromCenter;
    public float handModelOffset;
    public float activationRange;
    public float motionThreshold;
    public AudioClip audioClip_planeActive;

    private HandState rightHandState;
    private AudioGenerator audioSource_planeActive;
    private EventAnchor eventAnchor;
    private EventAnchor confirmedEventAnchor;
    private Action planeActivateInstruction;
    private Vector3 prePosition;
    private int motionCount;
    private readonly int planeMask = 1 << 8;
    private enum SceneState
    {
        Suspend,
        Ready,
        Counting,
        Anchored,
    }
    private SceneState currentState;

    void Start()
    {
        audioSource_planeActive = new AudioGenerator(gameObject, audioClip_planeActive);
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);

        ResetAll();
    }

    public void ResetAll()
    {
        currentState = SceneState.Suspend;
        eventAnchor = null;
        motionCount = -1;
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        glowingOrb.ShowBody();
        glowingOrb.ShowTrail();

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("你集齐了历史碎片");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1.5), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("现在，激活遗迹......\n唤醒久远的回忆......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration / 2), ignoreTimeScale: false);

        gameController.StartAmbientSound();

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration / 2 + 1), ignoreTimeScale: false);

        planeActivateInstruction = instructionGenerator.GenerateInstruction("任务: 激活遗迹", "将手掌贴在手印痕迹处，并保持两秒");
        gameController.StartPlaneHint();
        currentState = SceneState.Ready;
    }

    private async void EndingScene()
    {
        currentState = SceneState.Anchored;
        planeActivateInstruction();

        gameController.StopPlaneHint();
        gameController.ShowGroundMask();
        gameController.SetEventAnchor(confirmedEventAnchor);

        audioSource_planeActive.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("遗迹已激活...请收回手掌");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        glowingOrb.HideTrail();
        glowingOrb.FadeOut();

        await UniTask.Delay(TimeSpan.FromSeconds(4.47f), ignoreTimeScale: false);

        groundEffect_orbHit.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        currentState = SceneState.Suspend;
        glowingOrb.gameObject.SetActive(false);
        gameController.NextScene();
    }

    private void StartActivationTiming()
    {
        if(currentState == SceneState.Ready)
        {
            currentState = SceneState.Counting;
            InvokeRepeating(nameof(MotionDetection), 0, 0.5f);
        }
    }

    private void StopActivationTiming()
    {
        if (currentState == SceneState.Counting)
        {
            currentState = SceneState.Ready;
            motionCount = -1;
            CancelInvoke("MotionDetection");  
        }
    }

    private void MotionDetection()
    {
        Vector3 motionAnchor = rightHandState.GetJointPose(HandJointID.Palm).position;

        if (motionCount == -1)
        {
            motionCount++;
        } else if (motionCount == 1)
        {
            progressUI.StartRadialProgress();
            groundEffect_planeActive.transform.position = eventAnchor.GetCorrectedHitPoint();
            groundEffect_planeActive.Play();
            gameController.SetAmbientVolumeInSeconds(1, 2);
            motionCount++;
        } else if (motionCount == 4) {
            confirmedEventAnchor = eventAnchor;
            motionCount++;
        } else if (motionCount == 5)
        {
            StopActivationTiming();
            EndingScene();
        } else
        {
            if (Vector3.Distance(motionAnchor, prePosition) <= motionThreshold)
            {
                motionCount++;
            }
            else
            {
                motionCount = -1;
                confirmedEventAnchor = null;
                progressUI.ResetRadialProgress();
                groundEffect_planeActive.Stop();
                gameController.SetAmbientVolumeInSeconds(0, 1);
            }
        }

        NRDebugger.Info("[AnchorController] Motion counting: " + motionCount);
        prePosition = motionAnchor;
    }

     void Update()
    {
        if (currentState == SceneState.Suspend || currentState == SceneState.Anchored)
        {
            return;
        }

        if (!rightHandState.isTracked)
        {
            StopActivationTiming();
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
            return;
        }

        Pose jointPose = rightHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 laserPoint = jointPose.position;
        Vector3 laserDirection = jointPose.up;

        if (Physics.Raycast(new Ray(laserPoint, Vector3.down), out var hitResult, activationRange, planeMask))
        {
            // 如果右手MiddleTip与目标平面的距离在激活范围内
            StartActivationTiming();
            eventAnchor = new EventAnchor(hitResult, laserDirection, distanceFromCenter, handModelOffset);
            glowingOrb.SetEventAnchor(confirmedEventAnchor ?? eventAnchor);
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.planeAnchor);
        }
        else
        {
            StopActivationTiming();
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
        }
    }
}
