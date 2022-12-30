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

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        ObjectTransform = transform.GetChild(0);
        ObjectRenderer = ObjectTransform.GetComponent<Renderer>();
        ObjectAnimation = ObjectTransform.GetComponent<Animation>();

        ObjectTransform.gameObject.SetActive(false);

        Invoke("RunShowSelfAnimation", ShowSlefDelay);
    }

    private void RunShowSelfAnimation()
    {
        ObjectTransform.gameObject.SetActive(true);

        ObjectAnimation.Play("ShowSelf");

        if(isLastObject == true)
        {
            Invoke("FinishShowSelfAnimation", 0.75f);
        }
    }

    private void FinishShowSelfAnimation()
    {
        SendMessageUpwards("FinishInitializing");
    }

    public void ChangeToHoverState()
    {
        ObjectRenderer.material = HoverMaterial;

        ObjectAnimation.Play("HightlightSelf");
    }

    public void ChangeToDefaultState()
    {
        ObjectRenderer.material = DefaultMaterial;

        ObjectAnimation.Stop("HightlightSelf");

        ObjectTransform.localPosition = new Vector3(0, 0, 0);
    }

     // Update is called once per frame
    void Update()
    {
        
    }
}
