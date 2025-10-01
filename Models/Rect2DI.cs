using System;

namespace Minesweeper.Models;

internal sealed class Rect2DI(Vec2I start, Vec2I end)
    : IEquatable<Rect2DI>
{
    public Vec2I Start = start;

    public Vec2I End   = end;

    public Rect2DI(int left, int top, int right, int bottom)
        : this(new Vec2I(left, top), new Vec2I(right, bottom))
    {
    }

    public Rect2DI(Vec2I dimensions)
        : this(Vec2I.Zero, dimensions)
    {
    }

    public Rect2DI(int width, int height)
        : this(0, 0, width, height)
    {
    }

    #region Operators

    public static bool operator ==(Rect2DI r1, Rect2DI r2) => r1.Equals(r2);

    public static bool operator !=(Rect2DI r1, Rect2DI r2) => !r1.Equals(r2);

    #endregion

    public Vec2I Size()
    {
        return End - Start;
    }

    #region Overlaps

    public bool Overlaps(int left, int top, int right, int bottom)
    {
        if (End.X < left || Start.X > right)
        {
            return false;
        }
        if (End.Y < top || Start.Y > bottom)
        {
            return false;
        }
        return true;
    }

    public bool Overlaps(Vec2I start, Vec2I end)
    {
        return Overlaps(start.X, start.Y, end.X, end.Y);
    }

    public bool Overlaps(Rect2DI other)
    {
        return Overlaps(other.Start, other.End);
    }

    #endregion

    #region TrimFrom

    public void TrimFrom(int left, int top, int right, int bottom)
    {
        Start.X = Math.Max(Start.X, left);
        Start.Y = Math.Max(Start.Y, top);
        End.X   = Math.Min(End.X, right);
        End.Y   = Math.Min(End.Y, bottom);
    }

    public void TrimFrom(Vec2I start, Vec2I end)
    {
        TrimFrom(start.X, start.Y, end.X, end.Y);
    }

    public void TrimFrom(Rect2DI other)
    {
        TrimFrom(other.Start, other.End);
    }

    #endregion

    #region Equality

    public bool Equals(Rect2DI? other)
    {
        return other is not null && Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect2DI rect && Equals(rect);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    #endregion

    public override string ToString()
    {
        return $"[{Start}, {End}]";
    }
}