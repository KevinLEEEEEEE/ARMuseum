using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController : MonoBehaviour
{
    public TrackableObserver Observer;
    public GameObject TrackingItems;
    public GameObject InsctructionLayer;
    public int ModeSwitchDuration = 2;
    public int PinchInstructionDuration = 2;

    private TrackingItemsController _TrackingItemsController;
    private InstructionController _InstructionController;
    enum GameState
    {
        OnBoarding,
        ModeSwitching,
        ModeSwitched,
        Default,
    }
    private GameState CurrentState = GameState.OnBoarding;
    private bool isNavigating = false;

    void Start()
    {
#if !UNITY_EDITOR
        Destroy(GameObject.Find("EmulatorRoom"));
#endif
        _InstructionController = InsctructionLayer.GetComponent<InstructionController>();
        _TrackingItemsController = TrackingItems.GetComponent<TrackingItemsController>();
        Observer.FoundEvent += Found;
        Observer.LostEvent += Lost;
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        TrackingItems.transform.position = pos;
        TrackingItems.transform.rotation = qua;

        _TrackingItemsController.TargetFound();

        // ���û��״ν������ʱ�������⵽Ŀ��ͼ����չʾģʽ�л��̳�
        if (CurrentState == GameState.OnBoarding)
        {
            _InstructionController.ShowSwitchModeInstruction();
        } else
        {
            _InstructionController.HideSwitchModeInstruction();
        }
    }

    private void Lost()
    {
        _TrackingItemsController.TargetLost();

        _InstructionController.HideSwitchModeInstruction();
    }

    private void CanSwitchMode()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        bool trackingCondition = rightHandState.isTracked;
        bool grabbingCondition = rightHandState.currentGesture == HandGesture.Grab;

#if !UNITY_EDITOR
        trackingCondition = rightHandState.isTracked && leftHandState.isTracked;
        grabbingCondition = rightHandState.currentGesture == HandGesture.Grab && leftHandState.currentGesture == HandGesture.Grab;
#endif
        // ����ģʽ�л�����
        if (trackingCondition && grabbingCondition)
        {
            // ��������л�ģʽ����ģʽ�л��Ѿ���ɣ�ֱ���˳�
            if (CurrentState == GameState.Default || CurrentState == GameState.OnBoarding)
            {
                Debug.Log("[Player] Begin switching mode.");
                CurrentState = GameState.ModeSwitching;
                _InstructionController.ShowSwitchModeProgress();
                Invoke("SwitchMode", ModeSwitchDuration);
            }
            return;
        }

        // �����ģʽ�л������У���������תΪ�����㣬��ֹͣģʽ�л�
        if (CurrentState == GameState.ModeSwitching)
        {
            Debug.Log("[Player] Cancel switching mode.");

            CurrentState = GameState.ModeSwitched;
            _InstructionController.HideSwitchModeProgress();
            CancelInvoke("SwitchMode");

        } else if (CurrentState == GameState.ModeSwitched)
        {
            CurrentState = GameState.Default;
        }
    }

    private void SwitchMode()
    {
        Debug.Log("[Player] Successfully switching mode.");

        CurrentState = GameState.ModeSwitched;
        _InstructionController.ResetAll();

        isNavigating = !isNavigating;

        if (isNavigating == true)
        {
            Debug.Log("[Player] Current navigating mode: start.");

            _InstructionController.ShowPinchGestureInstruction();
            Invoke("HidePinchGestureInstruction", PinchInstructionDuration);
        }
        else
        {
            Debug.Log("[Player] Current navigating mode: end.");

            _TrackingItemsController.StopTracking();
        }
    }

    private void HidePinchGestureInstruction()
    {
        _InstructionController.HidePinchGestureInstruction();

        _TrackingItemsController.StartTracking();
    }

    void Update()
    {
        CanSwitchMode();
    }
}
