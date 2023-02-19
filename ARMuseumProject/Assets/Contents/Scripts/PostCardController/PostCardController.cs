using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PostCardController : MonoBehaviour
{
    public GameObject PostCard;
    public TextMeshProUGUI textMesh;

    private void Start()
    {
        HidePostCard();
    }

    public void SetUserID(string id)
    {
        textMesh.text = id;
    }

    public void ShowPostCard()
    {
        PostCard.SetActive(true);
    }

    public void HidePostCard()
    {
        PostCard.SetActive(false);
    }
}
