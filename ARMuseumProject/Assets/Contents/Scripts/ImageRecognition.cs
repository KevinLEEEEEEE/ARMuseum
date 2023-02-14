using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using RestSharp;
using Newtonsoft.Json;
using NRKernal;
using System.Diagnostics;
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NRKernal.NRExamples.NativeGalleryDataProvider;
#else
using GalleryDataProvider = NRKernal.NRExamples.MockGalleryDataProvider;
#endif

public class ImageRecogResult
{
    private readonly bool _isSuccessful;
    ObjectDetectionResponse _response;

    public ImageRecogResult(bool isSuccessful, ObjectDetectionResponse response)
    {
        _isSuccessful = isSuccessful;
        _response = response;
    }

    public float GetCostTime()
    {
        return _response.cost_ms;
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
#if UNITY_EDITOR
    private static bool isInUnityEditor = true;
#else
    private static bool isInUnityEditor = false;
#endif
    public string easydl_ip;
    public string port;
    public float threshold;
    public bool alwaysDetected;
    public bool savePhotoToGallery;
    public bool displayPhotoOnScreen;
    public RawImage captureImage;
    public Texture2D[] mockImages;

    private NRRGBCamTexture CamTexture { get; set; }
    private GalleryDataProvider galleryDataTool;
    private RestClient client;
    private bool IsInitialized
    {
        get { return client != null && CamTexture != null; }
    }

    void Start()
    {
        if (displayPhotoOnScreen)
        {
            captureImage.gameObject.SetActive(true);
        }

        StartImageRecogService();

        InvokeRepeating(nameof(Test), 1f, 1f);

        Test();
    }

    public async void Test()
    {
        //int index = UnityEngine.Random.Range(0, alwaysDetected ? 4 : 7);
        //Texture2D tex = mockImages[index];
        //byte[] _bytes = tex.EncodeToPNG();

        Stopwatch sw = new();
        sw.Start();

        ImageRecogResult result = await TakePhotoAndAnalysis();

        sw.Stop();
        UnityEngine.Debug.Log(string.Format("total: {0} ms", sw.ElapsedMilliseconds));

        UnityEngine.Debug.Log(result.GetCostTime());
    }

    public void StartImageRecogService()
    {
        client = new(easydl_ip + ":" + port + "?threshold=" + threshold);
        client.Timeout = 1000;
        StartCam();
    }

    public void StopImageRecogService()
    {
        client = null;
        StopCam();
    }

    private void StartCam()
    {
        if (CamTexture == null) CamTexture = new NRRGBCamTexture();
        CamTexture?.Play();
    }

    private void StopCam()
    {
        CamTexture?.Stop();
        CamTexture = null;
    }

    public async Task<ImageRecogResult> TakePhotoAndAnalysis()
    {
        if (!IsInitialized)
        {
            UnityEngine.Debug.LogError("[ImageRecognition] Initialize image recognition servive first.");
            return new ImageRecogResult(false, null);
        }

        Texture2D tex;

        if (isInUnityEditor)
        {
            int index = UnityEngine.Random.Range(0, alwaysDetected ? 4 : 7);
            tex = mockImages[index];

            UnityEngine.Debug.Log("[ImageRecognition] Use mock image NO." + (index + 1));
        }
        else
        {
            Texture2D texture = CamTexture.GetTexture();
            tex = FlipTexture(texture);
        }

        if (displayPhotoOnScreen)
        {
            captureImage.texture = FlipTexture(tex);
        }

        byte[] _bytes = tex.EncodeToPNG();

        if (savePhotoToGallery)
        {
            SaveTextureToGallery(_bytes);
        }

        return await AnalysisTexture2D(_bytes);
    }

    private Texture2D FlipTexture(Texture2D tex)
    {
        Texture2D flipTexture = new(tex.width, tex.height);
        Color[] pixels = tex.GetPixels();
        Array.Reverse(pixels);
        flipTexture.SetPixels(pixels);
        flipTexture.Apply();
        return flipTexture;
    }

    private async Task<ImageRecogResult> AnalysisTexture2D(byte[] bytes)
    {
        var request = new RestRequest()
            .AddHeader("Content-Type", "image/png")
            .AddParameter("application/octet-stream", bytes, ParameterType.RequestBody);

        IRestResponse response = await client.ExecuteAsync(request, Method.POST);

        if (response.IsSuccessful)
        {
            ObjectDetectionResponse result = JsonConvert.DeserializeObject<ObjectDetectionResponse>(response.Content);
            return new ImageRecogResult(true, result);
        }else
        {
            return new ImageRecogResult(false, null);
        }
    }

    public void SaveTextureToGallery(byte[] bytes)
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
            NRDebugger.Error("[TakePicture] Save picture faild!");
            throw e;
        }
    }
}

//public class AccessTokenResponse
//{
//    public string refresh_token { get; set; }
//    public int expires_in { get; set; }
//    public string scope { get; set; }
//    public string session_key { get; set; }
//    public string access_token { get; set; }
//    public string session_secret { get; set; }
//}

//public async Task<string> GetAccessToken()
//{
//    IRestRequest request = new RestRequest("oauth/2.0/token")
//        .AddParameter("grant_type", "client_credentials")
//        .AddParameter("client_id", API_KEY)

//        .AddParameter("client_secret", SECRET_KEY);

//    AccessTokenResponse response = await client.PostAsync<AccessTokenResponse>(request);
//    return response.access_token;
//}