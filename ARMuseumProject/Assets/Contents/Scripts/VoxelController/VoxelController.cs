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
    [SerializeField] private GameObject orbButtonInstruction;
    [SerializeField] private AudioClip audioClip_voxelAppear;
    [SerializeField] private AudioClip audioClip_voxelScale;

    private LeanServer leanServer;
    private CameraShakeInstance shake;
    private AudioGenerator audioSource_voxelAppear;
    private AudioGenerator audioSource_voxelScale;
    private Animation animationComp;
    private Action modifyInstructionHandler;
    private bool hasModified;
    private bool byteBufferSaved;

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
        byteBufferSaved = false;
        voxelBlock.SetActive(false);
        orbButtonInstruction.SetActive(false);
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        // ���ñ��������������볡��������
        _gameController.SetAmbientVolumeInSeconds(0.4f, 2);
        _gameController.ShowGroundMask();
        audioSource_voxelAppear.Play();

        // �����볡����
        voxelBlock.SetActive(true);
        animationComp.Play("VoxelMoveIn");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        shake.StartFadeOut(2f);

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        _gameController.HideGroundMask();
        modifyInstructionHandler = _instructionGenerator.GenerateInstruction("��������ģ", "����ʳָ������ɾ����\n����ʳָ���������ӡ�");
        handModifier.EnableModify();
        ModifyInstructionLoop();
    }

    private async void EndingScene()
    {
        handModifier.DisableModify();
        orbButtonInstruction.SetActive(false);

        // �����ٵȴ�
        await UniTask.NextFrame();

        // ��Voxel���ݴ�����������
        SaveUserVoxel();

        // �ȴ����ݴ������
        await UniTask.WaitUntil(() => byteBufferSaved == true);

        // �����ٵȴ�
        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        _dialogGenerator.GenerateDialog("�����ѹ̶�����ģ�γ���...");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        // ���Ž�������
        audioSource_voxelScale.Play();
        animationComp.Play("VoxelScale");
        _gameController.StopAmbientSound();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        _gameController.NextScene();

        // �ȴ�shell���ͺ�voxel��������
        await UniTask.Delay(TimeSpan.FromSeconds(12), ignoreTimeScale: false);

        Reset();
    }

    private async void ModifyInstructionLoop()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

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

    public void FirstModify()
    {
        hasModified = true;
        HideModifyInstruction();

        orbButton.EnableButton();
        orbButtonInstruction.SetActive(true);

        // �ȴ�һ��ʱ����ٸ�����һ��������ʾ
        //await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);


        //_instructionGenerator.GenerateInstruction("��ťλ����ʾ", "��ɺ����Ҳࡸ��һ������ť", 12f);

        //await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        //_instructionGenerator.GenerateTrail(orbButton.transform.position, 4f, 4f);
    }

    private void SaveUserVoxel()
    {
        voxelSaveSystem.EditorSaveMode = EditorVoxelSaveMode.ByteBuffer_V2;
        voxelSaveSystem.ModuleByteBuffer_V2.Key = _gameController.UserID;
        voxelSaveSystem.Save();
    }

    private async void VoxelByteBufferSaved(string id, byte[] bytes)
    {
        NRDebugger.Info(string.Format("[VoxelController] Voxel saved, key: {0}, byte length: {1}.", id, bytes.Length));

        await leanServer.SaveVoxel(id, bytes);

        byteBufferSaved = true;
    }
}
