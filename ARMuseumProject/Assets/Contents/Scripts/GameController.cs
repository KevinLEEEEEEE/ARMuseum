using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using NRKernal;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [SerializeField] private VideoCapture m_videoCapture;
    [SerializeField] private InstructionGenerator m_InstructionGenerator;
    [SerializeField] private DialogGenerator m_DialogGenerator;
    [SerializeField] private TrackableObserver observer;
    [SerializeField] private Transform observerFollower;
    [SerializeField] AudioClip audioClip_CurrentEntry;

    public event Action FoundObserverEvent;
    public event Action LostObserverEvent;
    public event Action BeginTourEvent;
    public event Action EndTourEvent;

    private AudioGenerator audioSource_CurrentEntry;
    private bool isTracking = false;
    private bool isGrabbing = false;
    private bool isTouring = false;
    private bool firstFound = true;
    private bool firstTour = true;

    void Awake()
    {
        audioSource_CurrentEntry = new AudioGenerator(gameObject, audioClip_CurrentEntry);

        observer.FoundEvent += Found;
        observer.LostEvent += Lost;

        m_videoCapture.StartRecord();
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        observerFollower.SetPositionAndRotation(pos, qua);

        if (!isTracking)
        {
            FoundObserverEvent?.Invoke();
            isTracking = true;
        }

        if (firstFound) FirstFound();
    }

    private async void FirstFound()
    {
        firstFound = false;
        audioSource_CurrentEntry.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(8), ignoreTimeScale: false);

        // 到特定节点仍然未开启展板
        if(firstTour)
        {
            m_InstructionGenerator.GenerateInstruction("开始探索", "激活「开始探索」按钮，查看更多内容");
        }
    }

    private void Lost()
    {
        if(isTracking)
        {
            LostObserverEvent?.Invoke();
            isTracking = false;
        }
    }

    public void ToggleTourState()
    {
        if(!isTouring)
        {
            isTouring = true;
            if (firstTour) 
                FirstTour();
            BeginTourEvent?.Invoke();
        } else
        {
            isTouring = false;
            m_videoCapture.StopRecord();
            EndTourEvent?.Invoke();
        }
    }

    private void FirstTour()
    {
        firstTour = false;
        m_InstructionGenerator.HideInstruction();
    }

    public void GrabStart()
    {
        isGrabbing = true;
    }

    public void GrabEnd()
    {
        isGrabbing = false;
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadSceneAsync("BeginScene");
    }

    private void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        if (isGrabbing || rightHandState.currentGesture == HandGesture.Point || leftHandState.currentGesture == HandGesture.Point)
        {
            NRInput.LaserVisualActive = false;
        } else
        {
            NRInput.LaserVisualActive = true;
        }
    }
}
