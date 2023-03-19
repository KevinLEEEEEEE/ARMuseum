using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal.NRExamples;
using NRKernal;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;

public class Menu : MonoBehaviour
{
    [SerializeField] InstructionGenerator m_InstructionGenerator;
    [SerializeField] InteractionHint m_InteractionHint;
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private Transform[] trailTarget;

    private AudioGenerator buttonHoverAudio;
    private AudioGenerator buttonClickAudio;
    private string targetSceneName;
    private Action instructionHandler;

    private void Start()
    {
        buttonHoverAudio = new AudioGenerator(gameObject, buttonHoverClip);
        buttonClickAudio = new AudioGenerator(gameObject, buttonClickClip);

        PlayerData.Scene1Entry++;
    }

    public void LoadScene1()
    {
        ButtonClick();
        LoadScene("CurrentWorldScene");
    }

    public void LoadScene2()
    {
        ButtonClick();
        LoadScene("HistoricalWorldScene");
    }

    public void OfflineMode()
    {
        ButtonClick();
        NRDebugger.Info("[Menu] Offline mode is under construction");
    }

    public void AdvancedSetting()
    {
        ButtonClick();
        NRDebugger.Info("[Menu] Advanced setting is under construction");
    }

    public void PlayButtonHoverAudio()
    {
        buttonHoverAudio.Play();
    }

    public void PlayButtonClickAudio()
    {
        buttonClickAudio.Play();
    }

    private void ButtonClick()
    {
        PlayButtonClickAudio();
        StopInstruction(); 
    }

    private void LockPosition()
    {
        NRDebugger.Info("[Menu] Menu position locked");
        transform.GetComponentInParent<MoveWithCamera>().enabled = false;
    }

    private void FadeInComplete()
    {
        StartInstruction();
    }

    private void FadeOutComplete()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    private void LoadScene(string name)
    {
        targetSceneName = name;
        transform.GetComponent<Animation>().Play("MenuFadeOut");
    }

    private void StartInstruction()
    {
        if (PlayerData.Scene1Entry == 1)
        {
            instructionHandler = m_InstructionGenerator.GenerateInstruction("选择章节", "用手势进入对应章节");
            m_InteractionHint.StartHintLoop();
        }
    }

    private void StopInstruction()
    {
        if (PlayerData.Scene1Entry == 1)
        {
            m_InteractionHint.StopHintLoop();
        }

        if (instructionHandler != null) instructionHandler();
    }
}
