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
    
    public MineGrid(int width, int height)
        : base(width, height)
    {
    }

    public void GenerateMines(Random random, int mines)
    {
        foreach (var pos in GeneratePositions(random, mines))
        {
            this[pos] = CellType.HiddenMine;
        }
    }

    public bool Reveal(int x, int y)
    {
        switch (this[x,y])
        {
            case CellType.HiddenEmpty:
                var count = NeighborCount(new Vec2I(x, y));
                this[x, y] = count switch
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
                this[x, y] = CellType.ShownMine;
                return true;
            default:
                return false;
        }
    }

    public bool Reveal(Vec2I position)
    {
        return Reveal(position.X, position.Y);
    }

    public bool PlaceFlag(int x, int y)
    {
        switch (this[x, y])
        {
            case CellType.HiddenEmpty:
                this[x, y] = CellType.FlagEmpty;
                return true;
            case CellType.HiddenMine:
                this[x, y] = CellType.FlagMine;
                return true;
            default:
                return false;
        }
    }

    public bool PlaceFlag(Vec2I position)
    {
        return PlaceFlag(position.X, position.Y);
    }

    private static bool IsMine(CellType type)
    {
        return type is CellType.HiddenMine or CellType.ShownMine or CellType.FlagMine;
    }

    private int NeighborCount(Vec2I pos)
    {
        var count = 0;
        foreach (var move in Moves)
        {
            var next = pos + move;
            if (!InRange(next) || !IsMine(this[next]))
            {
                continue;
            }
            count++;
        }
        return count;
    }

    public HashSet<Vec2I> GeneratePositions(Random random, int count)
    {
        if (count > Size)
        {
            throw new ArgumentException("Position count exceeds dimensions.");
        }

        var positions = new HashSet<Vec2I>(count);

        for (int i = 0; i < count; i++)
        {
            Vec2I pos;
            while (true)
            {
                pos = new Vec2I(random.Next(Width), random.Next(Height));

                if (!positions.Contains(pos))
                {
                    break;
                }
            }

            positions.Add(pos);
        }

        return positions;
    }
}