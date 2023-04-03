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
    private readonly float offset;
    private readonly float heightOffset;

    public EventAnchor(RaycastHit res, Vector3 dir, float distanceFromCenter, float handModelOffset, float yAxisOffset)
    {
        hitResult = res;
        hitDirection = dir;
        offset = distanceFromCenter + handModelOffset;
        heightOffset = yAxisOffset;
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

        Vector3 centerPoint = plane.ClosestPointOnPlane(point);

        return centerPoint + new Vector3(0, heightOffset, 0);
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
    public Progress progressUI;
    public ParticleSystem groundEffect_planeActive;
    public float activationRange;
    public float motionThreshold;
    public float distanceFromCenter;
    public float handModelOffset;
    public float yAxisOffset;
    public AudioClip audioClip_planeActive;
    [SerializeField] private AudioClip audioClip_HistoricalEntry;

    private AudioGenerator audioSource_HistoricalEntry;
    private AudioGenerator audioSource_planeActive;
    private EventAnchor eventAnchor;
    private EventAnchor confirmedEventAnchor;
    private HandState rightHandState;
    private Vector3 prePosition;
    private int motionCount;
    private readonly int planeMask = 1 << 8;
    private enum AnchorState
    {
        Suspend,
        Ready,
        Counting,
        Anchored,
    }
    private AnchorState currentState;

    void Start()
    {
        audioSource_planeActive = new AudioGenerator(gameObject, audioClip_planeActive);
        audioSource_HistoricalEntry = new AudioGenerator(gameObject, audioClip_HistoricalEntry, false, false, 0.5f);
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);

        ResetAll();
    }

    public void ResetAll()
    {
        currentState = AnchorState.Suspend;
        eventAnchor = null;
        motionCount = -1;

        SetRootsActive(false);
    }

    private void SetRootsActive(bool state)
    {
        foreach (Transform trans in transform)
            trans.gameObject.SetActive(state);
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        SetRootsActive(true);
        gameController.StartAmbientSound();
        gameController.SetAmbientVolumeInSeconds(0.4f, 4);

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        audioSource_HistoricalEntry.Play();
        audioSource_HistoricalEntry.SetVolumeInSeconds(1, 4);

        await UniTask.Delay(TimeSpan.FromSeconds(18), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("即将开启青铜器穿越之旅");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("现在，稍微退后\n看向平面...");

        await UniTask.Delay(TimeSpan.FromSeconds(4.5), ignoreTimeScale: false);

        instructionGenerator.GenerateInstruction("贴住手印", "将手掌贴在平面手印处，保持两秒");
        gameController.StartPlaneHint();
        currentState = AnchorState.Ready;
    }

    private async void EndingScene()
    {
        currentState = AnchorState.Anchored;
        
        gameController.StopPlaneHint();
        gameController.SetEventAnchor(confirmedEventAnchor);

        audioSource_planeActive.Play();
        instructionGenerator.HideInstruction();

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("已定位目标...可以收回手掌");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        currentState = AnchorState.Suspend;
        audioSource_planeActive.Unload();
        audioSource_HistoricalEntry.Unload();
        SetRootsActive(false);
        gameController.NextScene();
    }

    private void StartActivationTiming()
    {
        if(currentState == AnchorState.Ready)
        {
            currentState = AnchorState.Counting;
            InvokeRepeating(nameof(MotionDetection), 0, 0.5f);
        }
    }

    private void StopActivationTiming()
    {
        if (currentState == AnchorState.Counting)
        {
            currentState = AnchorState.Ready;
            motionCount = -1;
            CancelInvoke(nameof(MotionDetection));  
        }
    }

    private void MotionDetection()
    {
        Vector3 motionAnchor = rightHandState.GetJointPose(HandJointID.Palm).position;
        NRDebugger.Info("[AnchorController] Motion counting: " + motionCount);

        if (motionCount == -1)
        {
            prePosition = motionAnchor;
        } else if (motionCount == 1)
        {
            progressUI.StartRadialProgress();
            groundEffect_planeActive.transform.parent.position = eventAnchor.GetCorrectedHitPoint();
            groundEffect_planeActive.transform.parent.forward = eventAnchor.GetHitDirection();
            groundEffect_planeActive.Play();
            gameController.SetAmbientVolumeInSeconds(1, 2);
        } else if (motionCount == 4) {
            confirmedEventAnchor = eventAnchor;
        } else if (motionCount == 5)
        {
            StopActivationTiming();
            EndingScene();
        }

        // 判断手掌移动距离是否超出阈值
        if (Vector3.Distance(motionAnchor, prePosition) <= motionThreshold)
        {
            motionCount++;
        }
        else
        {
            motionCount = -1;
            confirmedEventAnchor = null;
            progressUI.ResetRadialProgress();
            groundEffect_planeActive.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            gameController.SetAmbientVolumeInSeconds(0.4f, 1);
        }
        
        prePosition = motionAnchor;
    }

     void Update()
    {
        if (currentState == AnchorState.Suspend || currentState == AnchorState.Anchored)
        {
            return;
        }

        if (!rightHandState.isTracked)
        {
            StopActivationTiming();
            return;
        }

        Pose jointPose = rightHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 laserPoint = jointPose.position;
        Vector3 laserDirection = jointPose.up;

        if (Physics.Raycast(new Ray(laserPoint, Vector3.down), out var hitResult, activationRange, planeMask))
        {
            float heightOffset = Application.isEditor ? 0 : yAxisOffset;
            eventAnchor = new EventAnchor(hitResult, laserDirection, distanceFromCenter, handModelOffset, heightOffset);
            StartActivationTiming();
        }
        else
        {
            eventAnchor = null;
            StopActivationTiming();
        }
    }
}
