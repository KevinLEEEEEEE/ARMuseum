using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObjectController : MonoBehaviour
{
    public Material DefaultMaterial;
    public Material HoverMaterial;
    public float ShowSlefDelay = 0;

    private Transform ObjectTransform;
    private Renderer ObjectRenderer;
    private Animation ObjectAnimation;
    private bool isFirstInitialize = true;
    private bool isInitializing = false;

    private void OnEnable()
    {
        ObjectTransform = transform.GetChild(0);
        ObjectRenderer = ObjectTransform.GetComponent<Renderer>();
        ObjectAnimation = ObjectTransform.GetComponent<Animation>();
        ObjectTransform.gameObject.SetActive(false);

        Invoke("RunShowExhibitsAnimation", isFirstInitialize ? ShowSlefDelay : 0.6f);
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

    private void RunShowExhibitsAnimation()
    {
        
        ObjectTransform.gameObject.SetActive(true);
        ObjectAnimation.Play("ShowExhibits");
        
    }

    public void FinishInitializingObject()
    {
        SendMessageUpwards("FinishInitializing");
        isFirstInitialize = false;
        isInitializing = false;
    }

    public void ChangeToHoverState()
    {
        ObjectRenderer.material = HoverMaterial;
        ObjectAnimation.Play("HightlightExhibits");
    }

    public void ChangeToDefaultState()
    {
        ObjectRenderer.material = DefaultMaterial;
        ObjectAnimation.Stop("HightlightExhibits");
        ObjectTransform.localPosition = new Vector3(0, 0, 0);
    }
}
