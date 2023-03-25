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
    public SceneIndexUpdateEvent sceneIndexUpdateEvent;

    [SerializeField] private VideoCapture m_videoCapture;
    [SerializeField] private InstructionGenerator m_InstructionGenerator;
    [SerializeField] private DialogGenerator m_DialogGenerator;

    [SerializeField]
    [FormerlySerializedAs("Observer")]
    private TrackableObserver observer;

    [SerializeField]
    [FormerlySerializedAs("TrackingItemList")]
    private Transform[] FollowTrackerList;

    public event Action FoundObserverEvent;
    public event Action LostObserverEvent;
    public event Action StartRaycastEvent;
    public event Action StopRaycastEvent;
    public event Action BeginTourEvent;
    public event Action EndTourEvent;

    private bool isTracking = false;
    private bool isGrabbing = false;
    private bool firstFound = true;
    private bool firstTour = true;

    void Start()
    {
        observer.FoundEvent += Found;
        observer.LostEvent += Lost;

        PlayerData.Scene2Entry++;

        m_videoCapture.StartRecord();
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        foreach(Transform trans in FollowTrackerList)
        {
            trans.SetPositionAndRotation(pos, qua);
        }

        if(!isTracking)
        {
            FoundObserverEvent?.Invoke();
            isTracking = true;
        }

        if (firstFound) FirstFound();
    }

    private async void FirstFound()
    {
        firstFound = false;

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        m_DialogGenerator.GenerateDialog("看向展板...开始探索");
        
    }

    private void Lost()
    {
        if(isTracking)
        {
            LostObserverEvent?.Invoke();
            isTracking = false;
        }
    }

    public void BeginTour()
    {
        BeginTourEvent?.Invoke();

        if(firstTour) FirstTour();
    }

    private async void FirstTour()
    {
        firstTour = false;
        m_InstructionGenerator.HideInstruction();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        m_InstructionGenerator.GenerateInstruction("探索更多", "伸手指向展品，「捏」一下", 10);
    }

    public void EndTour()
    {
        m_videoCapture.StopRecord();
        EndTourEvent?.Invoke();
    }

    public void GrabStart()
    {
        isGrabbing = true;
    }

    public void GrabEnd()
    {
        isGrabbing = false;
    }

    public async void LoadMainScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BeginScene");
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            await UniTask.Yield();
        }
    }

    private void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        bool canRaycast = true;

        // Stop raycast detection when grabbing or pointing
        if (isGrabbing || rightHandState.currentGesture == HandGesture.Point || leftHandState.currentGesture == HandGesture.Point)
        {
            canRaycast = false;
        }

        if(NRInput.LaserVisualActive && !canRaycast)
        {
            StopRaycastEvent?.Invoke();
            NRInput.LaserVisualActive = canRaycast;
        } else if (!NRInput.LaserVisualActive && canRaycast)
        {
            StartRaycastEvent?.Invoke();
            NRInput.LaserVisualActive = canRaycast;
        }
    }

    public delegate void SceneIndexUpdateEvent(int index);
}
