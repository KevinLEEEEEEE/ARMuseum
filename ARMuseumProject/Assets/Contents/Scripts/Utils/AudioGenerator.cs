using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioGenerator
{
    public readonly AudioSource source;
    private readonly float _minVolume;
    private bool isOccupied;

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

    public void SetVolume(float volume)
    {
        source.volume = GetTargetVolume(volume);
    }

    public async void SetVolumeInSeconds(float volume, float duration)
    {
        if(isOccupied)
        {
            return;
        }

        isOccupied = true;
        Debug.Log("[AudioGenerator] Start change volume.");

        duration.Tweeng((vol) =>
        {
            source.volume = vol;
        }, source.volume, GetTargetVolume(volume));

        await Task.Delay(System.TimeSpan.FromSeconds(duration));

        isOccupied = false;

        Debug.Log("[AudioGenerator] Change volume successfully in " + duration + " seconds.");
    }

    private float GetTargetVolume(float volume)
    {
        return volume < _minVolume ? _minVolume : volume;
    }
}