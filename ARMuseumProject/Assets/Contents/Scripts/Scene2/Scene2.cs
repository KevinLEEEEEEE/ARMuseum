using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using NRKernal;

public class Scene2 : MonoBehaviour
{
    public GameController_S2 gameController;
    public GameObject instruction_L;
    public GameObject instruction_R;
    public GameObject interactionHint_L;
    public GameObject interactionHint_R;
    public GameObject groundMask;
    public GameObject voxelGenerator;
    public GameObject controllerOrb;
    private CameraShakeInstance shake;
    private AudioSource quakeSoundPlayer;
    private Animation voxelAnimation;
    private bool isFirstSub = true;

    void Start()
    {
        quakeSoundPlayer = transform.GetComponent<AudioSource>();
        voxelAnimation = transform.GetComponent<Animation>();

        instruction_L.SetActive(false);
        instruction_R.SetActive(false);
        //controllerOrb.SetActive(false);
        voxelGenerator.SetActive(false);
    }

    public void StartScene(Vector3 point, Vector3 direction)
    {
        transform.position = point;
        transform.forward = direction;

        StartCoroutine(nameof(OpeningScene));
    }

    private IEnumerator OpeningScene()
    {
        groundMask.SetActive(true);
        quakeSoundPlayer.Play();

        yield return new WaitForSeconds(0.5f);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        yield return new WaitForSeconds(0.75f);

        voxelAnimation.Play("VoxelMoveIn");
        voxelGenerator.SetActive(true);

        yield return new WaitForSeconds(2f);

        shake.StartFadeOut(2.5f);

        yield return new WaitForSeconds(2f);

        StartCoroutine(ShowInstruction(instruction_R, interactionHint_R, "R"));
    }

    private IEnumerator EndingScene()
    {
        instruction_L.SetActive(false);
        instruction_R.SetActive(false);
        interactionHint_L.SetActive(false);
        interactionHint_R.SetActive(false);
        controllerOrb.SetActive(false);
        voxelAnimation.Play("VoxelScale");

        yield return new WaitForSeconds(3f);

        gameController.FinshModeling();
    }

    public void AddVoxel()
    {
        StartCoroutine(HideInstruction(instruction_R, interactionHint_R, "R"));

        if (isFirstSub)
        {
            StartCoroutine(ShowInstruction(instruction_L, interactionHint_L, "L"));
        }
    }

    public void SubVoxel()
    {
        if (isFirstSub && instruction_L.activeSelf)
        {
            StartCoroutine(HideInstruction(instruction_L, interactionHint_L, "L"));
        }

        isFirstSub = false;  
    }

    private IEnumerator ShowInstruction(GameObject instruction, GameObject interaction, string side)
    {
        yield return new WaitForSeconds(3f);

        instruction.GetComponent<Animation>().Play("InstructionFadeIn_" + side);
        instruction.SetActive(true);
        interaction.GetComponent<InteractionHint>().StartHintLoop();
    }

    private IEnumerator HideInstruction(GameObject instruction, GameObject interaction, string side)
    {
        instruction.GetComponent<Animation>().Play("InstructionFadeOut_" + side);
        interaction.GetComponent<InteractionHint>().StopHintLoop();

        yield return new WaitForSeconds(1f);

        instruction.SetActive(false);
        controllerOrb.SetActive(true);
    }

    private void DeleteStop()
    {

    }

    private void DeleteStart()
    {

    }

    private void DeleteComplete()
    {
        StartCoroutine("EndingScene");
    }

}
