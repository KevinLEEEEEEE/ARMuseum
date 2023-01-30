using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Scene2 : MonoBehaviour
{
    private CameraShakeInstance shake;


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

        shake = CameraShaker.Instance.StartShake(1f, 3.5f, .1f);

        yield return new WaitForSeconds(3f);

        //shake.StartFadeOut(2f);
    }

    void Update()
    {
        
    }
}
