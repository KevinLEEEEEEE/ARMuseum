using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using System;
using Fraktalia.VoxelGen;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class VoxelController : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private DialogGenerator _dialogGenerator;
    [SerializeField] private InstructionGenerator _instructionGenerator;
    [SerializeField] private HandModifier handModifier;
    [SerializeField] private HandRotator handRotator;
    [SerializeField] private OrbButton orbButton;
    [SerializeField] private VoxelSaveSystem voxelSaveSystem;
    [SerializeField] private InteractionHint interactionHint_R;
    [SerializeField] private GameObject voxelBlock;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private AudioClip audioClip_voxelAppear;
    [SerializeField] private AudioClip audioClip_voxelScale;

    private LeanServer leanServer;
    private AudioGenerator audioSource_voxelAppear;
    private AudioGenerator audioSource_voxelScale;
    private Animation animationComp;
    private bool byteBufferSaved;
    private bool hasModified;

    void Awake()
    {
        animationComp = transform.GetComponent<Animation>();
        leanServer = transform.GetComponent<LeanServer>();

        Fraktalia.VoxelGen.SaveSystem.Modules.SaveModule_ByteBuffer_V2.OnDataBufferSaved += VoxelByteBufferSaved;
        orbButton.orbButtonFinishEvent += EndingScene;

        Reset();
    }

    private void Reset()
    {
        byteBufferSaved = false;
        hasModified = false;
        menuRoot.SetActive(false);
        SetRootsActive(false);
    }

    private void SetRootsActive(bool state)
    {
        foreach (Transform trans in transform)
            trans.gameObject.SetActive(state);
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        audioSource_voxelAppear = new AudioGenerator(gameObject, audioClip_voxelAppear);
        audioSource_voxelScale = new AudioGenerator(gameObject, audioClip_voxelScale);

        // ���ñ��������������볡��������
        _gameController.SetAmbientVolumeInSeconds(0.4f, 2);
        _gameController.ShowGroundMask();
        audioSource_voxelAppear.Play();

        // �����볡����
        SetRootsActive(true);
        animationComp.Play("VoxelMoveIn");

        await UniTask.Delay(TimeSpan.FromSeconds(9), ignoreTimeScale: false);

        // �رյײ�����
        _gameController.HideGroundMask();
        
        // �������ƽ�����˵�
        handModifier.EnableModify();
        handRotator.EnableRotator();
        menuRoot.SetActive(true);

        // ��ʾ�û���һ������
        interactionHint_R.StartHintLoop();
        _instructionGenerator.GenerateInstruction("���������ͭ��", "�Ķ�ƽ���ϵĲ�����ʾ�����ģ������");
    }

    private async void EndingScene()
    {
        handModifier.DisableModify();
        handRotator.DisableRotator();
        menuRoot.SetActive(false);

        // ����༭������洢���ݣ����������洢����
        if(hasModified)
        {
            // ��Voxel���ݴ��������������ȴ����ݴ�����ɣ���ֹ����
            SaveUserVoxel();

            await UniTask.WaitUntil(() => byteBufferSaved == true);
        } else
        {
            FirstModify();
        }

        _dialogGenerator.GenerateDialog("�����ѹ̶�����ģ�γ���...");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // ���Ž�������
        audioSource_voxelScale.Play();
        animationComp.Play("VoxelScale");
        voxelBlock.transform.DORotate(new Vector3(0, 0, 0), 2.5f);
        _gameController.StopAmbientSound();

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        _gameController.NextScene();

        await UniTask.Delay(TimeSpan.FromSeconds(8), ignoreTimeScale: false);

        // �ȴ�shell���ͺ�voxel��������
        audioSource_voxelAppear.Unload();
        audioSource_voxelScale.Unload();
        Reset();
    }

    public void FirstModify()
    {
        hasModified = true;
        interactionHint_R.StopHintLoop();
        _instructionGenerator.HideInstruction();
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
