using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NRKernal.NRExamples;
using NRKernal;

public class GameController_S2 : MonoBehaviour
{
    public GameObject planeDetector;
    public GameObject groundMask;
    public AudioClip audioClip_ambientWind;
    public float ambientBasicVolume;
    public Transform[] eventAnchorListener;
    public GameObject[] initMessageListener;
    public Transform[] startPointListener;

    private int initializeIndex = 0;
    private Coroutine ambientCoroutine;
    private AudioGenerator audioSource_ambientWind;
    private HandEnum domainHand = HandEnum.RightHand;

    void Start()
    {
        audioSource_ambientWind = new AudioGenerator(gameObject, audioClip_ambientWind, true, false, 0, 0.3f);
        NRInput.RaycastersActive = false;   

        SetStartPoint(new Vector3(0, 0, 0.7f));
        NextScene();

        // Skip to voxel

        //foreach (Transform trans in eventAnchorListener)
        //{
        //    trans.position = new Vector3(0, 0, 0);
        //    trans.forward = new Vector3(0, 0, 10);
        //}

        //initMessageListener[1].SendMessage("Init");

        // Skip to shel
    }

    private void SetStartPoint(Vector3 point)
    {
        foreach(Transform trans in startPointListener)
        {
            if(trans)
            {
                trans.position = point;
            }
            
        }
    }

    public void SetEventAnchor(EventAnchor anchor)
    {
        planeDetector.GetComponent<PlaneDetector>().LockTargetPlane(anchor.GetHitObject());

        Vector3 position = anchor.GetCorrectedHitPoint();
        Vector3 forward = anchor.GetHitDirection();

        foreach (Transform trans in eventAnchorListener)
        {
            if(trans)
            {
                trans.position = position;
                trans.forward = forward;
            }
        }
    }

    public void NextScene()
    {
        Debug.Log("[GameController] Init scene: " + initializeIndex);

        Debug.Log(groundMask.activeSelf);

        initMessageListener[initializeIndex].SetActive(true);
        initMessageListener[initializeIndex].SendMessage("Init");
        initializeIndex++;
    }

    public void FinshModeling()
    {
        //shell.InitShell();
    }

    public void ShowGroundMask()
    {
        groundMask.SetActive(true);
    }

    public void HideGroundMask()
    {
        groundMask.SetActive(false);
    }

    public void StartAmbientSound()
    {
        audioSource_ambientWind.Play();
    }

    public void StopAmbientSound()
    {
        audioSource_ambientWind.Stop();
    }

    public void SetAmbientVolume(float volume)
    {
        audioSource_ambientWind.SetVolume(volume);
    }

    public void SetAmbientVolumeInSeconds(float volume, float duration)
    {
        if (ambientCoroutine != null)
        {
            StopCoroutine(ambientCoroutine);
        }

        ambientCoroutine = StartCoroutine(duration.Tweeng((vol) =>
        {
            audioSource_ambientWind.SetVolume(vol);
        }, audioSource_ambientWind.GetVolume(), volume));

        //await Task.Delay(System.TimeSpan.FromSeconds(duration));

        Debug.Log("[GameController] Change ambient volume to: " + volume);
    }

    public HandState GetDomainHandState()
    {
        return NRInput.Hands.GetHandState(domainHand);
    }

    public Pose getHandJointPose(HandJointID jointID)
    {
        return GetDomainHandState().GetJointPose(jointID);
    }

    public bool GetHandTrackingState()
    {
        return GetDomainHandState().isTracked;
    }

    public bool GetHandPinchState()
    {
        return GetDomainHandState().isPinching;
    }

    public void StartPlaneHint()
    {
        foreach (Transform plane in planeDetector.transform)
        {
            plane.GetComponent<Animation>().Play();
        }
    }

    public void StopPlaneHint()
    {
        foreach (Transform plane in planeDetector.transform)
        {
            plane.GetComponent<Animation>().Stop();
        }
    }
}
