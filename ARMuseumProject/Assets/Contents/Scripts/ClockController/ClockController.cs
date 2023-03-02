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
    private bool canRunClock;
    private Action instructionHandler;

    void Start()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        Reset();
    }

    public void Reset()
    {
        canRunClock = true;
        
        currentTime = 0;
        currentTimeSpan = normalTimeSpan;
    }

    public async void Init()
    {
        loadEventListener?.Invoke();

        // 动画等待两秒，舒缓节奏
        await UniTask.Delay(TimeSpan.FromSeconds(2f), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("它将历经时光的洗礼");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1.5f), ignoreTimeScale: false);

        BeginScene();

        await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);

        instructionHandler = _instructionGenerator.GenerateInstruction("握拳加速", "手掌握拳可以加速时间流逝");

        await UniTask.Delay(TimeSpan.FromSeconds(5.5f), ignoreTimeScale: false);

        HideSpeedUpInstruction();
    }

    private async void BeginScene()
    {
        startEventListener?.Invoke();

        while(canRunClock)
        {
            await UniTask.NextFrame();

            UpdateTime();
            UpdateSpeed();  
        }

        speedModeListener?.Invoke(SpeedMode.Normal);
        stopEventListener?.Invoke();

        await UniTask.Delay(TimeSpan.FromSeconds(6f), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("这是跨越时空的相遇");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1f), ignoreTimeScale: false);

        unloadEventListener?.Invoke();
        _gameController.NextScene();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * (DateDuration / currentTimeSpan);

        if (currentTime >= DateDuration)
        {
            canRunClock = false;
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
                HideSpeedUpInstruction();
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

    private void HideSpeedUpInstruction()
    {
        if(instructionHandler != null)
        {
            instructionHandler();
            instructionHandler = null;
        }
    }

    public delegate void DateMessageListener(int date, float progress);
    public delegate void SpeedModeListener(SpeedMode mode);
    public delegate void StartEventListener();
    public delegate void StopEventListener();
    public delegate void LoadEventListener();
    public delegate void UnloadEventListener();
}
