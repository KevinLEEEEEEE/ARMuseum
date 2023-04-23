using Cysharp.Threading.Tasks;
using NRKernal.NRExamples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogGenerator : MonoBehaviour
{
    public GameObject dialog;
    public const float dialogDuration = 6.5f;

    private MoveWithCamera m_MoveWithCamera;
    private bool isPlaying;

    private void Start()
    {
        m_MoveWithCamera = transform.GetComponent<MoveWithCamera>();

        Reset();
    }

    private void Reset()
    {
        isPlaying = false;
        m_MoveWithCamera.enabled = false;
        dialog.SetActive(false);
    }

    public async void GenerateDialog(string content)
    {
        if (isPlaying)
            return;

        isPlaying = true;
        m_MoveWithCamera.enabled = true;
        m_MoveWithCamera.ResetTransform();
        dialog.SetActive(true);
        dialog.GetComponent<Dialog>().StartDialog(content);

        await UniTask.Delay(TimeSpan.FromSeconds(dialogDuration), ignoreTimeScale: false);

        Reset();
    }
}
