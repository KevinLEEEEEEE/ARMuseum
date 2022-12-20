using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null) Destroy(meshRenderer);
            MeshFilter mesh = GetComponent<MeshFilter>();
            if (mesh != null) Destroy(mesh);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
