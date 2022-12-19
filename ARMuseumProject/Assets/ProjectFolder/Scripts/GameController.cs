using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController : MonoBehaviour
{
    public TrackableObserver Observer;
    public GameObject FollowLayerController;
    public GameObject CornerObjController;
    public GameObject FitToImageObj;

    private bool isNavigating = true;
    private bool hasNavigated = false;
    private bool isSwitchingNavMode = false;
    private bool isInCurrentCycle = false;

    private FollowLayerManager _FollowLayerScript;
    private CornerObjController _CornerObjController;

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        Destroy(GameObject.Find("EmulatorRoom"));
        isNavigating = false;
#endif
        _FollowLayerScript = FollowLayerController.GetComponent<FollowLayerManager>();
        _CornerObjController = CornerObjController.GetComponent<CornerObjController>();
        Observer.FoundEvent += Found;
        Observer.LostEvent += Lost;
    }
    private void Found(Vector3 pos, Quaternion qua)
    {
        FitToImageObj.transform.position = pos;
        FitToImageObj.transform.rotation = qua;
        SwitchUserOnBoarding(true);
    }

    private void Lost()
    {
        FitToImageObj.SetActive(false);
        SwitchUserOnBoarding(false);
    }

    private void SwitchUserOnBoarding(bool isTracking)
    {
        if (isNavigating == true || isSwitchingNavMode == true)
        {
            // 已经在导览模式中或正在进行模式切换，不显示Onboarding内容
            _FollowLayerScript.HideOnboarding();

            return;
        }

        // 如果未在导览模式或模式切换中，且首次进入，则仅当检测到目标图像时提示Onboarding
        if (isTracking == true && hasNavigated == false)
        {
            _FollowLayerScript.ShowOnboarding();
        } else
        {
            _FollowLayerScript.HideOnboarding();
        }
    }

    private void CanModeSwitch()
    {
        HandState RightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState LeftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        // 两只手被同时检测到，且同时为握拳状态 
        if (RightHandState.isTracked && LeftHandState.isTracked)
        {
            Debug.Log("Both hands detected.");

            if (RightHandState.currentGesture == HandGesture.Grab && LeftHandState.currentGesture == HandGesture.Grab)
            {
                // 此时，如果未处于模式切换过程中，则切换模式
                // 如果已经处于模式切换过程，则直接return，不建立新的Invoke

                if(isSwitchingNavMode == true || isInCurrentCycle == true)
                {
                    return;
                }

                Debug.Log("Both hands Grabbing, start switching mode.");

                isSwitchingNavMode = true;
                
                Invoke("SwitchNavMode", 3);

                UpdateSwitchNavModeInfo();

                return;
            }
        }

        // 如果在模式切换过程中，手势条件转为不满足，则停止模式切换
        if (isSwitchingNavMode == true)
        {
            isSwitchingNavMode = false;

            CancelInvoke("SwitchNavMode");

            UpdateSwitchNavModeInfo();
        }

        isInCurrentCycle = false; // 任何不满足双手握拳的情况都被视为重置本轮循环
    }

    private void SwitchNavMode()
    {
        isNavigating = !isNavigating;
        hasNavigated = true;

        isSwitchingNavMode = false;
        isInCurrentCycle = true; // 本轮成功切换模式后，提示本轮已经执行过切换模式

        UpdateSwitchNavModeInfo();

        if(isNavigating)
        {
            Debug.Log("Start Navigating.");

            FitToImageObj.SetActive(true);

            // 展示3s的单手Pinch&Select操作逻辑
            _FollowLayerScript.ShowPinchOnboarding();
            Invoke("HidePinchOnboarding", 3);

            // 告知窗体当前已经进入导览模式

            _CornerObjController.StartNavigating();
        }
        else
        {
            Debug.Log("Stop Navigating.");

            FitToImageObj.SetActive(false);

            _CornerObjController.StopNavigating();
        }
    }

    private void HidePinchOnboarding()
    {
        _FollowLayerScript.HidePinchOnboarding();
    }

    private void UpdateSwitchNavModeInfo()
    {
        if (isSwitchingNavMode == true)
        {
            _FollowLayerScript.ShowSwitchModeProgress();
        } else
        {
            _FollowLayerScript.HideSwitchModeProgress();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CanModeSwitch();
    }
}
