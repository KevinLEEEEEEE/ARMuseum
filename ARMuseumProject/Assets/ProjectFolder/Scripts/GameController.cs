using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController : MonoBehaviour
{
    [SerializeField] private TrackableObserver Observer;
    [SerializeField] private GameObject TrackingItems;
    [SerializeField] private TrackingItemsController _TrackingItemsController;
    [SerializeField] private InstructionController _InstructionController;
    [SerializeField] private int ModeSwitchDuration = 2;
    [SerializeField] private int PinchInstructionDuration = 2;

    private enum GameState
    {
        Default,
        OnBoarding,
        ModeSwitching,
        ModeSwitched,
    }
    private GameState CurrentState = GameState.OnBoarding;
    private bool isNavigating = false;
    private bool isTracking = false;

    void Start()
    {
#if !UNITY_EDITOR
        Destroy(GameObject.Find("EmulatorRoom"));
#endif
        Observer.FoundEvent += Found;
        Observer.LostEvent += Lost;
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        TrackingItems.transform.position = pos;
        TrackingItems.transform.rotation = qua;

        if(!isTracking)
        {
            _TrackingItemsController.TrackingImageFound();

            // 仅当用户首次进行模式切换时，给出操作提示
            if (CurrentState == GameState.OnBoarding)
            {
                _InstructionController.ShowSwitchModeInstruction();
            }

            isTracking = true;
        }
    }

    private void Lost()
    {
        if(isTracking)
        {
            _TrackingItemsController.TrackingImageLost();

            if (CurrentState == GameState.OnBoarding)
            {
                _InstructionController.HideSwitchModeInstruction();
            }

            isTracking = false;
        }
    }

    private void SwitchMode()
    { 
        _InstructionController.HideSwitchModeProgress();
        CurrentState = GameState.ModeSwitched;
        isNavigating = !isNavigating;

        Debug.Log("[Player] Successfully switching isNavigating to: " + isNavigating);

        if (isNavigating)
        {
            _InstructionController.ShowPinchGestureInstruction();
            Invoke("HidePinchGestureInstruction", PinchInstructionDuration);
        }
        else
        {
            _TrackingItemsController.StopNavigating();
        }
    }

    private void HidePinchGestureInstruction()
    {
        _InstructionController.HidePinchGestureInstruction();
        _TrackingItemsController.StartNavigating();
    }

    void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        bool trackingCondition = rightHandState.isTracked;
        bool grabbingCondition = rightHandState.currentGesture == HandGesture.Grab;

#if !UNITY_EDITOR
        trackingCondition = rightHandState.isTracked && leftHandState.isTracked;
        grabbingCondition = rightHandState.currentGesture == HandGesture.Grab && leftHandState.currentGesture == HandGesture.Grab;
#endif

        if (trackingCondition && grabbingCondition)
        {
            // 如果正在切换模式或本轮模式切换已经完成，直接退出
            if (CurrentState == GameState.Default || CurrentState == GameState.OnBoarding)
            {
                Debug.Log("[Player] Begin switching mode."); 
                _InstructionController.HideSwitchModeInstruction();
                _InstructionController.ShowSwitchModeProgress();
                CurrentState = GameState.ModeSwitching;
                Invoke("SwitchMode", ModeSwitchDuration);
            }
        } else
        {
            // 如果在模式切换过程中，手势条件转为不满足，则停止模式切换
            if (CurrentState == GameState.ModeSwitching)
            {
                Debug.Log("[Player] Cancel switching mode.");
                _InstructionController.HideSwitchModeProgress();
                CurrentState = GameState.Default;
                CancelInvoke("SwitchMode");
            } else if (CurrentState == GameState.ModeSwitched)
            {
                CurrentState = GameState.Default;
            }
        }
    }
}
