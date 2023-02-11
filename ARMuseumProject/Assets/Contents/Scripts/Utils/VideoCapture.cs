using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal.NRExamples;

public class VideoCapture : MonoBehaviour
{
    public bool captureOnStart;
    public bool saveOnDestory;
    public bool showPreviewer;
    public GameObject previewer;
    private VideoCapture2LocalExample capture;

    // Start is called before the first frame update
    void Start()
    {
        if (showPreviewer)
        {
            previewer.SetActive(true);
        }

        if (captureOnStart)
        {
            capture.OnClickPlayButton();
        }
    }

    private void OnDestroy()
    {
        if(saveOnDestory)
        {
            capture.OnClickPlayButton();
        }
    }

    public void StopRecord()
    {
        capture.StopVideoCapture();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
