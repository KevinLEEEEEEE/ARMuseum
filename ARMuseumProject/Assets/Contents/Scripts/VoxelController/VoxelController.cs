using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using NRKernal;
using System;

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

    private CameraShakeInstance shake;
    private Action instructionFadeOut;
    private AudioGenerator audioSource_voxelAppear;
    private AudioGenerator audioSource_voxelScale;
    private Animation voxelAnimation;

    void Start()
    {
        voxelAnimation = transform.GetComponent<Animation>();
        audioSource_voxelAppear = new AudioGenerator(gameObject, audioClip_voxelAppear);
        audioSource_voxelScale = new AudioGenerator(gameObject, audioClip_voxelScale);
        orbButton.orbButtonFinishEvent += () => { StartCoroutine(nameof(EndingScene)); };
        voxelBlock.SetActive(false);
        controllerOrb.SetActive(false);
    }

    public void Init()
    {
        StartCoroutine(nameof(OpeningScene));
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

        // send message to server

        yield return new WaitForSeconds(1f);

        dialogGenerator.GenerateDialog("器形已固定，容器形成中...");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1f);

        audioSource_voxelScale.Play();
        voxelAnimation.Play("VoxelScale");

        yield return new WaitForSeconds(4f);

        gameController.NextScene();
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
