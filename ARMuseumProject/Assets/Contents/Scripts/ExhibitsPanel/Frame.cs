using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class Frame : MonoBehaviour
{
    public void ActiveFrames()
    {
        SwitchFrameState(true);
    }

    public void InactiveFrames()
    {
        SwitchFrameState(false);
    }

    public void HightlightFrames()
    {
        foreach (Transform child in transform)
        {
            child.GetChild(0).GetComponent<Animation>().Play("HightlightCorner-" + child.name);
        }
    }

    public void RestoreFrames()
    {
        foreach (Transform child in transform)
        {
            child.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    private void SwitchFrameState(bool state)
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }

    public void ResetAll()
    {
        SwitchFrameState(false);
    }
}
