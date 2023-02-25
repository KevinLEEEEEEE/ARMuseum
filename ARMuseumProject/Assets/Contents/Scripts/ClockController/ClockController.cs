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

    private HandState rightHandState;
    private HandState leftHandState;
    private const int targetTime = 6523;
    private float currentTime;
    private int currentTimeSpan;
    private const int normalTimeSpan= 240; // 常规速度下走完全程消耗200s
    private const int acceleratedTimeSpan = 12; // 加速状态下走完全程消耗10s
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
        currentTime += Time.deltaTime * (targetTime / currentTimeSpan);

        if (currentTime >= targetTime)
        {
            canRunClock = false;
        }

        clockListener?.Invoke(currentTime, currentTime / targetTime);
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

    public delegate void ClockListener(float time, float percentage);
    public delegate void SpeedModeListener(SpeedMode mode);
    public delegate void StartEventListener();
    public delegate void StopEventListener();
}
