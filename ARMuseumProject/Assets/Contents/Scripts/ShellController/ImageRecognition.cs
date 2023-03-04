using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Newtonsoft.Json;
using NRKernal;
using System.Net;
using System.IO;
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
    [SerializeField] private OnPhotoAnalysisedCallback onPhotoAnalysisedCallback;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private string easydl_ip;
    [SerializeField] private string port;
    [SerializeField] private float threshold;
    [SerializeField] private int timeout;

    private GalleryDataProvider galleryDataTool;
    private Uri uri;

    void Start()
    {
        uri = new(easydl_ip + ":" + port + "?threshold=" + threshold);
    }

    public void TakePhotoAndAnalysis(OnPhotoAnalysisedCallback callback)
    {
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
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Timeout = timeout;

            Stream stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            WebResponse response = await request.GetResponseAsync();
            StreamReader sr = new(response.GetResponseStream());
            JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            ObjectDetectionResponse result = serializer.Deserialize<ObjectDetectionResponse>(reader);
            sr.Close();
            response.Close();
            NRDebugger.Info("[ImageRecognition] Receive result successfully.");
            return new ImageRecogResult(true, result, startTime);
        }
        catch (WebException e)
        {
            NRDebugger.Error("[ImageRecognition] Web exception raised: " + e.Message);
            return new ImageRecogResult(false, null, startTime);
        }
        catch (Exception e)
        {
            NRDebugger.Error("[ImageRecognition] Exception raised: " + e.Message);
            return new ImageRecogResult(false, null, startTime);
        }
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