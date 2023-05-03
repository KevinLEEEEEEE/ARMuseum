using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ExhibitsPanel : MonoBehaviour
{
    [SerializeField] private GameController m_GameController;
    [SerializeField] private GrabbablePanel m_GrabbablePanel;
    [SerializeField] private InstructionGenerator m_InstructionGenerator;
    [SerializeField] private Description m_Description;
    [SerializeField] private AudioClip enableExhibitClip;
    [SerializeField] private AudioClip hoverExhibitClip;
    [SerializeField] private AudioClip clickExhibitClip;
    [SerializeField] private Exhibit[] exhibitsList;
    [SerializeField] private Vector3 rootPosition;

    private AudioGenerator enableExhibitPlayer;
    private AudioGenerator hoverExhibitPlayer;
    private AudioGenerator clickExhibitPlayer;

    private bool firstSelectExhibit = true;

    void Awake()
    {
        m_GameController.BeginTourEvent += BeginTourEventHandler;
        m_GameController.EndTourEvent += EndTourEventHandler;

        enableExhibitPlayer = new AudioGenerator(gameObject, enableExhibitClip);
        hoverExhibitPlayer = new AudioGenerator(gameObject, hoverExhibitClip);
        clickExhibitPlayer = new AudioGenerator(gameObject, clickExhibitClip);

        foreach (Exhibit exhibit in exhibitsList)
        {
            exhibit.hoverExhibitEvent += HoverExhibitEventHandler;
            exhibit.clickExhibitEvent += ClickExhibitEventHandler;
            exhibit.exitExhibitEvent += ExitExhibitEventHandler;
        }

        if(!Application.isEditor)
        {
            transform.GetChild(0).localPosition = rootPosition;
        }
    }

    private async void EnableExhibits()
    {
        foreach (Exhibit exhibit in exhibitsList)
        {
            exhibit.EnableExhibit();

            await UniTask.Delay(TimeSpan.FromSeconds(0.6), ignoreTimeScale: false);
        }
    }

    private void DisableExhibits()
    {
        foreach (Exhibit exhibit in exhibitsList)
        {
            exhibit.DisableExhibit();
        }
    }

    private void BeginTourEventHandler()
    {  
        EnableExhibits();
        enableExhibitPlayer.Play();
    }

    private void EndTourEventHandler()
    {
        DisableExhibits();
    }

    private void HoverExhibitEventHandler(string exhibitID)
    { 
        m_Description.HoverExhibit(exhibitID);
        hoverExhibitPlayer.Play();
    }

    private void ExitExhibitEventHandler(string exhibitID)
    {
        m_Description.ExitExhibit(exhibitID);
    }

    private async void ClickExhibitEventHandler(string id, Transform trans)
    {
        DisableExhibits();
        clickExhibitPlayer.Play();
        m_GrabbablePanel.EnableGrabbaleExhibit(id, trans);

        if (firstSelectExhibit)
        {
            firstSelectExhibit = false;

            await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

            m_InstructionGenerator.GenerateInstruction("捏住并移动展品", "拇指与食指捏住展品，可进一步探索", 12);
        }
    }

    public void ResumeExhibitPanel()
    {
        EnableExhibits();
    }
}
