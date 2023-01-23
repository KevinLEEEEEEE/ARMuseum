using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class Act1 : MonoBehaviour
{  
    public DialogGenerator dialogGenerator;
    public GameController_S2 gameController;
    public GlowingOrb glowingOrb;
    public AudioClip handEntrySound;
    public AudioClip handStableSound;
    public AudioClip handActiveSound;

    private AudioSource handEntryPlayer;
    private AudioSource handStablePlayer;
    private AudioSource handActivePlayer;
    private HandEnum domainHand = HandEnum.RightHand;
    private float activationRange = 0.16f;
    private bool isActivationTiming;
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
        handEntryPlayer.volume = 0;
        handStablePlayer.clip = handStableSound;
        handActivePlayer.clip = handActiveSound;

        ResetAll();
    }

    public void ResetAll()
    {
        isActivationTiming = false;
        canDetectRange = false;
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

        // orbһ��ʼ�Ұ�
        glowingOrb.InitOrb(new Vector3(0, 0, 1)); // ����һ�������������
        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.centerCamera);

        yield return new WaitForSeconds(delayBeforeDialog);

        dialogGenerator.GenerateDialog("���ռ���Զ����Ƭ");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + delayAfterDialog);

        dialogGenerator.GenerateDialog("���ڣ�����Զ�ŵĺۼ�......\n���Ѵ�صĻ���......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + delayAfterDialog);

        glowingOrb.SetOrbTarget(GlowingOrb.OrbTarget.handJoint);
        // orb�����������������ž���ƽ��ı���仯
        canDetectRange = true;
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

    //private IEnumerator ActivationProcess()
    //{
    //    // ���Ȳ�����������֪��������

    //    yield return new WaitForSeconds
    //}

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
            gameController.SetStoryAnchor(hitResult, domainHand);
        } else
        {
            if (Vector3.Distance(motionAnchor, prePosition) <= 0.025f) // ��0.5s���˶�����С��1cm����Ϊ��ֹ
            {
                motionCount++;
            }
            else
            {
                motionCount = 0; // �������¼���
            }
        }

        prePosition = motionAnchor;
    }

    private void PlayHandEntrySound()
    {
        if(!handEntryPlayer.isPlaying)
        {
            handEntryPlayer.Play();
        }
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
            return; // ֹͣ���״̬�£�ֱ���˳�ѭ��
        }

        HandState domainHandState = gameController.GetDomainHandState();

        if (!domainHandState.isTracked)
        {
            StopActivationTiming();
            return;
        }

        Pose middleTipPose = domainHandState.GetJointPose(HandJointID.MiddleTip);
        Vector3 laserAnchor = middleTipPose.position + middleTipPose.up * 0.025f;
        float volume = 0f;

        Debug.DrawRay(laserAnchor, Vector3.down, Color.blue); // ��һ��debug�ߣ�ģ������

        if (Physics.Raycast(new Ray(laserAnchor, Vector3.down), out hitResult, maxRayDistance, planeMask))
        {
            if(hitResult.distance <= activationRange)
            {
                if(currentRange != HandRange.activationRange)
                {
                    currentRange = HandRange.activationRange;
                    PlayHandEntrySound();
                    StartActivationTiming();
                }
            } else
            {
                currentRange = HandRange.inRange;
                StopActivationTiming();
            }

            volume = (maxRayDistance - hitResult.distance) * 3;
        }
        else
        {
            currentRange = HandRange.outRange;
            StopActivationTiming();
        }

        UpdateHandEntryVolume(volume);   
    }
}
