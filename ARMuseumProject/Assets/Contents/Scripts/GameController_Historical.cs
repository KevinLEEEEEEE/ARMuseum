using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameController_Historical : MonoBehaviour
{
    public SceneIndexUpdateEvent sceneIndexUpdateEvent;
    [SerializeField] private VideoCapture _videoCapture;
    [SerializeField] private PlaneDetector _planeDetector;
    [SerializeField] private AudioClip audioClip_ambientWind;
    [SerializeField] private GameObject groundMask;
    [SerializeField] private float ambientBasicVolume;
    [SerializeField] private Light ambientLightComp;
    [SerializeField] private GameObject[] initMessageListener;
    [SerializeField] private Transform[] eventAnchorListener;
    [SerializeField] private Transform[] startPointListener;
    [Header("设置编辑器中启动场景的序号(从0开始)")]
    [SerializeField] private int initializeIndex;
    [Header("设置编辑器中场景的初始位置")]
    [SerializeField] private Vector3 customeAnchorPosition;
    [Header("设置光球的初始位置")]
    [SerializeField] private Vector3 customeStartPoint;
    [Header("录制设置")]
    [SerializeField] private int startIndex;
    [SerializeField] private int stopIndex;
    public string UserID
    {
        get
        {
            if(_userID == null)
            {
                _userID = GetUniqueUserID();
            }

            return _userID;
        }
    }
    private string _userID;
    private AudioGenerator audioSource_ambientWind;

    void Start()
    {
        audioSource_ambientWind = new AudioGenerator(gameObject, audioClip_ambientWind, true, false, 0, 0.3f);
        HideGroundMask();

        PlayerData.Scene3Entry++;

        if(initializeIndex != 0)
        {
            SkipPlaneDetectionStep();
        }

        SetStartPoint(customeStartPoint);
        NextScene();
    }

    private void SkipPlaneDetectionStep()
    {
        SetAnchoredPosition(customeAnchorPosition, new Vector3(0, 0, 10));
        _planeDetector.gameObject.SetActive(false);
    }

    private string GetUniqueUserID()
    {
        DateTime dt = DateTime.Now;
        return string.Format("{0}{1}{2}", dt.Day.ToString("00"), dt.Hour.ToString("00"), dt.Minute.ToString("00"));
    }

    private void SetStartPoint(Vector3 point)
    {
        foreach(Transform trans in startPointListener)
        {
            trans.position = point;
        }
    }

    private void SetAnchoredPosition(Vector3 position, Vector3 forward)
    {
        foreach (Transform trans in eventAnchorListener)
        {
            // 部分元素在编辑器外会被销毁，因此需要检测是否存在
            if(trans)
            {
                trans.position = position;
                trans.forward = forward;
            }
        }
    }

    public void SetEventAnchor(EventAnchor anchor)
    {
        SetAnchoredPosition(anchor.GetCorrectedHitPoint(), anchor.GetHitDirection());
        _planeDetector.LockTargetPlane(anchor.GetHitObject());
    }

    public void NextScene()
    {
        if (initializeIndex == startIndex)
        {
            _videoCapture.StartRecord();
        }
        else if (initializeIndex == stopIndex)
        {
            _videoCapture.StopRecord();
        }

        if (initializeIndex < initMessageListener.Length)
        {
            NRDebugger.Info(string.Format("[GameController] Init scene NO.{0}", initializeIndex + 1));
            sceneIndexUpdateEvent?.Invoke(initializeIndex);
            initMessageListener[initializeIndex++].SendMessage("Init");
        } else
        {
            NRDebugger.Info("[GameController] Game end, return to main scene");
            LoadMainScene();
        }
    }

    private async void LoadMainScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BeginScene");
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            await UniTask.Yield();
        }
    }

    public void StartAmbientSound()
    {
        audioSource_ambientWind.Play();
    }

    public void StopAmbientSound()
    {
        audioSource_ambientWind.Stop();
    }

    public void SetAmbientVolume(float volume)
    {
        audioSource_ambientWind.SetVolume(volume);
    }

    public void SetAmbientLightInSeconds(float intensity, float duration)
    {
        ambientLightComp.DOIntensity(intensity, duration);
    }

    public float GetAmbientLightIntensity()
    {
        return ambientLightComp.intensity;
    }

    public void SetAmbientVolumeInSeconds(float volume, float duration)
    {
        audioSource_ambientWind.SetVolumeInSeconds(volume, duration);
    }

    public void SetEnvLightIntensityInSeconds(float intensity, float duration)
    {
        DOTween.To(() => RenderSettings.ambientIntensity, (t) =>
        {
            RenderSettings.ambientIntensity = t;
        }, intensity, duration);
    }

    public void StartPlaneHint()
    {
        _planeDetector.StartPlaneHint();
    }

    public void StopPlaneHint()
    {
        _planeDetector.StopPlaneHint();
    }

    // 在编辑器中存在桌子，因此无需展示GroundMask
    public void ShowGroundMask()
    {
        if (!Application.isEditor)
        {
            groundMask.SetActive(true);
        }
    }

    public void HideGroundMask()
    {
        groundMask.SetActive(false);
    }

    public delegate void SceneIndexUpdateEvent(int index);
}
