using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbablePanel : MonoBehaviour
{
    public ExhibitsPanel exhibitsPanel;
    public GameController gameController;
    public AudioClip GrabStart;
    public AudioClip GrabEnd;
    public AudioClip SelectExhibit;
    public AudioClip DeleteExhibit;
    public AudioClip OpenOrb;
    public AudioClip CloseOrb;

    private AudioSource AudioPlayer;
    private GrabbableState state = GrabbableState.Default;
    private enum GrabbableState
    {
        Default,
        Grab, // 在Grab期间通知画布不检测raycast避免误触
        ShowOrbs,
    }

    void Start()
    {
        AudioPlayer = transform.GetComponent<AudioSource>();
        gameController.EndTourEvent += ResetAll;

        ResetAll();
    }

    // 整合进orb-prefab
    public void OpenOrbMessage()
    {
        PlaySound(OpenOrb);
    }

    public void CloseOrbMessage()
    {
        PlaySound(CloseOrb);
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name);

        targetObject.gameObject.SetActive(true);
        targetObject.GetComponent<GrabbableExhibit>().MoveToDestinationFrom(obj.transform.GetChild(0));
        PlaySound(SelectExhibit);
    }

    public void InactiveGrabbleItem(GameObject obj)
    {
        obj.SetActive(false);
        PlaySound(DeleteExhibit);
        exhibitsPanel.ActiveExhibit(obj.transform.name);
    }

    private void PlaySound(AudioClip clip)
    {
        AudioPlayer.clip = clip;
        AudioPlayer.Play();
    }

    private void ShowOrbs()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableExhibit>().ShowOrbs();
        }
    }

    private void HideOrbs()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableExhibit>().HideOrbs();
        }
    }

    public void StartGrab()
    {
        gameController.GrabStart();
        PlaySound(GrabStart);
    }

    public void StopGrab()
    {
        gameController.GrabEnd();
        PlaySound(GrabEnd);
    }

    public void ResetAll()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        state = GrabbableState.Default;
    }

    private void Update()
    {
        HandState RightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState LeftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        if(!RightHandState.isTracked && !LeftHandState.isTracked)
        {
            return;
        }

        if(RightHandState.currentGesture == HandGesture.Point || LeftHandState.currentGesture == HandGesture.Point)
        {
            if(state != GrabbableState.ShowOrbs)
            {
                ShowOrbs();
                state = GrabbableState.ShowOrbs;
            }
        } else
        {
            if(state != GrabbableState.Default)
            {
                HideOrbs();
                state = GrabbableState.Default;
            }
        }
    }
}
