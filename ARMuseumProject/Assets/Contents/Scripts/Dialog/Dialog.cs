using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog : MonoBehaviour
{
    public AudioClip audioClip_templeBell;
    public AudioClip[] audioClip_chant;
    private AudioGenerator audioSource_templeBell;
    private AudioGenerator audioSource_chant;
    private Animation dialogAnimation;

    private void Awake()
    {
        audioSource_templeBell = new AudioGenerator(gameObject, audioClip_templeBell);
        audioSource_chant = new AudioGenerator(gameObject, audioClip_chant[Random.Range(0, 3)]);
        dialogAnimation = transform.GetComponent<Animation>();
    }

    public void SetContent(string content)
    {
        transform.GetChild(0).GetComponent<Text>().text = content;
    }

    public void StartDialog()
    {
        audioSource_templeBell.Play();
        audioSource_chant.Play();
        dialogAnimation.Play();
    }
}
