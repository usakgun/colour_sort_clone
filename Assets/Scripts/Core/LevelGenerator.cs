using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    public static List<BottleModel> GenerateLevel(int colorCount, int emptyBottleCount, int capacity = 4)
    {
        int totalBottles = colorCount + emptyBottleCount;
        List<ColorType>[] tempBottles = new List<ColorType>[totalBottles];

        for (int i = 0; i < totalBottles; i++)
        {
            tempBottles[i] = new List<ColorType>(capacity);
        }

        List<ColorType> colorPool = new List<ColorType>();
        for (int i = 1; i <= colorCount; i++)
        {
            ColorType color = (ColorType)i;
            for (int j = 0; j < capacity; j++)
            {
                colorPool.Add(color);
            }
        }

        for (int i = 0; i < colorPool.Count; i++)
        {
            ColorType temp = colorPool[i];
            int randomIndex = Random.Range(i, colorPool.Count);
            colorPool[i] = colorPool[randomIndex];
            colorPool[randomIndex] = temp;
        }

        int poolIndex = 0;
        for (int i = 0; i < colorCount; i++)
        {
            for (int j = 0; j < capacity; j++)
            {
                tempBottles[i].Add(colorPool[poolIndex]);
                poolIndex++;
            }
        }

        for (int i = 0; i < colorCount; i++)
        {
            bool allSame = true;
            ColorType first = tempBottles[i][0];
            for (int j = 1; j < capacity; j++)
            {
                if (tempBottles[i][j] != first)
                {
                    allSame = false;
                    break;
                }
            }

            if (allSame)
            {
                int nextBottle = (i + 1) % colorCount;
                ColorType swap = tempBottles[i][capacity - 1];
                tempBottles[i][capacity - 1] = tempBottles[nextBottle][0];
                tempBottles[nextBottle][0] = swap;
            }
        }

        List<BottleModel> models = new List<BottleModel>();
        for (int i = 0; i < totalBottles; i++)
        {
            BottleModel model = new BottleModel(i, capacity);
            for (int j = 0; j < tempBottles[i].Count; j++)
            {
                model.ForceAddColor(tempBottles[i][j]);
            }
            models.Add(model);
        }

        return models;
    }
}