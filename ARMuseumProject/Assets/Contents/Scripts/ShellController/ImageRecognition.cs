using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using RestSharp;
using Newtonsoft.Json;
using NRKernal;
using TMPro;
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NRKernal.NRExamples.NativeGalleryDataProvider;
#else
    using GalleryDataProvider = NRKernal.NRExamples.MockGalleryDataProvider;
#endif

public class ImageRecogResult
{
    private readonly bool _isSuccessful;
    private readonly ObjectDetectionResponse _response;
    private readonly float _startTime;

    public ImageRecogResult(bool isSuccessful, ObjectDetectionResponse response, float startTime)
    {
        _isSuccessful = isSuccessful;
        _response = response;
        _startTime = startTime;
    }

    public bool IsSuccessful()
    {
        return _isSuccessful;
    }

    public float GetCostTime()
    {
        return _response.cost_ms;
    }

    public float GetStartTime()
    {
        return _startTime;
    }

    public bool ContainLabel(string label)
    {
        if(_isSuccessful)
        {
            foreach (ObjectArray item in _response.results)
            {
                if (item.label == label)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
public class ObjectDetectionResponse
{
    public int cost_ms;
    public int error_code;
    public ObjectArray[] results;
}
public class ObjectArray
{
    public float confidence;
    public string label;
    public int index;
    public ObjectLocation location;
}
public class ObjectLocation
{
    public float x1;
    public float y1;
    public float x2;
    public float y2;
}

public class ImageRecognition : MonoBehaviour
{
    public OnPhotoAnalysisedCallback onPhotoAnalysisedCallback;
    public CameraManager cameraManager;
    public string easydl_ip;
    public string port;
    public float threshold;
    public bool displayReceivedInfo;
    public TextMeshProUGUI receivedInfoUI;

    private GalleryDataProvider galleryDataTool;
    private RestClient client;

    void Start()
    {
        if (displayReceivedInfo)
        {
            receivedInfoUI.gameObject.SetActive(true);
        }
    }

    public void InitClient(int timeout = 1000)
    {
        client = new(easydl_ip + ":" + port + "?threshold=" + threshold);
        client.Timeout = timeout;
    }

    public void TakePhotoAndAnalysis(OnPhotoAnalysisedCallback callback)
    {
        if (client == null)
        {
            Debug.LogError("[ImageRecognition] Initialize image recognition client first.");
        }

        cameraManager.TakeAPhoto((byte[] bytes, float startTime) =>
        {
            OnPhotoCapturedCallback(bytes, callback, startTime);
        });
    }

    private async void OnPhotoCapturedCallback(byte[] bytes, OnPhotoAnalysisedCallback callback, float startTime)
    {
        ImageRecogResult result = await AnalysisImage(bytes, startTime);

        callback(result);
    }

    public delegate void OnPhotoAnalysisedCallback(ImageRecogResult result);

    private async Task<ImageRecogResult> AnalysisImage(byte[] bytes, float startTime)
    {
        var request = new RestRequest()
            .AddHeader("Content-Type", "image/png")
            .AddParameter("application/octet-stream", bytes, ParameterType.RequestBody);

        IRestResponse response = await client.ExecuteAsync(request, Method.POST);
        ImageRecogResult recogResult;

        if (response.IsSuccessful)
        {
            ObjectDetectionResponse result = JsonConvert.DeserializeObject<ObjectDetectionResponse>(response.Content);
            recogResult = new ImageRecogResult(true, result, startTime);
            UnityEngine.Debug.Log("[ImageRecognition] Receive result successfully.");
        }
        else
        {
            recogResult = new ImageRecogResult(false, null, startTime);
            UnityEngine.Debug.LogError("[ImageRecognition] Receive result failed: " + response.ErrorMessage);
        }

        if(displayReceivedInfo)
        {
            receivedInfoUI.text = string.Format("IsSuccessful: {0},\nStartTime: {1}s,\nRecogDuration: {2}ms,\nIsBurning: {3}.", 
                recogResult.IsSuccessful(), recogResult.GetStartTime().ToString("f3"), recogResult.GetCostTime(), recogResult.ContainLabel("burning"));
        }

        return recogResult;
    }

    public void SaveImageToGallery(byte[] bytes)
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
            NRDebugger.Error("[ImageRecognition] Save picture faild!");
            throw e;
        }
    }
}