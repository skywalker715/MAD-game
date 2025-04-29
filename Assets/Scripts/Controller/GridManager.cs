using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public RectTransform cardGrid;

    public void GenerateGrid(int totalPairs, int columns)
    {
        foreach (Transform child in cardGrid)
        {
            Destroy(child.gameObject);
        }

        Sprite[] allSprites = Resources.LoadAll<Sprite>("Cards");
        List<Sprite> selected = new List<Sprite>();

        // Pick random unique pairs
        List<int> usedIndexes = new List<int>();
        while (selected.Count < totalPairs)
        {
            int randIndex = Random.Range(0, allSprites.Length);
            if (!usedIndexes.Contains(randIndex))
            {
                usedIndexes.Add(randIndex);
                selected.Add(allSprites[randIndex]);
            }
        }

        // Duplicate for matching and shuffle
        List<(Sprite, int)> cardDataList = new List<(Sprite, int)>();
        for (int i = 0; i < selected.Count; i++)
        {
            cardDataList.Add((selected[i], i));
            cardDataList.Add((selected[i], i));
        }

        Shuffle(cardDataList);

        GridLayoutGroup layout = cardGrid.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;

        float spacing = layout.spacing.x;
        float availableWidth = cardGrid.rect.width - (columns - 1) * spacing;
        float cardWidth = availableWidth / columns;
        layout.cellSize = new Vector2(cardWidth, cardWidth);

        foreach (var data in cardDataList)
        {
            GameObject go = Instantiate(cardPrefab, cardGrid);
            Card card = go.GetComponent<Card>();
            card.SetCard(data.Item1, data.Item2);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}