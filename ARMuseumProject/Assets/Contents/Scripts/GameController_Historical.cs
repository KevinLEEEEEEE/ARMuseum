using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using DG.Tweening;

public class GameController_Historical : MonoBehaviour
{
    [SerializeField] private PlaneDetector _planeDetector;
    [SerializeField] private AudioClip audioClip_ambientWind;
    [SerializeField] private GameObject groundMask;
    [SerializeField] private float ambientBasicVolume;
    [SerializeField] private Light ambientLightComp;
    [SerializeField] private int initializeIndex;
    [SerializeField] private bool useCustomeAnchorPosition;
    [SerializeField] private Vector3 customeAnchorPosition;
    [SerializeField] private GameObject[] initMessageListener;
    [SerializeField] private Transform[] eventAnchorListener;
    [SerializeField] private Transform[] startPointListener;
    public string UserID
    {
        get
        {
            if(_userID == null)
            {
                _userID = GetUniqueUserID();
            }

            return _userID;
        }
    }
    private string _userID;
    private AudioGenerator audioSource_ambientWind;

    void Start()
    {
        audioSource_ambientWind = new AudioGenerator(gameObject, audioClip_ambientWind, true, false, 0, 0.3f);
        NRInput.RaycastersActive = false;
        HideGroundMask();

        if(useCustomeAnchorPosition && initializeIndex != 0)
        {
            SetAnchoredPosition(customeAnchorPosition, new Vector3(0, 0, 10));
        }

        SetStartPoint(new Vector3(0, 0.3f, 0.5f));
        NextScene();
    }

    private string GetUniqueUserID()
    {
        DateTime dt = DateTime.Now;
        return string.Format("{0}{1}{2}", dt.Day.ToString("00"), dt.Hour.ToString("00"), dt.Minute.ToString("00"));
    }

    private void SetStartPoint(Vector3 point)
    {
        foreach(Transform trans in startPointListener)
        {
            trans.position = point;
        }
    }

    private void SetAnchoredPosition(Vector3 position, Vector3 forward)
    {
        foreach (Transform trans in eventAnchorListener)
        {
            trans.position = position;
            trans.forward = forward;
        }
    }

    public void SetEventAnchor(EventAnchor anchor)
    {
        SetAnchoredPosition(anchor.GetCorrectedHitPoint(), anchor.GetHitDirection());
        _planeDetector.LockTargetPlane(anchor.GetHitObject());
    }

    public void NextScene()
    {
        NRDebugger.Info(string.Format("[GameController] Init scene NO.{0}", initializeIndex + 1));

        initMessageListener[initializeIndex++].SendMessage("Init");
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
        audioSource_ambientWind.SetVolumeInSeconds(volume, duration);
    }

    public void StartPlaneHint()
    {
        _planeDetector.StartPlaneHint();
    }

    public void StopPlaneHint()
    {
        _planeDetector.StopPlaneHint();
    }

    public void ShowGroundMask()
    {
        groundMask.SetActive(true);
    }

    public void HideGroundMask()
    {
        groundMask.SetActive(false);
    }
}
