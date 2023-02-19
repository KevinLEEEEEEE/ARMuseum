using System.Collections;
using System.Collections.Generic;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using NRKernal;
using System;
using TMPro;

public class ShellController : MonoBehaviour
{
    public GameController_Historical gameController;
    public DialogGenerator dialogGenerator;
    public InstructionGenerator instructionGenerator;
    public VoxelController voxelController;
    public PostCardController postCardController;
    public Match match;
    public AdvancedDissolvePropertiesController property_clay_standard;
    public AdvancedDissolvePropertiesController property_gold_standard;
    public GameObject block_standard_clay;
    public GameObject block_standard_gold;
    public GameObject blocks_geometric;
    public AudioClip audioClip_shellFadeIn;
    public float objectDetectionFrequency = 1f;
    public float shellFadeInDuration = 5.6f;
    public bool displayReceivedInfo;
    public TextMeshProUGUI receivedInfoUI;
    public ParticleSystem[] basicEffect;
    public ParticleSystem[] burningEffect;
    public ParticleSystem[] fadeoutEffect;

    private AudioGenerator audioSource_shellFadeIn;
    private CameraManager cameraManager;
    private ImageRecognition imageRecognition;
    private ImageRecogResult latestRecogResult;
    private Action hideMatchInstruction;

    void Start()
    {
        audioSource_shellFadeIn = new AudioGenerator(gameObject, audioClip_shellFadeIn);
        imageRecognition = transform.GetComponent<ImageRecognition>();
        cameraManager = transform.GetComponent<CameraManager>();
        match.BurningEventListener += StartBurning;
        match.BurnoutEventListener += () => { StartCoroutine(nameof(EndingScene)); };
        receivedInfoUI.gameObject.SetActive(displayReceivedInfo);
        ResetAll();
    }

    private void ResetAll()
    {
        block_standard_clay.SetActive(false);
        block_standard_gold.SetActive(false);
        blocks_geometric.SetActive(false);
    }

    public void Init()
    {
        StartCoroutine(nameof(OpeningScene));
    }

    private IEnumerator OpeningScene()
    {
        StartParticles(basicEffect);

        yield return new WaitForSeconds(2f);

        audioSource_shellFadeIn.Play();
        block_standard_clay.SetActive(true);
        PropertyFadeIn(property_clay_standard, shellFadeInDuration);

        yield return new WaitForSeconds(shellFadeInDuration + 2.5f);

        // 此时可以隐藏voxel全部内容
        voxelController.HideVoxel();

        dialogGenerator.GenerateDialog("容器已形成......");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        dialogGenerator.GenerateDialog("现在，点燃熔融之火");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        hideMatchInstruction = instructionGenerator.GenerateInstruction("任务:引燃火种", "划开火柴并用火苗触碰模型，点燃青铜之火");

        yield return new WaitForSeconds(3f);

        blocks_geometric.SetActive(true);
        block_standard_clay.SetActive(false);
        StartCoroutine(nameof(ObjectDetection));
    }

    private void StartBurning()
    {
        NRDebugger.Info("[ShellController] Shell start burning.");

        cameraManager.Close();
        StopCoroutine(nameof(ObjectDetection));

        StartParticles(burningEffect);
        StopParticles(basicEffect);

        hideMatchInstruction();
        hideMatchInstruction = null;
    }

    private IEnumerator EndingScene()
    {
        NRDebugger.Info("[ShellController] Shell burnout");

        block_standard_gold.SetActive(true);
        blocks_geometric.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        dialogGenerator.GenerateDialog("青铜器已完成铸造");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration);

        postCardController.SetUserID(gameController.UserID);
        postCardController.ShowPostCard();

        StopParticles(burningEffect);
        StartParticles(fadeoutEffect);
        PropertyFadeOut(property_gold_standard, 5f);
        
        yield return new WaitForSeconds(5f);

        block_standard_gold.SetActive(false);
    }

    private IEnumerator ObjectDetection()
    {
        while(true)
        {
            CapturePhotoAndAnalysis();

            yield return new WaitForSeconds(objectDetectionFrequency);
        }
    }

    private void CapturePhotoAndAnalysis()
    {
        bool rightHandPinchState = NRInput.Hands.GetHandState(HandEnum.RightHand).isPinching;
        bool leftHandPinchState = NRInput.Hands.GetHandState(HandEnum.LeftHand).isPinching;
        
        if(rightHandPinchState || leftHandPinchState)
        {
            NRDebugger.Info("[ShellController] Start taking photo.");

            imageRecognition.TakePhotoAndAnalysis(UpdateImageRecogResult);

        } else
        {
            NRDebugger.Info("[ShellController] Skip taking photo due to invalid gesture.");
        }
    }

    public void UpdateImageRecogResult(ImageRecogResult res)
    {
        if (displayReceivedInfo)
        {
            receivedInfoUI.text = string.Format("IsSuccessful: {0},\nStartTime: {1}s,\nRecogDuration: {2}ms,\nIsBurning: {3}.",
                res.IsSuccessful(), res.GetStartTime().ToString("f3"), res.GetCostTime(), res.ContainLabel("burning"));
        }

        if (latestRecogResult != null && res.GetStartTime() < latestRecogResult.GetStartTime())
        {
            NRDebugger.Info("[ShellController] Earlier recog result received, skip it.");
            return;
        }

        if (res.IsSuccessful())
        {
            if (res.ContainLabel("burning"))
            {
                NRDebugger.Info(string.Format("[ShellController] Match detected, Receive result in {0} ms.", res.GetCostTime()));
                match.EnableMatch();
                return;
            }
            else
            {
                NRDebugger.Info(string.Format("[ShellController] Match undetected, Receive result in {0} ms.", res.GetCostTime()));
            }

            latestRecogResult = res;
        }
        else
        {
            NRDebugger.Info("[ShellController] Image detection request failed.");
        }

        match.DisableMatch();
    }

    private void PropertyFadeIn(AdvancedDissolvePropertiesController controller, float duration)
    {
        StartCoroutine(duration.Tweeng((r) =>
        {
            controller.cutoutStandard.clip = r;
        }, 1f, 0));
    }

    private void PropertyFadeOut(AdvancedDissolvePropertiesController controller, float duration)
    {
        StartCoroutine(duration.Tweeng((r) =>
        {
            controller.cutoutStandard.clip = r;
        }, 0, 1f));
    }

    private void StartParticles(ParticleSystem[] particles)
    {
        foreach (ParticleSystem sys in particles)
        {
            sys.Play();
        }
    }

    private void StopParticles(ParticleSystem[] particles)
    {
        foreach (ParticleSystem sys in particles)
        {
            sys.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
