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
    [SerializeField] private int normalTimeSpan; // 常规速度下走完全程所需时间(秒)
    [SerializeField] private int acceleratedTimeSpan; // 加速状态下走完全程所需时间(秒)

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

        // 动画等待两秒，舒缓节奏
        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("它将从远古穿越至今");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        BeginScene();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        _instructionGenerator.GenerateInstruction("目标：公元2023年", "握拳可以「加速」时间\n松开拳头「恢复」流速", 8);
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

        _dialogGenerator.GenerateDialog("历经四千五百年岁月");

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
