using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioClip UserGuide;
    [SerializeField] private AudioClip ShowExhibits;
    [SerializeField] private AudioClip HoverExhibits;
    [SerializeField] private AudioClip SelectExhibits;
    [SerializeField] private AudioClip GrabStart;
    [SerializeField] private AudioClip GrabEnd;
    [SerializeField] private AudioClip OpenOrb;
    [SerializeField] private AudioClip CloseOrb;
    [SerializeField] private AudioClip DeleteExhibits;
    private AudioSource AudioPlayer;

    public enum Sounds
    {
        UserGuide,
        ShowExhibits,
        HoverExhibits,
        SelectExhibits,
        GrabStart,
        GrabEnd,
        OpenOrb,
        CloseOrb,
        DeleteExhibits
    }

    void Start()
    {
        AudioPlayer = transform.GetComponent<AudioSource>();
    }

    public void PlaySound(Sounds currSound)
    {
        //if(AudioPlayer.isPlaying)
        //{
        //    AudioPlayer.Stop();
        //}

        switch(currSound)
        {
            case Sounds.UserGuide:
                AudioPlayer.clip = UserGuide;
                break;
            case Sounds.ShowExhibits:
                AudioPlayer.clip = ShowExhibits;
                break;
            case Sounds.HoverExhibits:
                AudioPlayer.clip = HoverExhibits;
                break;
            case Sounds.SelectExhibits:
                AudioPlayer.clip = SelectExhibits;
                break;
            case Sounds.GrabStart:
                AudioPlayer.clip = GrabStart;
                break;
            case Sounds.GrabEnd:
                AudioPlayer.clip = GrabEnd;
                break;
            case Sounds.OpenOrb:
                AudioPlayer.clip = OpenOrb;
                break;
            case Sounds.CloseOrb:
                AudioPlayer.clip = CloseOrb;
                break;
            case Sounds.DeleteExhibits:
                AudioPlayer.clip = DeleteExhibits;
                break;
            default:
                AudioPlayer.clip = null;
                break;
        }

        AudioPlayer.Play();
    }

    public void StopSounds()
    {
        AudioPlayer.Stop();
        AudioPlayer.clip = null;
    }

}
