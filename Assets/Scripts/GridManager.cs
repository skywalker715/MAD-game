using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardGrid;
    public Sprite[] cardImages; // Should be even count for pairs

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Create pair list
        List<(Sprite, int)> cardDataList = new List<(Sprite, int)>();

        for (int i = 0; i < cardImages.Length; i++)
        {
            // Each image appears twice, with same ID
            cardDataList.Add((cardImages[i], i));
            cardDataList.Add((cardImages[i], i));
        }

        // Shuffle list
        for (int i = 0; i < cardDataList.Count; i++)
        {
            var temp = cardDataList[i];
            int randIndex = Random.Range(i, cardDataList.Count);
            cardDataList[i] = cardDataList[randIndex];
            cardDataList[randIndex] = temp;
        }

        // Instantiate cards
        foreach (var data in cardDataList)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardGrid);
            Card card = cardGO.GetComponent<Card>();
            card.SetCard(data.Item2, data.Item1);
        }
    }
}