using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class CornerObjController : MonoBehaviour
{
    [SerializeField]
    private GameObject UpperLeft;
    [SerializeField]
    private GameObject LowerLeft;
    [SerializeField]
    private GameObject UpperRight;
    [SerializeField]
    private GameObject LowerRight;

    public void ActiveCornerObjects()
    {
        SwitchCornerState(true);
    }

    public void InactiveCornerObjects()
    {
        SwitchCornerState(false);
    }

    public void HightlightCornerObjects()
    {
        foreach (Transform child in transform)
        {
            child.GetChild(0).GetComponent<Animation>().Play("HightlightCorner-" + child.name);
        }
    }

    public void RestoreCornerObjects()
    {
        foreach (Transform child in transform)
        {
            child.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
        }
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
