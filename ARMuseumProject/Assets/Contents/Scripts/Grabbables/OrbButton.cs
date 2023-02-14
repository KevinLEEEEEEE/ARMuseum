using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using TMPro;
using System;

public class OrbButton : MonoBehaviour
{
    public string textBeforeActivation;
    public string textDuringActivation;
    public TextMeshPro textMesh;
    public Image radialIndicatorUI;
    public AudioClip audioClip_startActivation;
    public AudioClip audioClip_stopActivation;
    public Action orbButtonStartEvent;
    public Action orbButtonStopEvent;
    public Action orbButtonFinishEvent;

    private AudioGenerator audioSource_startActivation;
    private AudioGenerator audioSource_stopActivation;
    private const float activationDuration = 2f;
    private float timer;
    private bool isTriggered;
    private string triggerCollider;
    private HandEnum TriggerHand
    {
        get
        {
            if(triggerCollider == "ColliderEntity_IndexTip_R")
            {
                return HandEnum.RightHand;
            } else if (triggerCollider == "ColliderEntity_IndexTip_L")
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
        audioSource_startActivation = new AudioGenerator(gameObject, audioClip_startActivation);
        audioSource_stopActivation = new AudioGenerator(gameObject, audioClip_stopActivation);

        ResetAll();
    }

    private void OnDisable()
    {
        if(isTriggered) ResetAll();
    }

    public void ResetAll()
    {
        timer = 0;
        isTriggered = false;
        triggerCollider = "";
        textMesh.text = textBeforeActivation;
        SetRadialIndicator(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;
        triggerCollider = other.name;
        if(textDuringActivation != "")
        {
            textMesh.text = textDuringActivation;
        }
        audioSource_startActivation.Play();
        orbButtonStartEvent?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[OrbButton] Stop orb button, hand exit trigger area.");
        StopOrbActivation();
    }

    private void StopOrbActivation()
    {
        audioSource_stopActivation.Play();
        orbButtonStopEvent?.Invoke();
        ResetAll();
    }

    private void SetRadialIndicator(float percentage)
    {
        radialIndicatorUI.fillAmount = percentage;
    }

    void Update()
    {
        if(isTriggered)
        {
            if(NRInput.Hands.GetHandState(TriggerHand).currentGesture != HandGesture.Point)
            {
                Debug.Log("[OrbButton] Stop orb button, hand gesture changed");
                StopOrbActivation();
            }

            timer += Time.deltaTime;  
            SetRadialIndicator(timer / activationDuration);

            if(timer >= activationDuration)
            {
                Debug.Log("[Player] Delete exhibit success");
                ResetAll();
                orbButtonFinishEvent?.Invoke();
            }
        }
    }
}
