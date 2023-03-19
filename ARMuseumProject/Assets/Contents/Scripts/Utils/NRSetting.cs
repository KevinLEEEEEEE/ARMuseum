using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NRSetting : MonoBehaviour
{
    [SerializeField] private bool raycastersActive;

    void Start()
    {
        NRInput.RaycastersActive = raycastersActive;
    }
}
