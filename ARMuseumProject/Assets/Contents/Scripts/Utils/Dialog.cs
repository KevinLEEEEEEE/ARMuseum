using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialog : MonoBehaviour
{
    public AudioClip templeClip;
    public AudioClip[] talkClips;
    public TextMeshProUGUI textMesh;
    public Animation animationPlayer;

    private AudioSource templePlayer;
    private AudioSource talkPlayer;

    public void SetContent(string content)
    {
        textMesh.text = content;
    }

    public void StartDialog()
    {
        templePlayer = gameObject.AddComponent<AudioSource>();
        talkPlayer = gameObject.AddComponent<AudioSource>();

        templePlayer.clip = templeClip;
        talkPlayer.clip = talkClips[Random.Range(0, 3)];

        templePlayer.Play();
        talkPlayer.Play();
        animationPlayer.Play();
    }
}
