using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioGenerator
{
    public readonly AudioSource source;
    private readonly float _minVolume;

    public AudioGenerator(GameObject target, AudioClip clip, bool isLoop = false, bool playOnAwake = false, float volume = 1, float minVolume = 0)
    {
        source = target.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = isLoop;
        source.playOnAwake = playOnAwake;
        source.volume = volume;
        _minVolume = minVolume;
    }

    public void Play()
    {
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public float GetVolume()
    {
        return source.volume;
    }

    public void SetVolume(float volume)
    {
        source.volume = GetTargetVolume(volume);
    }

    //public void SetVolumeInSeconds(float volume, float duration)
    //{

    //}

    //private void AdjustVolume()
    //{

    //}
         
    private float GetTargetVolume(float volume)
    {
        return volume < _minVolume ? _minVolume : volume;
    }
}