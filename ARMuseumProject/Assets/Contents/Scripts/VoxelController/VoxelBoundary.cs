using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelBoundary : MonoBehaviour
{
    public Action leftHandExitBoundaryListener;
    public Action rightHandExitBoundaryListener;

    private void OnTriggerExit(Collider other)
    {
        if(other.name == "ColliderEntity_IndexTip_R")
        {
            rightHandExitBoundaryListener?.Invoke();
        } else if (other.name == "ColliderEntity_IndexTip_L") {
            leftHandExitBoundaryListener?.Invoke();
        }
    }
}
