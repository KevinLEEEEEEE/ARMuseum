using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OrbMesh : MonoBehaviour
{
    public float scaleRatio;

    [SerializeField] private float scaleDuration;
    private Vector3 defaultScale;

    private void Start()
    {
        defaultScale = transform.localScale;
    }

    private void OnDisable()
    {
        Reset();
    }

    private void Reset()
    {
        transform.localScale = defaultScale;
    }

    public void EnterScale()
    {
        transform.DOScale(scaleRatio, scaleDuration);
    }

    public void ExitScale()
    {
        transform.DOScale(defaultScale.x, scaleDuration);
    }
}
