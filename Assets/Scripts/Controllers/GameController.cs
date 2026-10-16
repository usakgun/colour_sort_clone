using System.Collections.Generic;

public class GameController
{
    private readonly List<BottleModel> _bottles;
    private readonly CommandInvoker _commandInvoker;

    public GameController(List<BottleModel> bottles)
    {
        _bottles = bottles;
        _commandInvoker = new CommandInvoker();
    }

    public void AddExtraBottle(BottleModel newBottle)
    {
        _bottles.Add(newBottle);
    }

    public bool TryPour(BottleModel source, BottleModel destination)
    {
        ICommand pourCommand = new PourCommand(source, destination);
        return _commandInvoker.ExecuteCommand(pourCommand);
    }

    public bool TryUndo()
    {
        return _commandInvoker.UndoLastCommand();
    }

    public bool CheckWinCondition()
    {
        foreach (var bottle in _bottles)
        {
            if (!bottle.IsEmpty && !bottle.IsCompleted)
            {
                return false;
            }
        }
        return true;
    }

    public bool HasAnyValidMove()
    {
        for (int i = 0; i < _bottles.Count; i++)
        {
            BottleModel src = _bottles[i];
            if (src.IsEmpty || src.IsCompleted) continue;

            int srcTopStreak = GetTopColorStreak(src);

            for (int j = 0; j < _bottles.Count; j++)
            {
                if (i == j) continue;
                BottleModel dst = _bottles[j];

                if (!dst.CanAddColor(src.TopColor)) continue;

                if (dst.IsEmpty && IsUniformColor(src)) continue;

                int dstFreeSpace = dst.Capacity - dst.Colors.Count;
                if (dstFreeSpace >= srcTopStreak)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int GetTopColorStreak(BottleModel bottle)
    {
        if (bottle.IsEmpty) return 0;
        ColorType top = bottle.TopColor;
        int count = 0;
        foreach (var c in bottle.Colors)
        {
            if (c == top) count++;
            else break;
        }
        return count;
    }

    private bool IsUniformColor(BottleModel bottle)
    {
        if (bottle.IsEmpty) return true;
        ColorType first = bottle.TopColor;
        foreach (var c in bottle.Colors)
        {
            if (c != first) return false;
        }
        return true;
    }
}