using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Particle
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

public class GroundTimer : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private Particle[] groundEffects;

    void Start()
    {
        _clockController.speedModeListener += SpeedModeHandler;
        _clockController.startEventListener += StartEventHandler;
        _clockController.stopEventListener += StopEventHandler;
    }

    private void StartEventHandler()
    {
        for (int i = 0; i < groundEffects.Length; i++)
        {
            groundEffects[i].Play();
        }
    }

    private void StopEventHandler()
    {
        for (int i = 0; i < groundEffects.Length; i++)
        {
            groundEffects[i].Stop();
        }
    }

    private void SpeedModeHandler(SpeedMode mode)
    {
        for (int i = 0; i < groundEffects.Length; i++)
        {
            groundEffects[i].SetSpeedMode(mode);
        }
    }
}
