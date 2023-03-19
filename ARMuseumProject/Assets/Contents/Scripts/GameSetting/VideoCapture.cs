using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal.NRExamples;
using NRKernal;

public class VideoCapture : MonoBehaviour
{
    [SerializeField] private VideoCapture2LocalExample capture;
    [Header("录制预览(不要与拍照预览同时启用)")]
    [SerializeField] private bool enableVideoCapture;
    [SerializeField] private bool showPreviewer;
    [SerializeField] private GameObject previewer;

    private bool isRecording;

    void Start()
    {
        capture.gameObject.SetActive(enableVideoCapture);
        previewer.SetActive(showPreviewer);
        isRecording = false;
    }


    public void StartRecord()
    {
        if(enableVideoCapture && !isRecording)
        {
            NRDebugger.Info("[VideoCapture] Start Recording");
            capture.OnClickPlayButton();
            isRecording = true;
        }
    }

    public void StopRecord()
    {
        if(enableVideoCapture && isRecording)
        {
            NRDebugger.Info("[VideoCapture] Stop Recording");
            capture.OnClickPlayButton();
            isRecording = false;
        }
    }
}
