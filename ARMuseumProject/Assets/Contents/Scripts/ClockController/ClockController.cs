using Cysharp.Threading.Tasks;
using NRKernal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum SpeedMode
{
    Normal,
    Accelerated,
}

public class ClockController : MonoBehaviour
{
    public StartEventListener startEventListener;
    public StopEventListener stopEventListener;
    public ClockListener clockListener;
    public SpeedModeListener speedModeListener;
    [SerializeField] private int startDate;
    [SerializeField] private int endDate;

    private HandState rightHandState;
    private HandState leftHandState;
    private int dateDuration;
    private float currentTime;
    private int currentTimeSpan;
    private const int normalTimeSpan= 300; // 常规速度下走完全程所需时间(秒)
    private const int acceleratedTimeSpan = 15; // 加速状态下走完全程所需时间(秒)
    private bool canRunClock;

    void Start()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        Reset();

        RunClock();
    }

    public void Reset()
    {
        canRunClock = true;
        dateDuration = endDate - startDate;
        currentTime = 0;
        currentTimeSpan = normalTimeSpan;
    }

    private async void RunClock()
    {
        startEventListener?.Invoke();

        while(canRunClock)
        {
            UpdateTime();
            UpdateSpeed();

            await UniTask.NextFrame();
        }

        speedModeListener?.Invoke(SpeedMode.Normal);
        stopEventListener?.Invoke();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * (dateDuration / currentTimeSpan);

        if (currentTime >= dateDuration)
        {
            canRunClock = false;
        }

        int date = Mathf.FloorToInt(currentTime + startDate);
        float progress = currentTime / dateDuration;

        clockListener?.Invoke(Mathf.Min(date, endDate), Mathf.Min(progress, 1));
    }

    private void UpdateSpeed()
    {
        bool accelerateCondition = rightHandState.currentGesture == HandGesture.Grab || leftHandState.currentGesture == HandGesture.Grab;

        if(accelerateCondition)
        {
            if(currentTimeSpan != acceleratedTimeSpan)
            {
                currentTimeSpan = acceleratedTimeSpan;
                speedModeListener?.Invoke(SpeedMode.Accelerated);
            }  
        } else
        {
            if(currentTimeSpan != normalTimeSpan)
            {
                currentTimeSpan = normalTimeSpan;
                speedModeListener?.Invoke(SpeedMode.Normal);
            } 
        }
    }

    public delegate void ClockListener(int date, float progress);
    public delegate void SpeedModeListener(SpeedMode mode);
    public delegate void StartEventListener();
    public delegate void StopEventListener();
}
