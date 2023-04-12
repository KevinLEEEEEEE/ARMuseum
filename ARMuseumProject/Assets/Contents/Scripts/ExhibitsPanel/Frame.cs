using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using Cysharp.Threading.Tasks;

public enum FrameState
{
    Disabled,
    Enabled,
    Hightlighted,
}

public class Frame : MonoBehaviour
{
    [SerializeField] private GameController m_GameController;
    [SerializeField] private GameObject[] frames;

    //private string currentExhibitID;
    private FrameState state;

    private void Awake()
    {
        m_GameController.FoundObserverEvent += FoundObserverEventHandler;

        Reset();
    }

    private void Reset()
    {
        state = FrameState.Disabled;

        foreach (GameObject obj in frames)
        {
            obj.SetActive(false);
        }
    }

    private async void FoundObserverEventHandler()
    {
        if(state == FrameState.Disabled)
        {
            state = FrameState.Enabled;

            foreach (GameObject obj in frames)
            {
                obj.SetActive(true);
                obj.GetComponent<Animator>().Play("ShowFrame");

                await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false);
            }
        }
    }

    //public void HoverExhibit(string id)
    //{
    //    currentExhibitID = id;

    //    if (state == FrameState.Enabled)
    //    {  
    //        state = FrameState.Hightlighted;

    //        Debug.Log("hover: " + id);

    //        foreach (GameObject obj in frames)
    //        {
    //            obj.GetComponent<Animator>().SetFloat("Speed", 1);
    //            obj.GetComponent<Animator>().Play("HightlightFrame");
    //        }
    //    }
    //}

    //public void ExitExhibit(string id)
    //{
    //    if(state == FrameState.Hightlighted && id == currentExhibitID)
    //    {
    //        state = FrameState.Enabled;

    //        Debug.Log("exit: " + id);

    //        foreach (GameObject obj in frames)
    //        {
    //            obj.GetComponent<Animator>().SetFloat("Speed", -1);
    //            obj.GetComponent<Animator>().Play("HightlightFrame");
    //        }
    //    }
    //}
}
