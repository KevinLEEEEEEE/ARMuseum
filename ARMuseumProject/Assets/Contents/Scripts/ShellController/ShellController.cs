using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    public GameObject blocks;

    // Start is called before the first frame update
    void Start()
    {
        blocks.SetActive(false);
    }

    public void SetTransform(Vector3 point, Vector3 direction)
    {
        transform.position = point;
        transform.forward = direction;
    }

    public void InitShell()
    {
        blocks.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
