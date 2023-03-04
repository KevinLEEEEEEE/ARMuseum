using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class DebugSetting : MonoBehaviour
{
    [SerializeField] private LogLevel logLevel;

    [SerializeField] private bool showFPS;
    [SerializeField] private TextMeshProUGUI FPSTextComp;

    [SerializeField] private bool showCapturedImage;
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private RawImage capturedImageComp;

    [SerializeField] private bool showImageRecogResult;
    [SerializeField] private ShellController _shellController;
    [SerializeField] private TextMeshProUGUI RecogResultTextComp;

    const float fpsMeasurePeriod = 0.5f; //固定时间为0.5s
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    private int imageCaptureCount = 0;

    void Start()
    {
        NRDebugger.logLevel = logLevel;

        FPSTextComp.gameObject.SetActive(showFPS);
        capturedImageComp.gameObject.SetActive(showCapturedImage);
        RecogResultTextComp.gameObject.SetActive(showImageRecogResult);

        if(showFPS)
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        if (showCapturedImage)
        {
            _cameraManager.imageCapturedEventListener += CapturedImageEventHandler;
        }

        if (showImageRecogResult)
        {
            _shellController.imageRecogResultListener += ImageRecogResultHandler;
        }
    }

    private void ImageRecogResultHandler(ImageRecogResult res)
    {
        RecogResultTextComp.text = string.Format("IsSuccessful: {0},\nCost: {1}ms,\nIsBurning: {2},\nCount: {3}.",
            res.IsSuccessful(), res.GetCostTime(), res.ContainLabel("burning"), imageCaptureCount++);
    }

    private void CapturedImageEventHandler(byte[] bytes, int width, int height)
    {
        Texture2D tex = new(width, height);
        ImageConversion.LoadImage(tex, bytes);

        capturedImageComp.texture = tex;
    }

    private void Update()
    {
        if(showFPS)
        {
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
                FPSTextComp.text = string.Format("{0} FPS", m_CurrentFps);
            }
        }
    }
}
