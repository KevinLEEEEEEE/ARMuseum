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
            // �Ѿ��ڵ���ģʽ�л����ڽ���ģʽ�л�������ʾOnboarding����
            _FollowLayerScript.HideOnboarding();

            return;
        }

        // ���δ�ڵ���ģʽ��ģʽ�л��У����״ν��룬�������⵽Ŀ��ͼ��ʱ��ʾOnboarding
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

        // ��ֻ�ֱ�ͬʱ��⵽����ͬʱΪ��ȭ״̬ 
        if (RightHandState.isTracked && LeftHandState.isTracked)
        {
            Debug.Log("Both hands detected.");

            if (RightHandState.currentGesture == HandGesture.Grab && LeftHandState.currentGesture == HandGesture.Grab)
            {
                // ��ʱ�����δ����ģʽ�л������У����л�ģʽ
                // ����Ѿ�����ģʽ�л����̣���ֱ��return���������µ�Invoke

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

        // �����ģʽ�л������У���������תΪ�����㣬��ֹͣģʽ�л�
        if (isSwitchingNavMode == true)
        {
            isSwitchingNavMode = false;

            CancelInvoke("SwitchNavMode");

            UpdateSwitchNavModeInfo();
        }

        isInCurrentCycle = false; // �κβ�����˫����ȭ�����������Ϊ���ñ���ѭ��
    }

    private void SwitchNavMode()
    {
        isNavigating = !isNavigating;
        hasNavigated = true;

        isSwitchingNavMode = false;
        isInCurrentCycle = true; // ���ֳɹ��л�ģʽ����ʾ�����Ѿ�ִ�й��л�ģʽ

        UpdateSwitchNavModeInfo();

        if(isNavigating)
        {
            Debug.Log("Start Navigating.");

            FitToImageObj.SetActive(true);

            // չʾ3s�ĵ���Pinch&Select�����߼�
            _FollowLayerScript.ShowPinchOnboarding();
            Invoke("HidePinchOnboarding", 3);

            // ��֪���嵱ǰ�Ѿ����뵼��ģʽ

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
