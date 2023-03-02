using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TimeNode
{
    Start,
    Stop,
    Load,
    Unload,
    none,
}

[Serializable]
public class ClockAudio
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float normalVolume;
    [SerializeField] private float acceleratedVolume;
    [SerializeField] private float normalPinch;
    [SerializeField] private float acceleratedPinch;
    [SerializeField] private TimeNode playAt;
    [SerializeField] private TimeNode stopAt;
    [SerializeField] private bool isLoop;
    private AudioSource _audioSource;

    public void Trigger(TimeNode node)
    {
        if(node == playAt)
        {
            _audioSource.Play();
            _audioSource.DOFade(normalVolume, fadeInDuration);
        } else if(node == stopAt)
        {
            _audioSource.DOFade(0, fadeOutDuration).OnComplete(_audioSource.Stop);
        }
    }

    public void SetSpeedMode(SpeedMode mode)
    {
        if (!_audioSource.isPlaying) return;

        if(mode == SpeedMode.Normal)
        {
            _audioSource.volume = normalVolume;
            _audioSource.pitch = normalPinch;
        } else
        {
            _audioSource.volume = acceleratedVolume;
            _audioSource.pitch = acceleratedPinch;
        }
    }

    public void BindAudioSource(AudioSource source)
    {
        _audioSource = source;

        _audioSource.clip = clip;
        _audioSource.volume = 0;
        _audioSource.pitch = normalPinch;
        _audioSource.loop = isLoop;  
    }
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private ClockAudio[] clockAudios;

    void Start()
    {
        _clockController.speedModeListener += SpeedModeHandler;
        _clockController.startEventListener += StartEventHandler;
        _clockController.stopEventListener += StopEventHandler;
        _clockController.unloadEventListener += UnloadEventHandler;

        GenerateAudio();
    }

    private void GenerateAudio()
    {
        for (int i = 0; i < clockAudios.Length; i++)
        {
            clockAudios[i].BindAudioSource(gameObject.AddComponent<AudioSource>());
        }
    }

    private void UnloadEventHandler()
    {
        for (int i = 0; i < clockAudios.Length; i++)
        {
            clockAudios[i].Trigger(TimeNode.Unload);
        }
    }

    private void StartEventHandler()
    {
        for (int i = 0; i < clockAudios.Length; i++)
        {
            clockAudios[i].Trigger(TimeNode.Start);
        }
    }

    private void StopEventHandler()
    {
        for (int i = 0; i < clockAudios.Length; i++)
        {
            clockAudios[i].Trigger(TimeNode.Stop);
        }
    }

    private void SpeedModeHandler(SpeedMode mode)
    {
        for (int i = 0; i < clockAudios.Length; i++)
        {
            clockAudios[i].SetSpeedMode(mode);
        }
    }
}
