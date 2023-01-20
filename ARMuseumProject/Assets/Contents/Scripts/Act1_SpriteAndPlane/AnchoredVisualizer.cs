using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchoredVisualizer : MonoBehaviour
{
    public GameObject orbPrefab;
    private GameObject orb;

    void Start()
    {

    }

    private void OnEnable()
    {
        orb = Instantiate(orbPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        orb.SetActive(false);
        orb.transform.parent = transform;
        orb.transform.localPosition = new Vector3(0, 0, 0);
        orb.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        orb.SetActive(true);

        Invoke("EndAnimation", 14f);
    }

    public void EndAnimation()
    {
        GameObject.Destroy(orb);
        transform.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        
    }

    void Update()
    {
        
    }
}
