using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObjectController : MonoBehaviour
{
    public Material DefaultMaterial;
    public Material HoverMaterial;
    public float ShowSlefDelay = 0;
    public bool isLastObject = false;

    private Transform ObjectTransform;
    private Renderer ObjectRenderer;
    private Animation ObjectAnimation;
    private bool isFirstEnable;

    private void OnEnable()
    {
        ObjectTransform = transform.GetChild(0);
        ObjectRenderer = ObjectTransform.GetComponent<Renderer>();
        ObjectAnimation = ObjectTransform.GetComponent<Animation>();

        ObjectTransform.gameObject.SetActive(false);
        isFirstEnable = true;

        if (isFirstEnable == true)
        {
            Invoke("RunShowExhibitsAnimation", ShowSlefDelay);

            isFirstEnable = false;
        }
        else
        {
            Invoke("RunShowExhibitsAnimation", 1f);
        }
    }

    private void OnDisable()
    {
        CancelInvoke("RunShowExhibitsAnimation");
    }

    private void RunShowExhibitsAnimation()
    {
        ObjectTransform.gameObject.SetActive(true);

        ObjectAnimation.Play("ShowExhibits");

        if(isLastObject == true)
        {
            Invoke("FinishShowExhibitsAnimation", 0.75f);
        }
    }

    private void FinishShowExhibitsAnimation()
    {
        SendMessageUpwards("FinishInitializing");
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
