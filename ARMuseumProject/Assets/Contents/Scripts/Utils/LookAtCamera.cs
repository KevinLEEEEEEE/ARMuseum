using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class LookAtCamera : MonoBehaviour
{
    private Transform centerAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }

    void Update()
    {
        transform.LookAt(2 * transform.position - centerAnchor.transform.position);
    }
}
