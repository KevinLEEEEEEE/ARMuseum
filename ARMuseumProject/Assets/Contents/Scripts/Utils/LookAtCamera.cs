using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class LookAtCamera : MonoBehaviour
{
    private Transform m_CenterAnchor;
    private Transform centerAnchor
    {
        get
        {
            if (m_CenterAnchor == null)
            {
                m_CenterAnchor = NRSessionManager.Instance.NRHMDPoseTracker.centerAnchor;
            }
            return m_CenterAnchor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position - centerAnchor.position);
    }
}
