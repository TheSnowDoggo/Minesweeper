using System;
using System.Collections.Generic;

namespace Minesweeper.Models;

internal enum CellType : byte
{
    HiddenEmpty,
    HiddenMine,
    ShownEmpty,
    ShownMine,
    FlagEmpty,
    FlagMine,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
}

internal enum RevealResult
{
    Invalid,
    Empty,
    Mine,
}

internal sealed class MineGrid : Grid2D<CellType>
{
    private static readonly Vec2I[] Moves =
    [
        Vec2I.Up,
        Vec2I.Down,
        Vec2I.Left,
        Vec2I.Right,
        Vec2I.UpLeft,
        Vec2I.UpRight,
        Vec2I.DownLeft,
        Vec2I.DownRight,
    ];

    private readonly Random _rand = new();

    private int _invalidFlags = 0;

    private int _mines = 0;

    public MineGrid(int width, int height)
        : base(width, height)
    {
    }

    public HashSet<Vec2I> HiddenMines { get; } = [];

    public int Mines => _mines;

    public int InvalidFlags => _invalidFlags;

    public int FlagsRemaining => HiddenMines.Count - InvalidFlags;

    public static bool IsFlag(CellType cellType)
    {
        return cellType is CellType.FlagEmpty or CellType.FlagMine;
    }

    public static bool IsMine(CellType cellType)
    {
        return cellType is CellType.HiddenMine or CellType.ShownMine or CellType.FlagMine;
    }

    public void Reset()
    {
        Clear();
        HiddenMines.Clear();
        _mines = 0;
        _invalidFlags = 0;
    }

    public void GenerateGame(Vec2I start, int mines)
    {
        HiddenMines.Clear();

        var positions = GeneratePositions(start, mines);

        _mines = positions.Count;

        foreach (Vec2I position in positions)
        {
            this[position] = CellType.HiddenMine;
            HiddenMines.Add(position);
        }
    }

    public HashSet<Vec2I> ClearOut(Vec2I start)
    {
        var positions = new HashSet<Vec2I>();

        var stack = new Stack<Vec2I>();
        stack.Push(start);

        while (stack.TryPop(out Vec2I position))
        {
            positions.Add(position);

            if (Reveal(position) && this[position] != CellType.ShownEmpty)
            {
                continue;
            }

            foreach (Vec2I move in Moves)
            {
                Vec2I next = position + move;

                if (!InRange(next) || this[next] != CellType.HiddenEmpty || positions.Contains(next))
                {
                    continue;
                }

                stack.Push(next);
            }
        }

        return positions;
    }

    public bool Reveal(Vec2I position)
    {
        switch (this[position])
        {
            case CellType.HiddenEmpty:
                var count = NeighborCount(position);
                this[position] = count switch
                {
                    0 => CellType.ShownEmpty,
                    1 => CellType.One,
                    2 => CellType.Two,
                    3 => CellType.Three,
                    4 => CellType.Four,
                    5 => CellType.Five,
                    6 => CellType.Six,
                    7 => CellType.Seven,
                    8 => CellType.Eight,
                    _ => throw new NotImplementedException($"Invalid neighbor count {count}.")
                };
                return true;
            case CellType.HiddenMine:
                this[position] = CellType.ShownMine;
                return true;
            default:
                return false;
        }
    }

    public bool PlaceFlag(Vec2I position)
    {
        switch (this[position])
        {
            case CellType.HiddenEmpty:
                this[position] = CellType.FlagEmpty;
                _invalidFlags++;
                return true;
            case CellType.HiddenMine:
                this[position] = CellType.FlagMine;
                HiddenMines.Remove(position);
                return true;
            default:
                return false;
        }
    }

    public bool RemoveFlag(Vec2I position)
    {
        switch (this[position])
        {
            case CellType.FlagEmpty:
                this[position] = CellType.HiddenEmpty;
                _invalidFlags--;
                return true;
            case CellType.FlagMine:
                this[position] = CellType.HiddenMine;
                HiddenMines.Add(position);
                return true;
            default:
                return false;
        }
    }

    private int NeighborCount(Vec2I position)
    {
        var count = 0;
        foreach (var move in Moves)
        {
            var next = position + move;
            if (!InRange(next) || !IsMine(this[next]))
            {
                continue;
            }
            count++;
        }
        return count;
    }

    private HashSet<Vec2I> GeneratePositions(Vec2I start, int count)
    {
        if (count > Size)
        {
            throw new ArgumentException("Position count exceeds dimensions.");
        }

        var positions = new HashSet<Vec2I>(count);

        var disallowed = new HashSet<Vec2I>() { start };

        foreach (Vec2I move in Moves)
        {
            Vec2I next = start + move;
            if (InRange(next))
            {
                disallowed.Add(next);
            }
        }

        for (int i = 0; i < count; i++)
        {
            Vec2I position;
            while (true)
            {
                position = new Vec2I(_rand.Next(Width), _rand.Next(Height));

                if (positions.Contains(position) ||
                    disallowed.Contains(position))
                {
                    continue;
                }

                break;
            }

            positions.Add(position);
        }

        return positions;
    }
}