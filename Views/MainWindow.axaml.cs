using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Minesweeper.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Minesweeper.Views
{
    public partial class MainWindow : Window
    {
        private const int Rows    = 35;
        private const int Columns = 35;

        private const string BitmapDirectory = @"C:\Users\redst\Documents\minesweeper_images";

        private const int RectWidth  = 20;
        private const int RectHeight = 20;

        private readonly MineGrid _mineGrid = new(Rows, Columns);

        private readonly Random _rand = new();

        private readonly Grid2D<Image> _gridChildren = new(Rows, Columns);

        private readonly Dictionary<string, Bitmap> _bitmaps;

        public MainWindow()
        {
            InitializeComponent();

            InitializeGrid(Rows, Columns, RectWidth, RectHeight);

            _bitmaps = LoadBitmaps(BitmapDirectory);

            _mineGrid.GenerateMines(_rand, 180);

            foreach (var position in _mineGrid.GeneratePositions(_rand, 700))
            {
                if (_rand.Next(50) == 0)
                {
                    _mineGrid.PlaceFlag(position);
                }
                else
                {
                    _mineGrid.Reveal(position);
                }
            }

            for (int y = 0; y < _mineGrid.Height; y++)
            {
                for (int x = 0; x < _mineGrid.Width; x++)
                {
                    Bitmap bitmap = _mineGrid[x, y] switch
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
                        _ => throw new NotImplementedException("Unimplimented CellType")
                    };

                    _gridChildren[x, y].Source = bitmap;
                }
            }
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
                        Source = new Bitmap(@"C:\Users\redst\Documents\minesweeper_images\tile_one.png"),
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

            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(filePath);

                dict.Add(name, new Bitmap(filePath));
            }

            return dict;
        }
    }
}