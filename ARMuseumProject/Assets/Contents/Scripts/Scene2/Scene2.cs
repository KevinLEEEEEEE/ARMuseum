using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2 : MonoBehaviour
{

    void Start()
    {
        
    }

    public void StartScene(Vector3 point, Vector3 direction)
    {
        transform.position = point;
        transform.forward = direction;

        StartCoroutine("OpeningScene");
    }

    private IEnumerator OpeningScene()
    {
        yield return new WaitForSeconds(2f);
    }

    void Update()
    {
        
    }
}
