using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using NRKernal;
using System;
using Fraktalia.VoxelGen;
using Cysharp.Threading.Tasks;

public class VoxelController : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private DialogGenerator _dialogGenerator;
    [SerializeField] private InstructionGenerator _instructionGenerator;
    [SerializeField] private HandModifier handModifier;
    [SerializeField] private OrbButton orbButton;
    [SerializeField] private VoxelSaveSystem voxelSaveSystem;
    [SerializeField] private InteractionHint interactionHint_L;
    [SerializeField] private InteractionHint interactionHint_R;
    [SerializeField] private GameObject voxelBlock;
    [SerializeField] private AudioClip audioClip_voxelAppear;
    [SerializeField] private AudioClip audioClip_voxelScale;

    private LeanServer leanServer;
    private CameraShakeInstance shake;
    private AudioGenerator audioSource_voxelAppear;
    private AudioGenerator audioSource_voxelScale;
    private Animation animationComp;
    private Action modifyInstructionHandler;
    private bool hasModified;

    void Start()
    {
        animationComp = transform.GetComponent<Animation>();
        leanServer = transform.GetComponent<LeanServer>();

        Fraktalia.VoxelGen.SaveSystem.Modules.SaveModule_ByteBuffer_V2.OnDataBufferSaved += VoxelByteBufferSaved;
        orbButton.orbButtonFinishEvent += EndingScene;

        audioSource_voxelAppear = new AudioGenerator(gameObject, audioClip_voxelAppear);
        audioSource_voxelScale = new AudioGenerator(gameObject, audioClip_voxelScale);

        Reset();
    }

    private void Reset()
    {
        hasModified = false;
        voxelBlock.SetActive(false);
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        // 设置背景音量并播放入场动画声音
        _gameController.SetAmbientVolumeInSeconds(0.4f, 2);
        audioSource_voxelAppear.Play();

        // 启动入场动画
        voxelBlock.SetActive(true);
        animationComp.Play("VoxelMoveIn");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        shake.StartFadeOut(2f);

        await UniTask.Delay(TimeSpan.FromSeconds(5.5), ignoreTimeScale: false);

        modifyInstructionHandler = _instructionGenerator.GenerateInstruction("任务:制作内模", "使用左右手食指触碰模具为青铜器模具塑形");
        handModifier.EnableModify();
        ModifyInstructionLoop();
    }

    private async void EndingScene()
    {
        handModifier.DisableModify();

        // 将Voxel数据传送至服务器
        SaveUserVoxel();

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("器形已固定，容器形成中...");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        // 播放结束动画
        audioSource_voxelScale.Play();
        animationComp.Play("VoxelScale");
        _gameController.StopAmbientSound();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        _gameController.NextScene();

        // 等待shell成型后，voxel可以隐藏
        await UniTask.Delay(TimeSpan.FromSeconds(12), ignoreTimeScale: false);

        Reset();
    }

    private async void ModifyInstructionLoop()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        while (!hasModified)
        {
            interactionHint_R.StartHintLoop();

            await UniTask.Delay(TimeSpan.FromSeconds(2.1), ignoreTimeScale: false);

            interactionHint_R.StopHintLoop();

            await UniTask.Delay(TimeSpan.FromSeconds(1.2), ignoreTimeScale: false);

            interactionHint_L.StartHintLoop();

            await UniTask.Delay(TimeSpan.FromSeconds(2.1), ignoreTimeScale: false);

            interactionHint_L.StopHintLoop();

            await UniTask.Delay(TimeSpan.FromSeconds(3.5), ignoreTimeScale: false);
        }
    }

    private void HideModifyInstruction()
    {
        modifyInstructionHandler();
        interactionHint_R.gameObject.SetActive(false);
        interactionHint_L.gameObject.SetActive(false);
    }

    public async void FirstModify()
    {
        hasModified = true;
        HideModifyInstruction();
        
        // 等待一段时间后再给出下一步任务提示
        await UniTask.Delay(TimeSpan.FromSeconds(8), ignoreTimeScale: false);

        orbButton.EnableButton();
        _instructionGenerator.GenerateInstruction("看向按钮", "完成编辑后需要长按「下一步」按钮", 12f);

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        _instructionGenerator.GenerateTrail(orbButton.transform.position, 6f, 3f);
    }

    private void SaveUserVoxel()
    {
        voxelSaveSystem.EditorSaveMode = EditorVoxelSaveMode.ByteBuffer_V2;
        voxelSaveSystem.ModuleByteBuffer_V2.Key = _gameController.UserID;
        voxelSaveSystem.Save();
    }

    private void VoxelByteBufferSaved(string id, byte[] bytes)
    {
        NRDebugger.Info(string.Format("[VoxelController] Voxel saved, key: {0}, byte length: {1}.", id, bytes.Length));

        leanServer.SaveVoxel(id, bytes);
    }
}
