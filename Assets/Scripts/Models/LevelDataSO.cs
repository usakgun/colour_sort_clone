using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BottleConfig
{
    public List<ColorType> initialColors = new List<ColorType>();
}

[CreateAssetMenu(fileName = "Level_1", menuName = "MagicSort/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public int bottleCapacity = 4;
    public List<BottleConfig> bottles = new List<BottleConfig>();
}