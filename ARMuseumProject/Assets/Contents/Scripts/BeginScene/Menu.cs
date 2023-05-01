using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;
using TMPro;
using NRKernal.NRExamples;

public class Menu : MonoBehaviour
{
    [SerializeField] private VideoCapture m_VideoCapture;
    [SerializeField] private string[] overview;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private AudioClip fadeInClip;

    private AudioGenerator fadeInPlayer;
    private Animation animationComp;
    private int currentID = -1;

    private void Awake()
    {
        animationComp = transform.GetComponent<Animation>();
        fadeInPlayer = new AudioGenerator(gameObject, fadeInClip, false, false, 0.6f);
        fadeInPlayer.SetPinch(1.2f);

        textMesh.gameObject.SetActive(false);
    }

    private void Start()
    {
        m_VideoCapture.StartRecord();

        transform.GetComponent<MoveWithCamera>().ResetTransform();
        animationComp.Play("MenuFadeIn");
        fadeInPlayer.Play();
    }

    public void PointEnterEventHandler(int index)
    {
        currentID = index;
        textMesh.gameObject.SetActive(true);
        textMesh.text = overview[index];
    }

    public void PointExitEventHandler(int index)
    {
        if(currentID == index)
        {
            textMesh.gameObject.SetActive(false);
            currentID = index;
        }
    }

    public void LoadScene1()
    {
        LoadScene("CurrentWorldScene");
    }

    public void LoadScene2()
    {
        LoadScene("HistoricalWorldScene");
    }

    public void LoadGestureTutorial()
    {
        LoadScene("GestureTutorial");
    }

    private async void LoadScene(string name)
    {
        animationComp.Play("MenuFadeOut");

        await UniTask.Delay(TimeSpan.FromSeconds(5), ignoreTimeScale: false);

        m_VideoCapture.StopRecord();

        await UniTask.NextFrame();

        SceneManager.LoadScene(name);
    }
}
