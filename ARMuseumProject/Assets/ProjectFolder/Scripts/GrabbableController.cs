using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbableController : MonoBehaviour
{
    public GameObject NRCamera;
    public GameObject[] GrabbableItemsArray;
    private Transform m_CenterCamera;
    private Transform CenterCamera
    {
        get
        {
            if (m_CenterCamera == null)
            {
                if (NRSessionManager.Instance.CenterCameraAnchor != null)
                {
                    m_CenterCamera = NRSessionManager.Instance.CenterCameraAnchor;
                }
                else if (Camera.main != null)
                {
                    m_CenterCamera = Camera.main.transform;
                }
            }
            return m_CenterCamera;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //transform.position = TargetObject.transform.position;
        //transform.rotation = TargetObject.transform.rotation;
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name + "-Grabbable");
        //float originDistance = Vector3.Distance(transform.position, CenterCamera == null ? Vector3.zero : CenterCamera.position);

        targetObject.position = obj.transform.position;
        targetObject.rotation = obj.transform.rotation;
        targetObject.GetComponent<GrabbableObject>().TargetPosition = CenterCamera.transform.position + CenterCamera.transform.forward * 0.8f;
        targetObject.GetComponent<GrabbableObject>().TargetTransform = CenterCamera.transform;

        targetObject.gameObject.SetActive(true);
    }

    public void InactiveGrabbleItems()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
