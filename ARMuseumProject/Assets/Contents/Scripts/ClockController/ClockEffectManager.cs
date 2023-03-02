using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClockEffect
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private int normalSpeed;
    [SerializeField] private int acceleratedSpeed;

    public void Play()
    {
        particle.Play();
    }

    public void Stop()
    {
        particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    public void SetSpeedMode(SpeedMode mode)
    {
        var main = particle.main;

        if (mode == SpeedMode.Normal)
        {
            main.simulationSpeed = normalSpeed;
        }
        else
        {
            main.simulationSpeed = acceleratedSpeed;
        }
    }
}

public class ClockEffectManager : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private ClockEffect[] clockEffects;

    void Start()
    {
        _clockController.speedModeListener += SpeedModeHandler;
        _clockController.loadEventListener += LoadEventHandler;
        _clockController.stopEventListener += StopEventHandler;
    }

    private void LoadEventHandler()
    {
        for (int i = 0; i < clockEffects.Length; i++)
        {
            clockEffects[i].Play();
        }
    }

    private void StopEventHandler()
    {
        for (int i = 0; i < clockEffects.Length; i++)
        {
            clockEffects[i].Stop();
        }
    }

    private void SpeedModeHandler(SpeedMode mode)
    {
        for (int i = 0; i < clockEffects.Length; i++)
        {
            clockEffects[i].SetSpeedMode(mode);
        }
    }
}
