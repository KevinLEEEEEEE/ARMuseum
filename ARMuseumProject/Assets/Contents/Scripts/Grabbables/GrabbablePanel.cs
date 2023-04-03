using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbablePanel : MonoBehaviour
{
    [SerializeField] private ExhibitsPanel exhibitsPanel;
    [SerializeField] private GameController gameController;
    [SerializeField] private GrabbableExhibit[] grabbableExhibitsList;
    [SerializeField] private AudioClip grabStartClip;
    [SerializeField] private AudioClip grabEndClip;
    [SerializeField] private AudioClip deleteExhibitClip;

    private AudioGenerator grabStartPlayer;
    private AudioGenerator grabEndPlayer;
    private AudioGenerator deleteExhibitPlayer;

    void Awake()
    {
        grabStartPlayer = new AudioGenerator(gameObject, grabStartClip);
        grabEndPlayer = new AudioGenerator(gameObject, grabEndClip);
        deleteExhibitPlayer = new AudioGenerator(gameObject, deleteExhibitClip);
        gameController.EndTourEvent += Reset;

        foreach (GrabbableExhibit exhibit in grabbableExhibitsList)
        {
            exhibit.grabBeginEvent += GrabBeginEventHandler;
            exhibit.grabEndEvent += GrabEndEventHandler;
        }

        Reset();
    }

    private void Reset()
    {
        foreach(GrabbableExhibit exhibit in grabbableExhibitsList)
        { 
            exhibit.Reset();
        }
    }

    public void EnableGrabbaleExhibit(string id, Transform trans)
    {
        foreach(GrabbableExhibit exhibit in grabbableExhibitsList)
        {
            if(exhibit.exhibitID == id)
            {
                exhibit.EnableGrabbableExhibit();
                exhibit.MoveToDestinationFrom(trans);
                return;
            }
        }
    }

    public void InactiveGrabbleItem()
    {
        deleteExhibitPlayer.Play();
        exhibitsPanel.ResumeExhibitPanel();
    }

    private void GrabBeginEventHandler()
    {
        grabStartPlayer.Play();
        gameController.GrabStart();
    }

    private void GrabEndEventHandler()
    {
        grabEndPlayer.Play();
        gameController.GrabEnd();
    }
}
