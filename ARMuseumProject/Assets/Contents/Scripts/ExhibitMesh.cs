using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitMesh : MonoBehaviour
{
    public void FinishInitializingAnimation()
    {
        SendMessageUpwards("FinishInitializingObject");
    }
}
