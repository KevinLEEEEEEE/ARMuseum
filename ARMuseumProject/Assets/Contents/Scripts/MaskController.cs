using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        transform.gameObject.SetActive(false);
#endif
    }
}
