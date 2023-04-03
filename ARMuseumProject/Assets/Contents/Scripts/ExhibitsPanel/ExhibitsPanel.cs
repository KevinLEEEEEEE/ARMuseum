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
    [SerializeField] private Frame frame;
    [SerializeField] private AudioClip enableExhibitClip;
    [SerializeField] private AudioClip hoverExhibitClip;
    [SerializeField] private AudioClip clickExhibitClip;
    [SerializeField] private Exhibit[] exhibitsList;

    private AudioGenerator enableExhibitPlayer;
    private AudioGenerator hoverExhibitPlayer;
    private AudioGenerator clickExhibitPlayer;

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
        frame.ActiveFrames();
        enableExhibitPlayer.Play();
    }

    private void EndTourEventHandler()
    {
        DisableExhibits();
        frame.InactiveFrames();
    }

    private void HoverExhibitEventHandler()
    {
        hoverExhibitPlayer.Play();
    }

    private void ClickExhibitEventHandler(string id, Transform trans)
    {
        DisableExhibits();
        clickExhibitPlayer.Play();
        m_GrabbablePanel.EnableGrabbaleExhibit(id, trans);
    }

    public void ResumeExhibitPanel()
    {
        EnableExhibits();
    }
}
