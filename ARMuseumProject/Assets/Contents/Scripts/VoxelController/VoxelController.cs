using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using NRKernal;
using System;
using Fraktalia.VoxelGen;

public class VoxelController : MonoBehaviour
{
    public GameController_Historical gameController;
    public DialogGenerator dialogGenerator;
    public InstructionGenerator instructionGenerator;
    public HandModifier handModifier;
    public OrbButton orbButton;
    public GameObject interactionHint_L;
    public GameObject interactionHint_R;
    public GameObject voxelBlock;
    public GameObject controllerOrb;
    public AudioClip audioClip_voxelAppear;
    public AudioClip audioClip_voxelScale;
    public VoxelSaveSystem voxelSaveSystem;

    private LeanServer leanServer;
    private CameraShakeInstance shake;
    private Action instructionFadeOut;
    private AudioGenerator audioSource_voxelAppear;
    private AudioGenerator audioSource_voxelScale;
    private Animation voxelAnimation;

    void Start()
    {
        voxelAnimation = transform.GetComponent<Animation>();
        leanServer = transform.GetComponent<LeanServer>();

        Fraktalia.VoxelGen.SaveSystem.Modules.SaveModule_ByteBuffer_V2.OnDataBufferSaved += VoxelByteBufferSaved;
        orbButton.orbButtonFinishEvent += () => { StartCoroutine(nameof(EndingScene)); };

        audioSource_voxelAppear = new AudioGenerator(gameObject, audioClip_voxelAppear);
        audioSource_voxelScale = new AudioGenerator(gameObject, audioClip_voxelScale);

        voxelBlock.SetActive(false);
        controllerOrb.SetActive(false);  
    }

    public void Init()
    {
        StartCoroutine(nameof(OpeningScene));
    }

    public void HideVoxel()
    {
        voxelBlock.SetActive(false);
        handModifier.gameObject.SetActive(false);
    }

    private IEnumerator OpeningScene()
    {
        gameController.ShowGroundMask();
        gameController.StartAmbientSound();
        gameController.SetAmbientVolumeInSeconds(0.5f, 2);
        audioSource_voxelAppear.Play();
        voxelAnimation.Play("VoxelMoveIn");
        voxelBlock.SetActive(true);

        yield return new WaitForSeconds(2f);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        yield return new WaitForSeconds(3f);

        shake.StartFadeOut(2f);

        yield return new WaitForSeconds(5.5f);

        gameController.HideGroundMask();
        StartCoroutine(nameof(StartInstruction));

        yield return new WaitForSeconds(1f);

        handModifier.EnableModify();
        controllerOrb.SetActive(true);     
    }

    private IEnumerator EndingScene()
    {
        if(instructionFadeOut != null) StopInstruction();
        controllerOrb.SetActive(false);
        handModifier.DisableModify();

        // 将Voxel数据传送至服务器
        SaveUserVoxel();

        yield return new WaitForSeconds(1f);

        dialogGenerator.GenerateDialog("器形已固定，容器形成中...");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1f);

        audioSource_voxelScale.Play();
        voxelAnimation.Play("VoxelScale");

        yield return new WaitForSeconds(4f);

        gameController.NextScene();
    }

    private void SaveUserVoxel()
    {
        voxelSaveSystem.EditorSaveMode = EditorVoxelSaveMode.ByteBuffer_V2;
        voxelSaveSystem.ModuleByteBuffer_V2.Key = gameController.UserID;
        voxelSaveSystem.Save();
    }

    private void VoxelByteBufferSaved(string id, byte[] bytes)
    {
        NRDebugger.Info(string.Format("[VoxelController] Voxel saved, key: {0}, byte length: {1}.", id, bytes.Length));

        leanServer.SaveVoxel(id, bytes);
    }

    private IEnumerator StartInstruction()
    {
        instructionFadeOut = instructionGenerator.GenerateInstruction("任务:制作内模", "使用左右手食指触碰模具为青铜器模具塑形");
        interactionHint_R.GetComponent<InteractionHint>().StartHintLoop();

        yield return new WaitForSeconds(4f);

        interactionHint_L.GetComponent<InteractionHint>().StartHintLoop();
    }

    public void StopInstruction()
    {
        instructionFadeOut();
        instructionFadeOut = null;
        interactionHint_R.GetComponent<InteractionHint>().StopHintLoop();
        interactionHint_L.GetComponent<InteractionHint>().StopHintLoop();
    }
}
