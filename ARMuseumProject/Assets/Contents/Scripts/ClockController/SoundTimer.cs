using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private bool loop;
    [SerializeField] private bool once;
    private AudioSource _audioSource;

    public void Play()
    {
        _audioSource.Play();
        _audioSource.DOFade(normalVolume, fadeInDuration);
    }

    public void Stop()
    {
        if (once) return;

        _audioSource.DOFade(0, fadeOutDuration).OnComplete(_audioSource.Stop);
    }

    public void SetSpeedMode(SpeedMode mode)
    {
        if (once) return;

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
        _audioSource.loop = loop;
    }
}

public class SoundTimer : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private ClockAudio[] clockAudios;

    void Start()
    {
        _clockController.speedModeListener += UpdateSpeedMode;
        _clockController.startEventListener += StartTimer;
        _clockController.stopEventListener += StopTimer;

        GenerateAudio();
    }

    private void GenerateAudio()
    {
        foreach (ClockAudio audio in clockAudios)
        {
            audio.BindAudioSource(gameObject.AddComponent<AudioSource>());
        }
    }

    private void StartTimer()
    {
        foreach (ClockAudio audio in clockAudios)
        {
            audio.Play();
        }
    }

    private void StopTimer()
    {
        foreach (ClockAudio audio in clockAudios)
        {
            audio.Stop();
        }
    }

    private void UpdateSpeedMode(SpeedMode mode)
    {
        foreach (ClockAudio audio in clockAudios)
        {
            audio.SetSpeedMode(mode);
        }
    }
}
