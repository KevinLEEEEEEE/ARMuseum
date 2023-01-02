using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class InfoOrb : MonoBehaviour
{
    public AudioClip OpenOrb;
    public AudioClip CloseOrb;

    private bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Player] Trigger Info Contact: " + transform.name);

        if(isActive)
        {
            SendMessageUpwards("OpenOrbMessage");
        } else
        {
            SendMessageUpwards("CloseOrbMessage");
        }

        isActive = !isActive;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
