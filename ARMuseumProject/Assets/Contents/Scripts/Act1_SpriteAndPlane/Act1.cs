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
    public GameObject ground;
    public AudioClip handEntrySound;
    public AudioClip handStableSound;
    public AudioClip handActiveSound;
    public ParticleSystem[] groundEffects;

    private AudioSource handEntryPlayer;
    private AudioSource handStablePlayer;
    private AudioSource handActivePlayer;
    private float handEntryBaseVolum = 0.3f;
    private float activationRange = 0.16f;
    private bool isActivationTiming;
    private bool isAnchored;
    private Vector3 planeAnchor;
    private Vector3 prePosition;
    private int motionCount;
    private RaycastHit hitResult;
    private bool canDetectRange;

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

        handEntryPlayer.Play();
        canDetectRange = true;

        yield return new WaitForSeconds(delayAfterDialog);

        gameController.StartPlaneHint();   
    }

    private IEnumerator EndingScene()
    {
        handStablePlayer.Play();
        gameController.StopPlaneHint();
        UpdateHandEntryVolume(handEntryBaseVolum);

        yield return new WaitForSeconds(2f);

        dialogGenerator.GenerateDialog("历史已被唤醒......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        ground.SetActive(true);
        glowingOrb.FadeOut();
        UpdateHandEntryVolume(0.2f);
        gameController.SetStoryAnchor(hitResult);

        yield return new WaitForSeconds(4.47f); // 等待撞击

        foreach (ParticleSystem sys in groundEffects)
        {
            sys.Play();
        }

        yield return new WaitForSeconds(4f); // 等待动画结束

        gameController.NextAct();
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

    private void SetPlaneAnchor()
    {
        glowingOrb.SetPlaneAnchor(GetCorrectedHitPoint(), true);
        planeAnchor = GetCorrectedHitPoint();
        ground.transform.position = GetCorrectedHitPoint();
    }

    public Vector3 GetCorrectedHitPoint()
    {
        HandState domainHandState = gameController.GetDomainHandState();
        Pose jointPose = domainHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 point = hitResult.point + jointPose.up * 0.1f;
        Vector3 planeNormal = hitResult.collider.transform.up;
        Vector3 planePosition = hitResult.point;
        Plane plane = new Plane(planeNormal, planePosition);

        return plane.ClosestPointOnPlane(point);
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
            glowingOrb.SetPlaneAnchor(GetCorrectedHitPoint(), false);
            glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.planeAnchor);
            motionCount++;
        } else if (motionCount == 8) {
            SetPlaneAnchor();
            motionCount++;
        } else if (motionCount == 9)
        {
            isAnchored = true;
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
                glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);
            }
        }

        Debug.Log("count: " + motionCount);
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
        if (!canDetectRange || isAnchored)
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
                StartActivationTiming();
            } else
            {
                StopActivationTiming();
            }

            volume += (maxRayDistance - hitResult.distance) * 3;
        }
        else
        {
            StopActivationTiming();
        }

        UpdateHandEntryVolume(volume);   
    }
}
