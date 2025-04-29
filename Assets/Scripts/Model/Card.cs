using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image frontImage;
    public Image backImage;

    public int cardId;
    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;

    public void SetCard(Sprite image, int id)
    {
        frontImage.sprite = image;
        cardId = id;
        FlipCard(false);
    }

    public void OnClick()
    {
        if (IsMatched || IsFlipped || GameManager.Instance == null)
            return;

        GameManager.Instance.CheckCard(this);
    }

    public void FlipCard()
    {
        IsFlipped = !IsFlipped;
        frontImage.gameObject.SetActive(IsFlipped);
        backImage.gameObject.SetActive(!IsFlipped);
    }

    public void FlipCard(bool showFront)
    {
        IsFlipped = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
    }

    public void SetMatched(bool matched)
    {
        IsMatched = matched;
        if (matched)
        {
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(false);
        }
    }
}