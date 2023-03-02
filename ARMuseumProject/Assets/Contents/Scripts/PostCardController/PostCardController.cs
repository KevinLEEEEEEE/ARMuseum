using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PostCardController : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private GameObject PostCard;
    [SerializeField] private TextMeshProUGUI textMesh;

    private void Start()
    {
        HidePostCard();
    }

    public void Init()
    {
        SetUserID(_gameController.UserID);
        ShowPostCard();
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
