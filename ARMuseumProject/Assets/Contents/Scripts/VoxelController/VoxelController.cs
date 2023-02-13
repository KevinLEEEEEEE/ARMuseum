using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using NRKernal;
using System;

public class VoxelController : MonoBehaviour
{
    public GameController_S2 gameController;
    public InstructionGenerator instructionGenerator;
    public HandModifier handModifier;
    public GameObject interactionHint_L;
    public GameObject interactionHint_R;
    public GameObject voxelBlock;
    public GameObject controllerOrb;
    public AudioClip audioClip_voxelActivate;

    private CameraShakeInstance shake;
    private Action instructionFadeOut;
    private AudioGenerator audioSource_voxelActivate;
    private Animation voxelAnimation;

    void Start()
    {
        voxelAnimation = transform.GetComponent<Animation>();
        audioSource_voxelActivate = new AudioGenerator(gameObject, audioClip_voxelActivate);
        voxelBlock.SetActive(false);
        controllerOrb.SetActive(false);

        // 重新设定显隐藏逻辑
    }

    public void Init()
    {
        StartCoroutine(nameof(OpeningScene));
    }

    private IEnumerator OpeningScene()
    {
        gameController.ShowGroundMask();
        audioSource_voxelActivate.Play();
        voxelAnimation.Play("VoxelMoveIn");
        voxelBlock.SetActive(true);

        yield return new WaitForSeconds(2f);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        yield return new WaitForSeconds(3f);

        shake.StartFadeOut(2f);

        yield return new WaitForSeconds(5.5f);

        gameController.HideGroundMask();
        gameController.SetAmbientVolumeInSeconds(0.5f, 2);
        controllerOrb.SetActive(true);
        handModifier.EnableModify();
        
        StartCoroutine(nameof(StartInstruction));
    }

    private IEnumerator EndingScene()
    {
        controllerOrb.SetActive(false);
        handModifier.DisableModify();
        voxelAnimation.Play("VoxelScale");

        // send message to server

        yield return new WaitForSeconds(3f);

        gameController.NextScene();
    }

    private IEnumerator StartInstruction()
    {
        instructionFadeOut = instructionGenerator.GenerateInstruction("任务:制作内模", "使用左右手食指触碰模具为青铜器塑形");
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

    private void DeleteStop()
    {

    }

    private void DeleteStart()
    {

    }

    private void DeleteComplete()
    {
        StartCoroutine(nameof(EndingScene));
    }

}
