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

                await UniTask.Delay(TimeSpan.FromSeconds(0.7), ignoreTimeScale: false);
            }
        }
    }
}
