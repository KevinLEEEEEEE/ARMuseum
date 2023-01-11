using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        transform.gameObject.SetActive(false);
#endif
    }
}
