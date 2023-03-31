using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private bool lockY;
    private Transform CenterAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }

    void Update()
    {
        Vector3 point = CenterAnchor.transform.position;

        if(lockY)
        {
            point.y = transform.position.y;
        }

        transform.LookAt(2 * transform.position - point);
    }
}
