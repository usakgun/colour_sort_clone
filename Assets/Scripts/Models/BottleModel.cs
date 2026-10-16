using System.Collections.Generic;

public class BottleModel
{
    public int Capacity { get; private set; }
    public Stack<ColorType> Colors { get; private set; }
    public int Id { get; private set; }

    public BottleModel(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Colors = new Stack<ColorType>(capacity);
    }

    public bool IsFull => Colors.Count >= Capacity;
    public bool IsEmpty => Colors.Count == 0;
    public ColorType TopColor => IsEmpty ? ColorType.None : Colors.Peek();
    public bool IsCompleted => IsFull && IsAllSameColor();

    public bool CanAddColor(ColorType color)
    {
        if (IsFull) return false;
        if (IsEmpty) return true;
        return TopColor == color;
    }

    public void AddColor(ColorType color)
    {
        if (CanAddColor(color))
        {
            Colors.Push(color);
        }
    }

    public void ForceAddColor(ColorType color)
    {
        if (!IsFull)
        {
            Colors.Push(color);
        }
    }

    public ColorType RemoveColor()
    {
        return IsEmpty ? ColorType.None : Colors.Pop();
    }

    private bool IsAllSameColor()
    {
        if (IsEmpty) return false;
        
        ColorType firstColor = TopColor;
        foreach (var color in Colors)
        {
            if (color != firstColor) return false;
        }
        return true;
    }
}