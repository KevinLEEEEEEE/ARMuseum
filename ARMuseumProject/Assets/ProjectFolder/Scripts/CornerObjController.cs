using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class CornerObjController : MonoBehaviour
{
    [SerializeField]
    private GameObject LeftTopCorner;
    [SerializeField]
    private GameObject LeftBottomCorner;
    [SerializeField]
    private GameObject RightTopCorner;
    [SerializeField]
    private GameObject RightBottomCorner;

    public void ActiveCornerObjects()
    {
        SwitchCornerState(true);
    }

    public void InactiveCornerObjects()
    {
        SwitchCornerState(false);
    }

    private void SwitchCornerState(bool state)
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }

    public void ResetAll()
    {
        SwitchCornerState(false);
    }
}
