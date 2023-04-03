using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

public class GrabbableExhibit : MonoBehaviour
{
    public string exhibitID;
    public GrabBeginEvent grabBeginEvent;
    public GrabEndEvent grabEndEvent;

    [SerializeField] private OrbButton deleteOrb;
    [SerializeField] private OrbButton toggleOrb;
    [SerializeField] private MeshRenderer rendererComp;
    [SerializeField] private AudioClip exhibitIntroductionClip;
    [SerializeField] private bool playOnEnabled = false;

    private NRGrabbableObject m_NRGrabbableObject;
    private AudioGenerator exhibitIntroductionPlayer;
    private Collider colliderComp;
    private GameObject root;

    private void Awake()
    {
        root = transform.GetChild(0).gameObject;
        colliderComp = transform.GetComponent<Collider>();
        m_NRGrabbableObject = transform.GetComponent<NRGrabbableObject>();
        exhibitIntroductionPlayer = new AudioGenerator(gameObject, exhibitIntroductionClip, false, false, 0);

        deleteOrb.orbButtonStartEvent += DeleteStart;
        deleteOrb.orbButtonStopEvent += DeleteStop;
        deleteOrb.orbButtonFinishEvent += DeleteComplete;
        toggleOrb.orbButtonFinishEvent += () => TogglePlayMode();
        m_NRGrabbableObject.OnGrabBegan += GrabBeginEventHandler;
        m_NRGrabbableObject.OnGrabEnded += GrabEndEventHandler;
    }

    public void Reset()
    {
        DisableGrabbableExhibit();
    }

    private void GrabBeginEventHandler()
    {
        grabBeginEvent?.Invoke();
        deleteOrb.DisableButton();
        toggleOrb.DisableButton();
    }

    private void GrabEndEventHandler()
    {
        grabEndEvent?.Invoke();
        deleteOrb.EnableButton();
        toggleOrb.EnableButton();
    }

    public void EnableGrabbableExhibit()
    {
        deleteOrb.EnableButton();
        toggleOrb.EnableButton();
        colliderComp.enabled = true;
        root.SetActive(true);

        if (playOnEnabled)
            TogglePlayMode(2, 2);
    }

    public void DisableGrabbableExhibit()
    {
        deleteOrb.DisableButton();
        toggleOrb.DisableButton();
        colliderComp.enabled = false;
        root.SetActive(false);
        exhibitIntroductionPlayer.Stop();
    }

    private async void TogglePlayMode(float delay = 0, float duration = 2)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: false);

        if (exhibitIntroductionPlayer.IsPlaying())
        {
            exhibitIntroductionPlayer.Pause();
        } else
        {
            exhibitIntroductionPlayer.Play();
            exhibitIntroductionPlayer.SetVolumeInSeconds(1, duration);     
        }
    }

    public void DeleteStart()
    {
        rendererComp.material.EnableKeyword("_EMISSION");
        DOTween.To((p) =>
        {
            rendererComp.material.SetColor("_EmissionColor", new Color(0.2f, 0, 0) * p);
        }, 2.5f, 0, 1f);
    }

    public void DeleteStop()
    {
        rendererComp.material.DisableKeyword("_EMISSION");
        rendererComp.material.SetColor("_EmissionColor", new Color(0.2f, 0, 0) * 2.5f);
    }

    public void DeleteComplete()
    {
        DeleteStop();
        DisableGrabbableExhibit();
        SendMessageUpwards("InactiveGrabbleItem");
    }

    public void MoveToDestinationFrom(Transform point)
    {
        Transform centerAnchor = NRSessionManager.Instance.CenterCameraAnchor;

        transform.position = point.position;
        transform.LookAt(2 * transform.position - centerAnchor.position);
        transform.DOLocalMove(centerAnchor.position + centerAnchor.forward * 0.45f, 0.5f);
    }

    public delegate void GrabBeginEvent();
    public delegate void GrabEndEvent();
}


