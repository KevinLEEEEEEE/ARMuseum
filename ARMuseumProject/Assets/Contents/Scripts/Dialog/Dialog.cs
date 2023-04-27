using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog : MonoBehaviour
{
    public AudioClip audioClip_templeBell;
    public AudioClip[] audioClip_chant;
    public TextMeshProUGUI textMesh;
    private AudioGenerator audioSource_templeBell;
    private AudioGenerator audioSource_chant;
    private Animation dialogAnimation;

    private void Awake()
    {
        audioSource_templeBell = new AudioGenerator(gameObject, audioClip_templeBell);
        audioSource_chant = new AudioGenerator(gameObject);
        dialogAnimation = transform.GetComponent<Animation>();
    }

    public void StartDialog(string content)
    {
        audioSource_templeBell.Stop();
        audioSource_chant.Stop();

        audioSource_chant.SetClip(audioClip_chant[Random.Range(0, 3)]);
        textMesh.text = content;

        audioSource_templeBell.Play();
        audioSource_chant.Play();
        dialogAnimation.Play();
    }
}
