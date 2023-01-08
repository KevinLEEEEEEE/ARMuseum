using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteractionHint : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        animator.Play("AirTap_R");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
