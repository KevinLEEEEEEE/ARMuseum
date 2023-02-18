using UnityEngine;
using TMPro;

public class FPSManager : MonoBehaviour
{
    public bool showFPS = true;
    public float updateInterval = 1f;
    public TextMeshProUGUI textMesh;

    private float lastInterval;

    void Start()
    {
        textMesh.gameObject.SetActive(showFPS);
        lastInterval = Time.realtimeSinceStartup;     
    }
    void Update()
    {
        if(showFPS)
        {
            float timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                lastInterval = timeNow;
                textMesh.text = (1f / Time.unscaledDeltaTime).ToString("F2");
            }
        }
    }
}