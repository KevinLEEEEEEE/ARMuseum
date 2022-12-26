using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class LookAtCamera : MonoBehaviour
{
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

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position - CenterCamera.transform.position);
    }
}
