using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Minesweeper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Input;

namespace Minesweeper.Views
{
    public partial class MainWindow : Window
    {
        private const int Rows    = 15;
        private const int Columns = 15;

        private const string BitmapDirectory = @"C:\Users\redst\Documents\minesweeper_images";

        private const string BitmapDirLinux = @"/home/luna-sparkle/RiderProjects/Minesweeper/minesweeper_images";

        private const int RectWidth  = 40;
        private const int RectHeight = 40;

        private readonly MineGrid _mineGrid = new(Rows, Columns);

        private readonly Random _rand = new();

        private readonly Grid2D<Image> _gridChildren = new(Rows, Columns);

        private readonly Dictionary<string, Bitmap> _bitmaps;

        public MainWindow()
        {
            InitializeComponent();
            
            _bitmaps = LoadBitmaps(BitmapDirLinux);
            
            _mineGrid.GenerateMines(_rand, 60);

            InitializeGrid(Rows, Columns, RectWidth, RectHeight);
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

        private void PointerPressedHandler(object sender, PointerPressedEventArgs args)
        {
            var point = args.GetCurrentPoint(sender as Control);

            var gridPosition = new Vec2I()
            {
                X = (int)(point.Position.X / RectWidth),
                Y = (int)(point.Position.Y / RectHeight)
            };

            var changed = false;

            if (point.Properties.IsLeftButtonPressed)
            {
                changed = _mineGrid.Reveal(gridPosition);
            }

            if (point.Properties.IsRightButtonPressed)
            {
                changed = _mineGrid.PlaceFlag(gridPosition);
            }

            if (!changed)
            {
                return;
            }
            
            _gridChildren[gridPosition].Source = GetBitmap(_mineGrid[gridPosition]);
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