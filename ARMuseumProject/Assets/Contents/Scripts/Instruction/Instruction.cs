using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Instruction : MonoBehaviour
{
    public ParticleSystem particle;
    public TextMeshProUGUI ins_title;
    public TextMeshProUGUI ins_content;
    public AudioClip audioClip_instructionActive;

    private AudioGenerator audioSource_instructionActive;
    private Animation instructionAnimation;

    private void Awake()
    {
        instructionAnimation = transform.GetComponent<Animation>();
        audioSource_instructionActive = new AudioGenerator(gameObject, audioClip_instructionActive);
    }

    private void Start()
    {
        StartInstruction();
    }

    public void SetContent(string title, string content)
    {
        ins_title.text = title;
        ins_content.text = content;
    }

    public void StartInstruction()
    {
        audioSource_instructionActive.Play();
        particle.Play();
        instructionAnimation.Play("InstructionFadeIn");
    }

    public void EndInstruction()
    {
        StartCoroutine(nameof(FadeOut));
    }

    private IEnumerator FadeOut()
    {
        instructionAnimation.Play("InstructionFadeOut");

        yield return new WaitForSeconds(2f);

        GameObject.Destroy(gameObject);
    }
}
