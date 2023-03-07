using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogGenerator : MonoBehaviour
{
    public GameObject dialog;
    public const float dialogDuration = 6.5f;

    private void Start()
    {
        dialog.SetActive(false);
    }

    public async void GenerateDialog(string content)
    {
        dialog.SetActive(true);
        dialog.GetComponent<Dialog>().StartDialog(content);

        await UniTask.Delay(TimeSpan.FromSeconds(dialogDuration), ignoreTimeScale: false);

        dialog.SetActive(false);
    }
}
