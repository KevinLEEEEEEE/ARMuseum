using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using TMPro;
using NRKernal;

public class ShellController : MonoBehaviour
{
    public GameController_S2 gameController;
    public ImageRecognition imageRecog;
    public TextMeshProUGUI receivedState;
    public TextMeshProUGUI currentState;
    public GameObject blocks_standard;
    public GameObject blocks_geometric;
    public DialogGenerator dialog;
    public Match match;
    public AdvancedDissolvePropertiesController property_clay_standard;
    public AdvancedDissolvePropertiesController property_gold_standard;
    private ImageRecogResult currentResult;
    private bool isTracking;

    void Start()
    {
        ResetAll();

        StartCoroutine(nameof(ShellFadeIn));
    }

    private void ResetAll()
    {
        blocks_standard.SetActive(false);
        blocks_geometric.SetActive(false);
        currentResult = new ImageRecogResult(true, true, 0);
        isTracking = false;  
    }

    private IEnumerator ShellFadeIn()
    {
        blocks_standard.SetActive(true);

        yield return new WaitForSeconds(2f);

        float shellFadeInDuration = 6f;

        PropertyFadeIn(property_clay_standard, shellFadeInDuration);

        yield return new WaitForSeconds(shellFadeInDuration);

        blocks_geometric.SetActive(true);
        blocks_standard.SetActive(false);
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

    public void SetTransform(Vector3 point, Vector3 direction)
    {
        transform.position = point;
        transform.forward = direction;
    }

    public void InitShell()
    {
        StartCoroutine("StartScene");
    }

    private void BurnOutMessage()
    {
        //blocks_geometric.SetActive(false);
        //blocks_standard.SetActive(true);

        Debug.Log("Burnout");
    }

    private IEnumerator StartScene()
    {
        
        //dialog.GenerateDialog("ÒýÈ¼»ðÖÖ");

        yield return new WaitForSeconds(1f);

        //StartMatchTracking();
        //matchLight.SetActive(true);
        isTracking = true;
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
        receivedState.text = "Received: " + result.GetResult() + ", Time use: " + (Time.time - result.GetTimestamp());

        if (result.GetValidation())
        {
            if (currentResult == null)
            {
                currentResult = result;
            }

            if (result.GetTimestamp() >= currentResult.GetTimestamp())
            {
                currentResult = result;
                currentState.text = "Current: " + result.GetResult() + ", Time use: " + (Time.time - result.GetTimestamp());
            }
        }
    }

    void Update()
    {
        if(isTracking && currentResult.GetResult())
        {
            // update position
            //matchLight.SetActive(true);
            //matchLight.transform.position = gameController.getHandJointPose(HandJointID.IndexTip).position;
            match.ShowMatch();
        } else
        {
            //matchLight.SetActive(false);
            match.HideMatch();
        }
    }
}
