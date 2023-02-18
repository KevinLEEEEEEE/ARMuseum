using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal.Record;
using System;
using NRKernal;
using System.Linq;
using UnityEngine.UI;
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NRKernal.NRExamples.NativeGalleryDataProvider;
#else
using GalleryDataProvider = NRKernal.NRExamples.MockGalleryDataProvider;
#endif

public class CameraManager : MonoBehaviour
{
    public bool alwaysDetectedInEditor;
    public bool savePhotoToGallery;
    public bool displayCapturedImage;
    public RawImage capturedImageUI;
    public Texture2D[] mockImages_Burning;
    public Texture2D[] mockImages_Others;

    private NRPhotoCapture m_PhotoCaptureObject;
    private Resolution m_CameraResolution;
    //private bool isOnPhotoProcess = false;
    private GalleryDataProvider galleryDataTool;
    private byte[] imageBytes;

    private void Start()
    {
        capturedImageUI.gameObject.SetActive(displayCapturedImage);
        imageBytes = mockImages_Burning[0].EncodeToPNG();
    }

    void Create(Action<NRPhotoCapture> onCreated)
    {
        if (m_PhotoCaptureObject != null)
        {
            NRDebugger.Info("The NRPhotoCapture has already been created.");
            return;
        }

        // Create a PhotoCapture object
        NRPhotoCapture.CreateAsync(false, delegate (NRPhotoCapture captureObject)
        {
            m_CameraResolution = NRPhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

            if (captureObject == null)
            {
                NRDebugger.Error("Can not get a captureObject.");
                return;
            }

            m_PhotoCaptureObject = captureObject;

            CameraParameters cameraParameters = new();
            cameraParameters.cameraResolutionWidth = m_CameraResolution.width;
            cameraParameters.cameraResolutionHeight = m_CameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
            cameraParameters.frameRate = NativeConstants.RECORD_FPS_DEFAULT;
            cameraParameters.blendMode = BlendMode.RGBOnly;

            // Activate the camera
            m_PhotoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (NRPhotoCapture.PhotoCaptureResult result)
            {
                NRDebugger.Info("Start PhotoMode Async");
                if (result.success)
                {
                    onCreated?.Invoke(m_PhotoCaptureObject);
                }
                else
                {
                    //isOnPhotoProcess = false;
                    this.Close();
                    NRDebugger.Error("Start PhotoMode faild." + result.resultType);
                }
            }, true);
        });
    }

    public delegate void OnPhotoCapturedCallback(byte[] bytes, float startTime);

    public void TakeAPhoto(OnPhotoCapturedCallback callback)
    {
        float startTime = Time.time;

        void capturedCallback(byte[] bytes)
        {
            OnCapturedProcessCallback(bytes, callback, startTime);
        }

        // Run mock method in unity editor
        if (!Application.isEditor)
        {
            //int index = UnityEngine.Random.Range(0, alwaysDetectedInEditor ? (mockImages_Burning.Length - 1) : (mockImages_Burning.Length + mockImages_Others.Length - 1));
            //Texture2D tex = index <= (mockImages_Burning.Length - 1) ? mockImages_Burning[index] : mockImages_Others[index - mockImages_Burning.Length];

            //UnityEngine.Debug.Log("[ImageRecognition] Use mock image NO." + (index + 1));

            //capturedCallback(tex.EncodeToPNG());

            capturedCallback(imageBytes);
            return;
        }

        //if (isOnPhotoProcess)
        //{
        //    NRDebugger.Warning("Currently in the process of taking pictures, Can not take photo .");
        //    return;
        //}

        //isOnPhotoProcess = true;

        if (m_PhotoCaptureObject == null)
        {
            this.Create((capture) =>
            {
                capture.TakePhotoAsync(capturedCallback);
            });
        }
        else
        {
            m_PhotoCaptureObject.TakePhotoAsync(capturedCallback);
        }
    }

    private void OnCapturedProcessCallback(byte[] bytes, OnPhotoCapturedCallback callback, float startTime)
    {
        callback(bytes, startTime);
        //this.Close();

        if(savePhotoToGallery)
        {
            SaveBytesToGallery(bytes);
        }

        if (displayCapturedImage)
        {
            Texture2D tex = new(m_CameraResolution.width, m_CameraResolution.height);
            ImageConversion.LoadImage(tex, bytes);

            capturedImageUI.texture = tex;
        }
    }

    public void Close()
    {
        if (m_PhotoCaptureObject == null)
        {
            if(!Application.isEditor) NRDebugger.Error("The NRPhotoCapture has not been created.");
            return;
        }

        m_PhotoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(NRPhotoCapture.PhotoCaptureResult result)
    {
        m_PhotoCaptureObject?.Dispose();
        m_PhotoCaptureObject = null;
        //isOnPhotoProcess = false;
    }

    void OnDestroy()
    {
        m_PhotoCaptureObject?.Dispose();
        m_PhotoCaptureObject = null;
    }

    public void SaveBytesToGallery(byte[] bytes)
    {
        try
        {
            string filename = string.Format("Nreal_Shot_{0}.png", NRTools.GetTimeStamp().ToString());
            NRDebugger.Info(bytes.Length / 1024 + "Kb was saved as: " + filename);
            if (galleryDataTool == null)
            {
                galleryDataTool = new GalleryDataProvider();
            }

            galleryDataTool.InsertImage(bytes, filename, "Screenshots");
        }
        catch (Exception e)
        {
            NRDebugger.Error("Save picture faild!");
            throw e;
        }
    }
}
