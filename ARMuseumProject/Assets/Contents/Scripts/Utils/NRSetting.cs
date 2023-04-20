using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NRSetting : MonoBehaviour
{
    [SerializeField] private bool raycastersActive;

    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        NRInput.RaycastersActive = raycastersActive;
    }

    public void EnableRaycaster()
    {
        NRInput.RaycastersActive = true;
    }

    public void DisableRaycaster()
    {
        NRInput.RaycastersActive = false;
    }
}
