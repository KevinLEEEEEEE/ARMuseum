using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGenerator
{
    public readonly AudioSource source;
    private readonly float _minVolume;

    public AudioGenerator(GameObject target, AudioClip clip = null, bool isLoop = false, bool playOnAwake = false, float volume = 1, float minVolume = 0)
    {
        source = target.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = isLoop;
        source.playOnAwake = playOnAwake;
        source.volume = volume;
        _minVolume = minVolume;
    }

    public void SetClip(AudioClip clip)
    {
        source.clip = clip;
    }

    public void Play()
    {
        if(!source.isPlaying)
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

    public float GetVolume()
    {
        return source.volume;
    }

    public void SetVolume(float volume)
    {
        source.volume = GetTargetVolume(volume);
    }
         
    private float GetTargetVolume(float volume)
    {
        return volume < _minVolume ? _minVolume : volume;
    }
}