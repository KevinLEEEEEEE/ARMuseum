using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using DG.Tweening;

public class GameController_Historical : MonoBehaviour
{
    public GameObject planeDetector;
    public GameObject groundMask;
    public AudioClip audioClip_ambientWind;
    public float ambientBasicVolume;
    public Light ambientLightComp;
    public Transform[] eventAnchorListener;
    public GameObject[] initMessageListener;
    public Transform[] startPointListener;
    public string UserID
    {
        get
        {
            return _userID;
        }
    }

    private string _userID;
    private int initializeIndex = 0;
    private Coroutine ambientCoroutine;
    private AudioGenerator audioSource_ambientWind;
    private readonly HandEnum domainHand = HandEnum.RightHand;

    void Start()
    {
        audioSource_ambientWind = new AudioGenerator(gameObject, audioClip_ambientWind, true, false, 0, 0.3f);
        NRInput.RaycastersActive = false;
        _userID = GetUniqueUserID();

        //SetStartPoint(new Vector3(0, 0, 0.7f));
        //NextScene();

        //foreach (Transform trans in eventAnchorListener)
        //{
        //    trans.position = new Vector3(0, -0.5f, 0.7f);
        //    trans.forward = new Vector3(0, 0, 10);
        //}

        //initMessageListener[2].SendMessage("Init");
    }

    private string GetUniqueUserID()
    {
        DateTime dt = DateTime.Now;

        return string.Format("{0}{1}{2}", dt.Day, dt.Hour, dt.Minute);
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
        NRDebugger.Info("[GameController] Init scene NO." + (initializeIndex + 1));

        initMessageListener[initializeIndex].SendMessage("Init");
        initializeIndex++;
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

    public void SetAmbientLightInSeconds(float intensity, float duration)
    {
        ambientLightComp.DOIntensity(intensity, duration);
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

        NRDebugger.Info("[GameController] Set ambient sound volume to: " + volume);
    }

    public HandState GetDomainHandState()
    {
        return NRInput.Hands.GetHandState(domainHand);
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
