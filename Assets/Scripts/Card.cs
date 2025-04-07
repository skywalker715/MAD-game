using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image frontImage;
    public Image backImage;

    [HideInInspector] public int cardId;
    [HideInInspector] public bool isFlipped = false;
    [HideInInspector] public bool isMatched = false;

    public void SetCard(int id, Sprite frontSprite)
    {
        cardId = id;
        frontImage.sprite = frontSprite;
        SetMatched(false);
        isFlipped = false;
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
    }

    public void FlipCard()
    {
        if (isMatched) return;

        isFlipped = !isFlipped;
        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);
    }

    public void SetMatched(bool matched)
    {
        isMatched = matched;
        if (matched)
        {
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (isFlipped || isMatched) return;

        FlipCard();
        GameManager.Instance.CardRevealed(this);
    }
}