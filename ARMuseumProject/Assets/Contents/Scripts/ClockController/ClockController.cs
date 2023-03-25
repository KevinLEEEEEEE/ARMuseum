using Cysharp.Threading.Tasks;
using NRKernal;
using System;
using UnityEngine;

public enum SpeedMode
{
    Normal,
    Accelerated,
}

public class ClockController : MonoBehaviour
{
    public DateMessageListener dateMessageListener;
    public SpeedModeListener speedModeListener;
    public LoadEventListener loadEventListener;
    public UnloadEventListener unloadEventListener;
    public StartEventListener startEventListener;
    public StopEventListener stopEventListener;

    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private DialogGenerator _dialogGenerator;
    [SerializeField] private InstructionGenerator _instructionGenerator;
    [SerializeField] private int startDate;
    [SerializeField] private int endDate;
    [SerializeField] private int normalTimeSpan; // �����ٶ�������ȫ������ʱ��(��)
    [SerializeField] private int acceleratedTimeSpan; // ����״̬������ȫ������ʱ��(��)

    private HandState rightHandState;
    private HandState leftHandState;
    private int DateDuration
    {
        get
        {
            return endDate - startDate;
        }
    }
    private float currentTime;
    private int currentTimeSpan;
    private bool canLoop;

    void Start()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        Reset();
    }

    public void Reset()
    {
        canLoop = true;
        currentTime = 0;
        currentTimeSpan = normalTimeSpan;
    }

    public async void Init()
    {
        loadEventListener?.Invoke();

        // �����ȴ����룬�滺����
        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("������Զ�Ŵ�Խ����");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        BeginScene();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        _instructionGenerator.GenerateInstruction("Ŀ�꣺��Ԫ2023��", "��ȭ���ԡ����١�ʱ��\n�ɿ�ȭͷ���ָ�������", 8);
    }

    private async void BeginScene()
    {
        startEventListener?.Invoke();

        while(canLoop)
        {
            await UniTask.NextFrame();

            UpdateTime();
            UpdateSpeed();  
        }

        speedModeListener?.Invoke(SpeedMode.Normal);
        stopEventListener?.Invoke();

        await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("������ǧ���������");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        unloadEventListener?.Invoke();
        _gameController.NextScene();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * (DateDuration / currentTimeSpan);

        if (currentTime >= DateDuration)
        {
            canLoop = false;
        }

        int date = Mathf.FloorToInt(currentTime + startDate);
        float progress = currentTime / DateDuration;

        dateMessageListener?.Invoke(Mathf.Min(date, endDate), Mathf.Min(progress, 1));
    }

    private void UpdateSpeed()
    {
        bool accelerateCondition = rightHandState.currentGesture == HandGesture.Grab || leftHandState.currentGesture == HandGesture.Grab;

        if(accelerateCondition)
        {
            if(currentTimeSpan != acceleratedTimeSpan)
            {
                currentTimeSpan = acceleratedTimeSpan;
                speedModeListener?.Invoke(SpeedMode.Accelerated);
                _instructionGenerator.HideInstruction();
            }  
        } else
        {
            if(currentTimeSpan != normalTimeSpan)
            {
                currentTimeSpan = normalTimeSpan;
                speedModeListener?.Invoke(SpeedMode.Normal);
            } 
        }
    }

    public delegate void DateMessageListener(int date, float progress);
    public delegate void SpeedModeListener(SpeedMode mode);
    public delegate void StartEventListener();
    public delegate void StopEventListener();
    public delegate void LoadEventListener();
    public delegate void UnloadEventListener();
}
