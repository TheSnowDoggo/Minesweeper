using System;
using System.Collections;
using System.Collections.Generic;

namespace Minesweeper.Models;

internal sealed class Grid2DView<T>(Grid2D<T> grid) : IEnumerable<T>,
    ICloneable
{
    private readonly Grid2D<T> _grid = grid;

    public int Width  => _grid.Width;

    public int Height => _grid.Height;

    public int Size   => _grid.Size;

    public Vec2I Dimensions => _grid.Dimensions;

    public T this[int x, int y] => _grid[x, y];

    public T this[Vec2I pos]    => _grid[pos];

    public override string ToString()
    {
        return _grid.ToString();
    }

    public Rect2DI Area() => _grid.Area();

    #region IEnumerable

    public IEnumerator<T> GetEnumerator()
    {
        return _grid.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _grid.GetEnumerator();
    }

    #endregion

    #region ICloneable

    public object Clone()
    {
        return _grid.Clone();
    }

    #endregion
}