using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exhibits : MonoBehaviour
{
    public Material DefaultMaterial;
    public Material HoverMaterial;
    public float ShowSlefDelay = 0;

    private Transform exhibitTransform;
    private Renderer exhibitRenderer;
    private Animation exhibitAnimation;
    private bool isFirstInitialize = true;
    private bool isInitializing = false;

    private void OnEnable()
    {
        exhibitTransform = transform.GetChild(0);
        exhibitRenderer = exhibitTransform.GetComponent<Renderer>();
        exhibitAnimation = exhibitTransform.GetComponent<Animation>();
        exhibitTransform.gameObject.SetActive(false);

        Invoke("RunShowExhibitsAnimation", isFirstInitialize ? ShowSlefDelay : 0);
        SendMessageUpwards("BeginInitializing");
        isInitializing = true;
    }

    private void OnDisable()
    {
        if (isInitializing)
        {
            CancelInvoke("RunShowExhibitsAnimation");
            SendMessageUpwards("FinishInitializing");
        }
    }

    public void ResetAll()
    {
        ChangeToDefaultState();
        isFirstInitialize = true;
    }

    private void RunShowExhibitsAnimation()
    {
        exhibitTransform.gameObject.SetActive(true);
        exhibitAnimation.Play("ShowExhibits");
    }

    public void FinishInitializingObject()
    {
        SendMessageUpwards("FinishInitializing");
        isFirstInitialize = false;
        isInitializing = false;
    }

    public void ChangeToHoverState()
    {
        exhibitRenderer.material = HoverMaterial;
        exhibitAnimation.Play("HightlightExhibits");
    }

    public void ChangeToDefaultState()
    {
        exhibitRenderer.material = DefaultMaterial;
        exhibitAnimation.Stop("HightlightExhibits");
        exhibitTransform.localPosition = new Vector3(0, 0, 0);
    }
}
