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
        _clockController.speedModeListener += UpdateState;
        _clockController.startEventListener += StartTimer;
        _clockController.stopEventListener += StopTimer;
    }

    private void StartTimer()
    {
        foreach (Particle effect in groundEffects)
        {
            effect.Play();
        }
    }

    private void StopTimer()
    {
        foreach (Particle effect in groundEffects)
        {
            effect.Stop();
        }
    }

    private void UpdateState(SpeedMode mode)
    {
        foreach (Particle effect in groundEffects)
        {
            effect.SetSpeedMode(mode);
        }
    }
}
