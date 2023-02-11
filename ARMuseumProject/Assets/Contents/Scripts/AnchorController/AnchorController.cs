using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

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
    public GameController_S2 gameController;
    public GlowingOrb glowingOrb;
    public Progress progressUI;
    //public GameObject groundLayer;
    public ParticleSystem[] groundEffects;
    public float distanceFromCenter;
    public float handModelOffset;
    public float activationRange;
    public float maxRayDistance;
    public float delayBeforeDialog = 2f;
    public float delayAfterDialog = 2f;
    public float motionThreshold = 0.016f;

    public AudioClip audioClip_planeActive;
    private AudioGenerator audioSource_planeActive;

    private EventAnchor eventAnchor;
    private EventAnchor confirmedEventAnchor;
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
        motionCount = -1;
    }

    public void StartAct(Vector3 point)
    {
        StartCoroutine(OpeningScene(point));
    }

    private IEnumerator OpeningScene(Vector3 startPoint)
    {
        glowingOrb.transform.position = startPoint;
        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
        glowingOrb.ShowBody();
        glowingOrb.ShowTrail();

        yield return new WaitForSeconds(delayBeforeDialog);

        dialogGenerator.GenerateDialog("你集齐了历史碎片");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + delayAfterDialog);

        dialogGenerator.GenerateDialog("现在，伸手掌按住手印......\n唤醒久远的记忆......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration / 2);

        gameController.PlayAmbientSound();
        
        yield return new WaitForSeconds(DialogGenerator.dialogDuration / 2 + delayAfterDialog);

        gameController.StartPlaneHint();
        currentState = SceneState.Ready;
    }

    private IEnumerator EndingScene()
    {
        currentState = SceneState.Anchored;
        audioSource_planeActive.Play();
        gameController.StopPlaneHint();
        gameController.UpdateAmbientSoundVolume(0);
        gameController.SetEventAnchor(confirmedEventAnchor);

        yield return new WaitForSeconds(delayBeforeDialog);

        dialogGenerator.GenerateDialog("历史已被唤醒......\n现在，可以收回手掌......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1f);

        //groundLayer.transform.position = confirmedEventAnchor.GetCorrectedHitPoint();
        //groundLayer.SetActive(true);
        glowingOrb.HideTrail();
        glowingOrb.FadeOut();

        yield return new WaitForSeconds(4.47f); // 等待撞击

        foreach (ParticleSystem sys in groundEffects)
        {
            sys.Play();
        }

        yield return new WaitForSeconds(4f); // 等待动画结束

        currentState = SceneState.Suspend;
        glowingOrb.HideBody();
        glowingOrb.gameObject.SetActive(false);
        //groundLayer.SetActive(false);
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
        Vector3 motionAnchor = gameController.getHandJointPose(HandJointID.Palm).position;

        if (motionCount == -1)
        {
            motionCount++;
        } else if (motionCount == 1)
        {
            progressUI.StartRadialProgress();
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

        float volume = 0;

        if (!gameController.GetHandTrackingState())
        {
            StopActivationTiming();
            gameController.UpdateAmbientSoundVolume(volume);
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
            return;
        }

        Pose jointPose = gameController.getHandJointPose(HandJointID.MiddleTip);
        Vector3 laserPosition = jointPose.position;
        Vector3 laserDirection = jointPose.up;
        Ray laser = new(laserPosition, Vector3.down);

        if (Physics.Raycast(laser, out var hitResult, maxRayDistance, planeMask))
        {
            if (hitResult.distance <= activationRange)
            {
                StartActivationTiming();
            }
            else
            {
                StopActivationTiming();
            }

            volume = (maxRayDistance - hitResult.distance) * 3;
            eventAnchor = new EventAnchor(hitResult, laserDirection, distanceFromCenter, handModelOffset);

            glowingOrb.SetEventAnchor(confirmedEventAnchor ?? eventAnchor);
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.planeAnchor);
        }
        else
        {
            StopActivationTiming();
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
        }

        gameController.UpdateAmbientSoundVolume(volume);
    }
}
