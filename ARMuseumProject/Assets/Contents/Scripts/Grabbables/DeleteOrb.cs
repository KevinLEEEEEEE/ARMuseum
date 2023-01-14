using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using TMPro;

public class DeleteOrb : MonoBehaviour
{
    public TextMeshPro textMesh;
    public Image radialIndicatorUI;
    public AudioClip deleteStart;
    public AudioClip deleteStop;
    private AudioSource audioPlayer;
    private const float DeleteDuration = 2f;
    private float deleteTimer;
    private bool isDeleting;
    private string _triggerCollider;
    private HandEnum triggerHand
    {
        get
        {
            if(_triggerCollider == "ColliderEntity_IndexTip_R")
            {
                return HandEnum.RightHand;
            } else if (_triggerCollider == "ColliderEntity_IndexTip_L")
            {
                return HandEnum.LeftHand;
            } else
            {
                return HandEnum.None;
            }
        }
    }

    private void Start()
    {
        audioPlayer = transform.GetComponent<AudioSource>();

        ResetAll();
    }

    private void OnDisable()
    {
        if(isDeleting) ResetAll();
    }

    public void ResetAll()
    {
        isDeleting = false;
        deleteTimer = 0;
        _triggerCollider = "";
        textMesh.text = "delete";
        ResetRadialIndicator();
        SendMessageUpwards("DeleteStop");
    }

    private void OnTriggerEnter(Collider other)
    {
        isDeleting = true;
        _triggerCollider = other.name;
        textMesh.text = "deleting";
        PlayDeleteStartSound();
        SendMessageUpwards("DeleteStart");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[Player] Quit delete, trigger exit");
        PlayDeleteStopSound();
        ResetAll();
    }

    private void SetRadialIndicator(float percentage)
    {
        radialIndicatorUI.fillAmount = percentage;
    }

    private void ResetRadialIndicator()
    {
        radialIndicatorUI.fillAmount = 0;
    }

    private void PlayDeleteStartSound()
    {
        audioPlayer.clip = deleteStart;
        audioPlayer.Play();
    }

    private void PlayDeleteStopSound()
    {
        audioPlayer.clip = deleteStop;
        audioPlayer.Play();
    }

    void Update()
    {
        if(isDeleting)
        {
            if(NRInput.Hands.GetHandState(triggerHand).currentGesture != HandGesture.Point)
            {
                Debug.Log("[Player] Quit delete, hand gesture changed");
                PlayDeleteStopSound();
                ResetAll();
            }

            deleteTimer += Time.deltaTime;  
            SetRadialIndicator(deleteTimer / DeleteDuration);

            if(deleteTimer >= DeleteDuration)
            {
                Debug.Log("[Player] Delete exhibit success");
                ResetAll();
                SendMessageUpwards("DeleteComplete");    
            }
        }
    }
}
