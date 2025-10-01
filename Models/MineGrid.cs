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
}

internal enum RevealResult
{
    Invalid,
    Empty,
    Mine,
}

internal sealed class MineGrid : Grid2D<CellType>
{
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

    public RevealResult Reveal(int x, int y)
    {
        switch (this[x,y])
        {
            case CellType.HiddenEmpty:
                int count = NeighborCount(x, y);
                this[x, y] = count switch
                {
                    0 => CellType.ShownEmpty,
                    1 => CellType.One,
                    2 => CellType.Two,
                    3 => CellType.Three,
                    4 => CellType.Four,
                    _ => throw new NotImplementedException($"Invalid neighbor count {count}.")
                };
                return RevealResult.Empty;
            case CellType.HiddenMine:
                this[x, y] = CellType.ShownMine;
                return RevealResult.Mine;
            default:
                return RevealResult.Invalid;
        }
    }

    public RevealResult Reveal(Vec2I position)
    {
        return Reveal(position.X, position.Y);
    }

    public RevealResult PlaceFlag(int x, int y)
    {
        switch (this[x, y])
        {
            case CellType.HiddenEmpty:
                this[x, y] = CellType.FlagEmpty;
                return RevealResult.Empty;
            case CellType.HiddenMine:
                this[x, y] = CellType.FlagMine;
                return RevealResult.Mine;
            default:
                return RevealResult.Invalid;
        }
    }

    public RevealResult PlaceFlag(Vec2I position)
    {
        return PlaceFlag(position.X, position.Y);
    }

    private static bool IsMine(CellType type)
    {
        return type is CellType.HiddenMine or CellType.ShownMine or CellType.FlagMine;
    }

    private int NeighborCount(int x, int y)
    {
        int count = 0;
        if (x > 0 && IsMine(this[x - 1, y]))
        {
            count++;
        }
        if (x < Width - 1 && IsMine(this[x + 1, y]))
        {
            count++;
        }
        if (y > 0 && IsMine(this[x, y - 1]))
        {
            count++;
        }
        if (y < Height - 1 && IsMine(this[x, y + 1]))
        {
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