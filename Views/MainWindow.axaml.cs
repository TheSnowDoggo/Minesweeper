using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Minesweeper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Minesweeper.Views
{
    public partial class MainWindow : Window
    {
        private const int Rows    = 15;
        private const int Columns = 15;

        private const double RestartDelay = 1.0;

        private const string BitmapDirectory = @"C:\Users\redst\source\repos\Minesweeper\minesweeper_images\";

        private const string BitmapDirLinux = @"/home/luna-sparkle/RiderProjects/Minesweeper/minesweeper_images";

        private const int RectWidth  = 40;
        private const int RectHeight = 40;

        private readonly MineGrid _mineGrid = new(Rows, Columns);

        private readonly Grid2D<Image> _gridChildren = new(Rows, Columns);

        private readonly Dictionary<string, Bitmap> _bitmaps;

        private readonly Stopwatch _restartDelay = new();

        private bool started = false;

        private bool active = true;

        public MainWindow()
        {
            InitializeComponent();
            
            _bitmaps = LoadBitmaps(BitmapDirectory);

            InitializeGrid(Rows, Columns, RectWidth, RectHeight);
        }

        private void PointerPressedHandler(object sender, PointerPressedEventArgs args)
        {
            var point = args.GetCurrentPoint(sender as Control);

            var gridPosition = new Vec2I()
            {
                X = (int)(point.Position.X / RectWidth),
                Y = (int)(point.Position.Y / RectHeight)
            };

            if (!active)
            {
                if (_restartDelay.Elapsed.TotalSeconds < RestartDelay)
                {
                    return;
                }

                _mineGrid.Reset();
                UpdateAll();

                started = false;

                _restartDelay.Reset();

                active = true;
                return;
            }

            var changed = false;

            if (point.Properties.IsLeftButtonPressed)
            {
                if (!started)
                {
                    _mineGrid.GenerateGame(gridPosition, 40);

                    UpdateRange(_mineGrid.ClearOut(gridPosition));

                    started = true;
                    return;
                }

                changed = _mineGrid.Reveal(gridPosition);

                if (changed && _mineGrid[gridPosition] == CellType.ShownEmpty)
                {
                    UpdateRange(_mineGrid.ClearOut(gridPosition));

                    changed = false;
                }

                if (_mineGrid[gridPosition] == CellType.ShownMine)
                {
                    active = false;
                    _restartDelay.Start();

                    foreach (var position in _mineGrid.HiddenMines)
                    {
                        _mineGrid[position] = CellType.ShownMine;
                        Update(position);
                    }
                }
            }

            if (point.Properties.IsRightButtonPressed)
            {
                if (MineGrid.IsFlag(_mineGrid[gridPosition]))
                {
                    changed = _mineGrid.RemoveFlag(gridPosition);
                }
                else
                {
                    changed = _mineGrid.PlaceFlag(gridPosition);

                    if (_mineGrid.InvalidFlags == 0 && _mineGrid.HiddenMines.Count == 0)
                    {
                        active = false;
                        _restartDelay.Start();
                    }
                }
            }

            if (!changed)
            {
                return;
            }

            Update(gridPosition);
        }

        private void Update(int x, int y)
        {
            _gridChildren[x, y].Source = GetBitmap(_mineGrid[x, y]);
        }

        private void UpdateRange(IEnumerable<Vec2I> collection)
        {
            foreach (Vec2I position in collection)
            {
                Update(position);
            }
        }

        private void Update(Vec2I position)
        {
            Update(position.X, position.Y);
        }

        private void UpdateAll()
        {
            for (int y = 0; y < _mineGrid.Height; y++)
            {
                for (int x = 0; x < _mineGrid.Width; x++)
                {
                    Update(x, y);
                }
            }
        }

        private Bitmap GetBitmap(CellType cellType)
        {
            var bitmap = cellType switch
            {
                CellType.HiddenEmpty or
                CellType.HiddenMine  => _bitmaps["tile_hidden"],
                CellType.ShownEmpty  => _bitmaps["tile_empty" ],
                CellType.ShownMine   => _bitmaps["tile_mine"  ],
                CellType.FlagEmpty   or
                CellType.FlagMine    => _bitmaps["tile_flag"  ],
                CellType.One         => _bitmaps["tile_one"   ],
                CellType.Two         => _bitmaps["tile_two"   ],
                CellType.Three       => _bitmaps["tile_three" ],
                CellType.Four        => _bitmaps["tile_four"  ],
                CellType.Five        => _bitmaps["tile_five"  ],
                CellType.Six         => _bitmaps["tile_six"   ],
                CellType.Seven       => _bitmaps["tile_seven" ],
                CellType.Eight       => _bitmaps["tile_eight" ],
                _ => throw new NotImplementedException("Unimplimented CellType")
            };
            return bitmap;
        }

        private void InitializeGrid(int rows, int columns, int rectHeight, int rectWidth)
        {
            MainGrid.Height = rows    * rectHeight;
            MainGrid.Width  = columns * rectWidth;

            //MainGrid.ShowGridLines = true;

            for (int j = 0; j < Rows; j++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                
                for (int i = 0; i < Columns; i++)
                {
                    if (j == 0)
                    {
                        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                    }

                    var child = new Image()
                    {
                        Width = RectWidth, Height = RectHeight,
                        Source = GetBitmap(_mineGrid[i, j]),
                    };

                    _gridChildren[i, j] = child;

                    Grid.SetRow(child, j);
                    Grid.SetColumn(child, i);

                    MainGrid.Children.Add(child);
                }
            }
        }

        private static Dictionary<string, Bitmap> LoadBitmaps(string directory)
        {
            var dict = new Dictionary<string, Bitmap>();

            foreach (var filePath in Directory.GetFiles(directory))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(filePath);

                dict.Add(name, new Bitmap(filePath));
            }

            return dict;
        }
    }
}