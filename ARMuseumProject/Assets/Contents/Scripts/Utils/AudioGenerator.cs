using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioGenerator
{
    public readonly AudioSource source;
    private readonly float _minVolume;
    private Tween tween;

    public AudioGenerator(GameObject target, AudioClip clip = null, bool isLoop = false, bool playOnAwake = false, float volume = 1, float minVolume = 0)
    {
        source = target.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = isLoop;
        source.playOnAwake = playOnAwake;
        source.volume = volume;
        _minVolume = minVolume;
    }

    public void SetPinch(float pinch)
    {
        source.pitch = pinch;
    }

    public void SetClip(AudioClip clip)
    {
        source.clip = clip;
    }

    public void Play()
    {
        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    public void Stop()
    {
        if (source.isPlaying)
        {
            source.Stop();
        }
    }

    public void Pause()
    {
        if (source.isPlaying)
        {
            source.Pause();
        }
    }

    public float GetVolume()
    {
        return source.volume;
    }

    public void SetVolume(float volume)
    {
        source.volume = GetTargetVolume(volume);
    }

    public Tween SetVolumeInSeconds(float volume, float duration)
    {
        if (tween != null) tween.Kill();

        tween = source.DOFade(GetTargetVolume(volume), duration).OnComplete(() => {
            tween = null;
        });

        return tween;
    }

    public void Unload()
    {
        source.enabled = false;
    }

    public void Reload()
    {
        source.enabled = true;
    }

    private float GetTargetVolume(float volume)
    {
        return volume < _minVolume ? _minVolume : volume;
    }
}