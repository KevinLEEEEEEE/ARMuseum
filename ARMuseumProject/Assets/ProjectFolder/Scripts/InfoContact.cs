using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class InfoContact : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(transform.name + ":" + other.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
