using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Instruction : MonoBehaviour
{
    public Action AnimationEndEventListener;
    [SerializeField] private TextMeshProUGUI ins_title;
    [SerializeField] private TextMeshProUGUI ins_content;
    [SerializeField] private AudioClip audioClip_instructionActive;

    private AudioGenerator audioSource_instructionActive;
    private Animation animatorComp;

    private void Start()
    {
        animatorComp = transform.GetComponent<Animation>();
        audioSource_instructionActive = new AudioGenerator(gameObject, audioClip_instructionActive);
    }

    public void StartInstruction(string title, string content)
    {
        ins_title.text = title;
        ins_content.text = content;
        audioSource_instructionActive.Play();
        animatorComp.Play("InsFadeIn");   
    }

    public void EndInstruction()
    {
        animatorComp.Play("InsFadeOut");
    }

    private void FadeInComplete()
    {
        animatorComp.Play("InsInProgress");
    }

    private void FadeOutComplete()
    {
        AnimationEndEventListener?.Invoke();
    }
}
