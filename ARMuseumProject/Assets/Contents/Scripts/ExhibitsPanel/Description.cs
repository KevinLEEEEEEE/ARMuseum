using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class DescriptionData
{
    public string exhibitID;
    public string title;
    public string content;
}

public class Description : MonoBehaviour
{
    [SerializeField] private GameController m_GameController;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private DescriptionData[] descriptionData;

    private string currentID;

    void Awake()
    {
        m_GameController.BeginTourEvent += ShowRoot;
        m_GameController.EndTourEvent += HideRoot;

        Reset();
    }

    private void ShowRoot()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void HideRoot()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        Reset();
    }

    private void Reset()
    {
        currentID = "";
        title.text = "";
        content.text = "";
    }

    public void HoverExhibit(string id)
    {
        foreach(DescriptionData data in descriptionData)
        {
            currentID = id;

            if(data.exhibitID == id)
            {
                title.text = data.title;
                content.text = data.content;
            }
        }
    }

    public void ExitExhibit(string id)
    {
        if(currentID == id)
        {
            Reset();
        }
    }
}
