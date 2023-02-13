using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderEvent : MonoBehaviour
{
    public Action triggerEnterListener;

    private void OnTriggerEnter(Collider other)
    {
        triggerEnterListener?.Invoke();
    }
}
