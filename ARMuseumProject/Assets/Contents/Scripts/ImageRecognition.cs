using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using RestSharp;
using Newtonsoft.Json;
using NRKernal;
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NRKernal.NRExamples.NativeGalleryDataProvider;
#else
using GalleryDataProvider = NRKernal.NRExamples.MockGalleryDataProvider;
#endif

public class ImageRecogResult
{
    private readonly bool _isValid;
    private readonly bool _hasMatch;
    private readonly float _timestamp;

    public ImageRecogResult(bool isValid, bool hasMatch, float timestamp)
    {
        _isValid = isValid;
        _hasMatch = hasMatch;
        _timestamp = timestamp;
    }

    public bool GetValidation()
    {
        return _isValid;
    }

    public bool GetResult()
    {
        return _hasMatch;
    }

    public float GetTimestamp()
    {
        return _timestamp;
    }
}
public class AccessTokenResponse
{
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public string session_key { get; set; }
    public string access_token { get; set; }
    public string session_secret { get; set; }
}
public class ObjectDetectionResponse
{
    public double log_id { get; set; }
    public ObjectArray[] results { get; set; }
}
public class ObjectArray
{
    public string name { get; set; }
    public double score { get; set; }
    public ObjectLocation location { get; set; }
}
public class ObjectLocation
{
    public int left { get; set; }
    public int top { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class ImageRecognition : MonoBehaviour
{
#if UNITY_EDITOR
    private static bool isInUnityEditor = true;
#else
    private static bool isInUnityEditor = false;
#endif
    public bool alwaysDetected;
    public bool savePhotoToGallery;
    public bool displayPhotoOnScreen;
    public RawImage captureImage;
    public Texture2D[] mockImages;
    private NRRGBCamTexture camTexture { get; set; }
    private GalleryDataProvider galleryDataTool;
    private RestClient client;
    private string accessToken;
    private bool isInitialized
    {
        get { return client != null && accessToken != null && camTexture != null; }
    }
    const string API_KEY = "hVi4vsUIRrzMcRSw5rvl1ecf";
    const string SECRET_KEY = "6lSGZAMnntKMsW9YLGdiW571cbsEB0Gq";

    void Start()
    {
        if (displayPhotoOnScreen)
        {
            captureImage.gameObject.SetActive(true);
        }

        StartCam();
        InitClientService();
    }

    public void StartCam()
    {
        if (camTexture == null)
        {
            camTexture = new NRRGBCamTexture();
        }
        camTexture?.Play();
    }

    public void PauseCam()
    {
        camTexture?.Pause();
    }

    public void StopCam()
    {
        camTexture?.Stop();
        camTexture = null;
    }

    private async void InitClientService()
    {
        client = InitClient();
        accessToken = await GetAccessToken();
    }

    private RestClient InitClient()
    {
        RestClient _client = new("https://aip.baidubce.com/");
        _client.Timeout = 1500;
        return _client;
    }

    public async Task<string> GetAccessToken()
    {
        IRestRequest request = new RestRequest("oauth/2.0/token")
            .AddParameter("grant_type", "client_credentials")
            .AddParameter("client_id", API_KEY)

            .AddParameter("client_secret", SECRET_KEY);

        AccessTokenResponse response = await client.PostAsync<AccessTokenResponse>(request);
        return response.access_token;
    }

    public async Task<ImageRecogResult> TakePhotoAndAnalysis()
    {
        if (!isInitialized)
        {
            Debug.LogError("[Player] Image recognition servive failed to initialize.");
            return new ImageRecogResult(false, false, 0);
        }

        Texture2D tex;

        if (isInUnityEditor)
        {
            int random = UnityEngine.Random.Range(0, alwaysDetected ? 4 : 7);
            int index = random > 5 ? 5 : random;
            Debug.Log("[Player] Use mock image NO." + (index + 1));
            tex = mockImages[index];
        }
        else
        {
            Texture2D texture = camTexture.GetTexture();
            tex = FlipTexture(texture);
        }

        if (displayPhotoOnScreen)
        {
            captureImage.texture = FlipTexture(tex);
        }

        if (savePhotoToGallery)
        {
            SaveTextureToGallery(tex);
        }

        return await AnalysisTexture2D(tex);
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

    private async Task<ImageRecogResult> AnalysisTexture2D(Texture2D tex)
    {
        float timestamp = Time.time;
        string base64 = Convert.ToBase64String(tex.EncodeToPNG());

        var request = new RestRequest("rpc/2.0/ai_custom/v1/detection/matchrecog?access_token=" + accessToken)
            .AddJsonBody(new { image = base64 });

        IRestResponse response = await client.ExecuteAsync(request, Method.POST);

        if(response.IsSuccessful)
        {
            ObjectDetectionResponse result = JsonConvert.DeserializeObject<ObjectDetectionResponse>(response.Content);
            bool hasMatch = result.results.Length > 0;
            return new ImageRecogResult(true, hasMatch, timestamp);
        }else
        {
            return new ImageRecogResult(false, false, timestamp);
        }
    }

    public void SaveTextureToGallery(Texture2D _texture)
    {
        try
        {
            string filename = string.Format("Nreal_Shot_{0}.png", NRTools.GetTimeStamp().ToString());
            byte[] _bytes = _texture.EncodeToPNG();
            NRDebugger.Info(_bytes.Length / 1024 + "Kb was saved as: " + filename);
            if (galleryDataTool == null)
            {
                galleryDataTool = new GalleryDataProvider();
            }

            galleryDataTool.InsertImage(_bytes, filename, "Screenshots");
        }
        catch (Exception e)
        {
            NRDebugger.Error("[TakePicture] Save picture faild!");
            throw e;
        }
    }
}
