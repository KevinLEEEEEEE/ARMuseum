using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShellAudio
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume;
    [SerializeField] private ShellNode playAt;
    [SerializeField] private ShellNode stopAt;
    [SerializeField] private ShellNode unloadAt;
    [SerializeField] private bool isLoop;
    private AudioSource _audioSource;

    public void Trigger(ShellNode node)
    {
        if (node == playAt)
        {
            _audioSource.Play();
        }
        else if (node == stopAt)
        {
            _audioSource.Stop();
        } else if(node == unloadAt)
        {
            _audioSource.enabled = false;
        }
    }

    public void BindAudioSource(AudioSource source)
    {
        _audioSource = source;

        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.loop = isLoop;
    }
}

public class ShellSoundManager : MonoBehaviour
{
    [SerializeField] private ShellController _shellController;
    [SerializeField] private ShellAudio[] shellAudios;

    void Start()
    {
        _shellController.shellStateListener += ShellStateHandler;

        GenerateAudio();
    }

    private void GenerateAudio()
    {
        for (int i = 0; i < shellAudios.Length; i++)
        {
            shellAudios[i].BindAudioSource(gameObject.AddComponent<AudioSource>());
        }
    }

    private void ShellStateHandler(ShellNode node)
    {
        for (int i = 0; i < shellAudios.Length; i++)
        {
            shellAudios[i].Trigger(node);
        }
    }
}
