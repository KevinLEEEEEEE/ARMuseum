using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Scene2 : MonoBehaviour
{
    private CameraShakeInstance shake;
    private AudioSource quakeSoundPlayer;
    private Animation voxelAppear;

    void Start()
    {
        quakeSoundPlayer = transform.GetComponent<AudioSource>();
        voxelAppear = transform.GetComponent<Animation>();
    }

    public void StartScene(Vector3 point, Vector3 direction)
    {
        transform.position = point;
        transform.forward = direction;

        StartCoroutine("OpeningScene");
    }

    private IEnumerator OpeningScene()
    {
        ShowSceneObjects();
        quakeSoundPlayer.Play();

        yield return new WaitForSeconds(0.5f);

        shake = CameraShaker.Instance.StartShake(1f, 5f, 2f);

        yield return new WaitForSeconds(0.75f);

        voxelAppear.Play();

        yield return new WaitForSeconds(2f);

        shake.StartFadeOut(2.5f);
    }

    private void ShowSceneObjects()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    } 
}
