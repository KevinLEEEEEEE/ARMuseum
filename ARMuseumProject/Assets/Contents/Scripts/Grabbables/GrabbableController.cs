using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbableController : MonoBehaviour
{
    public ExhibitsPanel _TrackingItemController;
    public GameController _GameController;
    public AudioClip GrabStart;
    public AudioClip GrabEnd;
    public AudioClip SelectExhibit;
    public AudioClip DeleteExhibit;
    public AudioClip OpenOrb;
    public AudioClip CloseOrb;

    //private GameObject IndexTip;
    //private GameObject MiddleTip;
    private AudioSource AudioPlayer;
    private GrabbableState CurrState = GrabbableState.Default;
    private enum GrabbableState
    {
        Default,
        Grab, // 在Grab期间通知画布不检测raycast避免误触
        ShowDelete,
        ShowInfoContact,
    }

    void Start()
    {
        AudioPlayer = transform.GetComponent<AudioSource>();

        _GameController.EndTourEvent += ResetAll;

        ResetAll();
    }

    public void OpenOrbMessage()
    {
        PlaySound(OpenOrb);
    }

    public void CloseOrbMessage()
    {
        PlaySound(CloseOrb);
    }

    public void DeleteExhibitsMessage()
    {
        PlaySound(DeleteExhibit);
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name);

        targetObject.position = obj.transform.GetChild(0).position; // 设置初始位置
        targetObject.rotation = obj.transform.GetChild(0).rotation;
        targetObject.gameObject.SetActive(true);
        PlaySound(SelectExhibit);
    }

    public void InactiveGrabbleItem(GameObject obj)
    {
        obj.SetActive(false);
        _TrackingItemController.ActiveExhibit(obj.transform.name);
    }

    private void PlaySound(AudioClip clip)
    {
        AudioPlayer.clip = clip;
        AudioPlayer.Play();
    }

    private void ShowInfoContact()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().ShowInfoContact();
        }
    }

    private void HideInfoContact()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().HideInfoContact();
        }
    }

    //private void EnterDeleteMode()
    //{
    //    foreach (Transform child in transform)
    //    {
    //        child.gameObject.GetComponent<GrabbableObject>().EnterDeleteMode();
    //    }

    //    SwitchFingerTipState(true);
    //}

    //private void ExitDeleteMode()
    //{
    //    foreach (Transform child in transform)
    //    {
    //        child.gameObject.GetComponent<GrabbableObject>().ExitDeleteMode();
    //    }

    //    SwitchFingerTipState(false);
    //}

    public void StartGrab()
    {
        _GameController.GrabStart();
        PlaySound(GrabStart);
    }

    public void StopGrab()
    {
        _GameController.GrabEnd();
        PlaySound(GrabEnd);
    }

    //private void SwitchFingerTipState(bool canTrigger)
    //{
    //    GameObject NRHandVisual = GameObject.Find("NRHandCapsuleVisual_R");

    //    IndexTip = NRHandVisual.transform.GetChild(31).gameObject;
    //    MiddleTip = NRHandVisual.transform.GetChild(35).gameObject;

    //    IndexTip.GetComponent<SphereCollider>().enabled = canTrigger;
    //    IndexTip.GetComponent<SphereCollider>().isTrigger = canTrigger;
    //    MiddleTip.GetComponent<SphereCollider>().enabled = canTrigger;
    //    MiddleTip.GetComponent<SphereCollider>().isTrigger = canTrigger;
    //}

    public void ResetAll()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        CurrState = GrabbableState.Default;
    }

    private void Update()
    {
        HandState RightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState LeftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        bool isTracking = RightHandState.isTracked || LeftHandState.isTracked;
        bool isPointing = RightHandState.currentGesture == HandGesture.Point || LeftHandState.currentGesture == HandGesture.Point;
        bool isVctory = RightHandState.currentGesture == HandGesture.Victory || LeftHandState.currentGesture == HandGesture.Victory;

        if (isTracking)
        {
            if (isPointing)
            {
                if(CurrState != GrabbableState.ShowInfoContact)
                {
                    //ExitDeleteMode();
                    ShowInfoContact();

                    CurrState = GrabbableState.ShowInfoContact;
                }
            }
            else if (isVctory)
            {
                if (CurrState != GrabbableState.ShowDelete)
                {
                    HideInfoContact();
                    //EnterDeleteMode();

                    CurrState = GrabbableState.ShowDelete;
                }
            } else
            {
                if (CurrState != GrabbableState.Default)
                {
                    //ExitDeleteMode();
                    HideInfoContact();

                    CurrState = GrabbableState.Default;
                }
            }  
        }
    }
}
