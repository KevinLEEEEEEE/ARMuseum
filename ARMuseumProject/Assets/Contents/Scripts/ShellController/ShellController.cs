using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using TMPro;
using NRKernal;

public class ShellController : MonoBehaviour
{
    public GameController_Historical gameController;
    public DialogGenerator dialogGenerator;
    public InstructionGenerator instructionGenerator;
    public Match match;
    public AdvancedDissolvePropertiesController property_clay_standard;
    public AdvancedDissolvePropertiesController property_gold_standard;
    public GameObject blocks_standard;
    public GameObject blocks_geometric;
    public TextMeshProUGUI receivedState;
    public TextMeshProUGUI currentState;
    public AudioClip audioClip_shellFadeIn;
    public ParticleSystem basicEffect_smoke;

    private AudioGenerator audioSource_shellFadeIn;
    private ImageRecognition imageRecog;
    private ImageRecogResult currentResult;
    private readonly float shellFadeInDuration = 5.5f;
    private bool isTracking;

    void Start()
    {
        imageRecog = transform.GetComponent<ImageRecognition>();
        audioSource_shellFadeIn = new AudioGenerator(gameObject, audioClip_shellFadeIn);

        ResetAll();  
    }

    private void ResetAll()
    {
        currentResult = new ImageRecogResult(false, null);
        isTracking = false;
        blocks_standard.SetActive(false);
        blocks_geometric.SetActive(false);
        match.gameObject.SetActive(false);
    }

    public void Init()
    {
        StartCoroutine(nameof(ShellFadeIn));
    }

    private IEnumerator ShellFadeIn()
    {
        basicEffect_smoke.Play();

        yield return new WaitForSeconds(2f);

        audioSource_shellFadeIn.Play();
        blocks_standard.SetActive(true);
        PropertyFadeIn(property_clay_standard, shellFadeInDuration);

        yield return new WaitForSeconds(shellFadeInDuration + 2f);

        dialogGenerator.GenerateDialog("外范已经聚合");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        dialogGenerator.GenerateDialog("现在，点燃熔融之火...");

        yield return new WaitForSeconds(DialogGenerator.dialogDuration + 1.5f);

        instructionGenerator.GenerateInstruction("任务:引燃火焰", "划开火柴并靠近外范，以热量融化青铜");

        yield return new WaitForSeconds(2f);

        blocks_standard.SetActive(false);
        blocks_geometric.SetActive(true);
        
        //isTracking = true;
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

    private void BurnOutMessage()
    {
        //blocks_geometric.SetActive(false);
        //blocks_standard.SetActive(true);

        Debug.Log("Burnout");
    }

    private IEnumerator MiddleScene()
    {
        StopMatchTracking();
        isTracking = false;

        yield return new WaitForSeconds(3f);
    }

    private IEnumerator EndingScene()
    {
        yield return new WaitForSeconds(3f);
    }

    private void StartMatchTracking()
    {
        InvokeRepeating(nameof(CapturePhotoAndAnalysis), 0f, 1.5f);
    }

    private void StopMatchTracking()
    {
        CancelInvoke(nameof(CapturePhotoAndAnalysis));
    }

    private async void CapturePhotoAndAnalysis()
    {
        //if(gameController.GetHandPinchState())
        //{
            ImageRecogResult res = await imageRecog.TakePhotoAndAnalysis();

            UpdateRecogState(res);
        //}
    }

    private void UpdateRecogState(ImageRecogResult result)
    {
        //receivedState.text = "Received: " + result.GetResult() + ", Time use: " + (Time.time - result.GetTimestamp());

        //if (result.GetValidation())
        //{
        //    if (currentResult == null)
        //    {
        //        currentResult = result;
        //    }

        //    if (result.GetTimestamp() >= currentResult.GetTimestamp())
        //    {
        //        currentResult = result;
        //        currentState.text = "Current: " + result.GetResult() + ", Time use: " + (Time.time - result.GetTimestamp());
        //    }
        //}
    }

    void Update()
    {
        //if(isTracking && currentResult.GetResult())
        //{
        //    // update position
        //    //matchLight.SetActive(true);
        //    //matchLight.transform.position = gameController.getHandJointPose(HandJointID.IndexTip).position;
        //    match.ShowMatch();
        //} else
        //{
        //    //matchLight.SetActive(false);
        //    match.HideMatch();
        //}
    }
}
