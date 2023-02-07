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

public class Scene1 : MonoBehaviour
{
    public DialogGenerator dialogGenerator;
    public GameController_S2 gameController;
    public GlowingOrb glowingOrb;
    public Progress progressUI;
    public AudioClip handStableSound;
    public GameObject groundLayer;
    public ParticleSystem[] groundEffects;
    public float distanceFromCenter;
    public float handModelOffset;
    public float activationRange;
    public float maxRayDistance;
    public float delayBeforeDialog = 2f;
    public float delayAfterDialog = 2f;
    public float motionThreshold = 0.016f;

    private EventAnchor eventAnchor;
    private EventAnchor confirmedEventAnchor;
    private AudioSource handStablePlayer;
    private Vector3 prePosition;
    private int motionCount;
    private int planeMask = 1 << 8;
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
        handStablePlayer = gameObject.AddComponent<AudioSource>();
        handStablePlayer.clip = handStableSound;
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
        glowingOrb.InitOrb(startPoint);
        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);

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
        handStablePlayer.Play();
        gameController.StopPlaneHint();
        gameController.UpdateAmbientSoundVolume(0);
        gameController.UpdateEventAnchor(confirmedEventAnchor);

        yield return new WaitForSeconds(delayBeforeDialog);

        dialogGenerator.GenerateDialog("历史已被唤醒......\n现在，可以收回手掌......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        groundLayer.transform.position = confirmedEventAnchor.GetCorrectedHitPoint();
        groundLayer.SetActive(true);
        glowingOrb.FadeOut();

        yield return new WaitForSeconds(4.47f); // 等待撞击

        foreach (ParticleSystem sys in groundEffects)
        {
            sys.Play();
        }

        yield return new WaitForSeconds(4f); // 等待动画结束

        currentState = SceneState.Suspend;
        glowingOrb.DestoryOrb();
        groundLayer.SetActive(false);
        gameController.NextScene();
    }

    private void StartActivationTiming()
    {
        if(currentState == SceneState.Ready)
        {
            currentState = SceneState.Counting;
            InvokeRepeating("MotionDetection", 0, 0.5f);
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
            StartCoroutine("EndingScene");
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

        Debug.Log("count: " + motionCount);
        prePosition = motionAnchor;
    }

     void Update()
    {
        if (currentState == SceneState.Suspend || currentState == SceneState.Anchored)
        {
            return;
        }

        float volume = 0;

        if (!gameController.getHandTrackingState())
        {
            StopActivationTiming();
            gameController.UpdateAmbientSoundVolume(volume);
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
            return;
        }

        Pose jointPose = gameController.getHandJointPose(HandJointID.MiddleTip);
        Vector3 laserPosition = jointPose.position;
        Vector3 laserDirection = jointPose.up;
        Ray laser = new Ray(laserPosition, Vector3.down);

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

            glowingOrb.SetPlaneAnchor(confirmedEventAnchor != null ? confirmedEventAnchor : eventAnchor);
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
