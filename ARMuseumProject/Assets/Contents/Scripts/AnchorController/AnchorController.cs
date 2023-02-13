using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;

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
    public GameController_S2 gameController;
    public GlowingOrb glowingOrb;
    public Progress progressUI;
    public ParticleSystem[] groundEffect_orbHit;
    public ParticleSystem[] groundEffect_planeActive;
    public GameObject effectLayer_planeActive;
    public float distanceFromCenter;
    public float handModelOffset;
    public float activationRange;
    public float motionThreshold;
    public AudioClip audioClip_planeActive;
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
        StartCoroutine(nameof(OpeningScene));
    }

    private IEnumerator OpeningScene()
    {
        glowingOrb.ShowBody();
        glowingOrb.ShowTrail();

        yield return new WaitForSeconds(2f);

        dialogGenerator.GenerateDialog("你集齐了历史碎片");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        dialogGenerator.GenerateDialog("现在，激活遗迹......\n唤醒久远的回忆......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration / 2);

        gameController.StartAmbientSound();

        yield return new WaitForSeconds(DialogGenerator.dialogDuration / 2 + 1f);

        planeActivateInstruction = instructionGenerator.GenerateInstruction("任务: 激活遗迹", "触碰平面手印处两秒以开启遗迹");
        gameController.StartPlaneHint();
        currentState = SceneState.Ready;
    }

    private IEnumerator EndingScene()
    {
        currentState = SceneState.Anchored;
        planeActivateInstruction();
        audioSource_planeActive.Play();

        yield return new WaitForSeconds(3f);

        dialogGenerator.GenerateDialog("遗迹已被激活......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1f);

        glowingOrb.HideTrail();
        glowingOrb.FadeOut();
        gameController.StopPlaneHint();
        gameController.SetEventAnchor(confirmedEventAnchor);
        gameController.ShowGroundMask();

        yield return new WaitForSeconds(4.47f); // 等待撞击

        StartParticles(groundEffect_orbHit);

        yield return new WaitForSeconds(4f); // 等待动画结束

        currentState = SceneState.Suspend;
        glowingOrb.gameObject.SetActive(false);
        gameController.HideGroundMask();
        gameController.NextScene();
    }

    private void StartParticles(ParticleSystem[] particles)
    {
        foreach (ParticleSystem sys in particles)
        {
            sys.Play();
        }
    }

    private void StopParticles(ParticleSystem[] particles)
    {
        foreach (ParticleSystem sys in particles)
        {
            sys.Stop();
        }
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
        Vector3 motionAnchor = gameController.getHandJointPose(HandJointID.Palm).position;

        if (motionCount == -1)
        {
            motionCount++;
        } else if (motionCount == 1)
        {
            progressUI.StartRadialProgress();
            StartParticles(groundEffect_planeActive);
            effectLayer_planeActive.transform.position = eventAnchor.GetCorrectedHitPoint();
            gameController.SetAmbientVolumeInSeconds(1, 2);
            motionCount++;
        } else if (motionCount == 4) {
            confirmedEventAnchor = eventAnchor;
            motionCount++;
        } else if (motionCount == 5)
        {
            StopActivationTiming();
            StartCoroutine(nameof(EndingScene));
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
                StopParticles(groundEffect_planeActive);
                gameController.SetAmbientVolumeInSeconds(0, 1);
            }
        }

        Debug.Log("[AnchorController] Motion counting: " + motionCount);
        prePosition = motionAnchor;
    }

     void Update()
    {
        if (currentState == SceneState.Suspend || currentState == SceneState.Anchored)
        {
            return;
        }

        if (!gameController.GetHandTrackingState())
        {
            StopActivationTiming();
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
            return;
        }

        Pose jointPose = gameController.getHandJointPose(HandJointID.MiddleTip);
        Vector3 laserPosition = jointPose.position;
        Vector3 laserDirection = jointPose.up;
        Ray laser = new(laserPosition, Vector3.down);

        if (Physics.Raycast(laser, out var hitResult, activationRange, planeMask))
        {
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
