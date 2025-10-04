using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Minesweeper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Interactivity;

namespace Minesweeper.Views;

public partial class MainWindow : Window
{
    private class GameInfo(int rows, int columns, int mines)
    {
        public int Rows    { get; } = rows;
        public int Columns { get; } = columns;
        public int Mines   { get; } = mines;
    }

    private const string TileDirectory = "minesweeper_tiles";

    private const double RestartDelay = 1.0;

    private const Key LockKey = Key.Q;

    private const int Mines   = 60;

    private const int Rows    = 16;
    private const int Columns = 16;

    private const int RectWidth  = 40;
    private const int RectHeight = 40;

    private static readonly GameInfo[] DifficultyModes =
    [
        new GameInfo(8 , 8 , 10),
        new GameInfo(16, 16, 40),
        new GameInfo(16, 30, 99),
    ];

    private readonly MineGrid _mineGrid = new(Rows, Columns);

    private readonly Grid2D<Image> _gridChildren = new(Rows, Columns);

    private readonly Dictionary<string, Bitmap> _bitmaps;

    private readonly Stopwatch _restartDelay = new();

    private readonly KeyStateLogger _keyState = new();

    private int _difficultyMode = 1;

    private bool _started = false;

    private bool _active = true;

    private bool _ctrlLock = false;

    public MainWindow()
    {
        InitializeComponent();

        InitializeInput();

        _bitmaps = LoadBitmaps(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TileDirectory));

        InitializeGrid();

        InfoText.Text = "Click Anywhere to Start";
    }

    private void PointerPressedHandler(object sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(sender as Control);

        var gridPosition = new Vec2I()
        {
            X = (int)(point.Position.X / RectWidth ),
            Y = (int)(point.Position.Y / RectHeight)
        };

        // Reset game
        if (!_active)
        {
            if (_restartDelay.Elapsed.TotalSeconds < RestartDelay)
            {
                return;
            }
            ResetGame();
            return;
        }

        var update = false;

        if (point.Properties.IsLeftButtonPressed)
        {
            if (_ctrlLock && !_keyState[Key.LeftCtrl])
            {
                return;
            }

            // Start game
            if (!_started)
            {
                StartGame(gridPosition);
                return;
            }
                
            update = _mineGrid.Reveal(gridPosition);

            if (update && _mineGrid[gridPosition] == CellType.ShownEmpty)
            {
                UpdateRange(_mineGrid.ClearOut(gridPosition));

                update = false;
            }

            if (_mineGrid[gridPosition] == CellType.ShownMine)
            {
                _active = false;
                _restartDelay.Start();

                foreach (var position in _mineGrid.HiddenMines)
                {
                    _mineGrid[position] = CellType.ShownMine;
                    Update(position);
                }

                InfoText.Text = "Game Over!";
            }
        }

        if (point.Properties.IsRightButtonPressed)
        {
            if (MineGrid.IsFlag(_mineGrid[gridPosition]))
            {
                update = _mineGrid.RemoveFlag(gridPosition);
            }
            else
            {
                update = _mineGrid.PlaceFlag(gridPosition);

                if (_mineGrid.InvalidFlags == 0 && _mineGrid.HiddenMines.Count == 0)
                {
                    _active = false;
                    _restartDelay.Start();
                }
            }

            UpdateFlagInfo();
        }

        if (!update)
        {
            return;
        }

        Update(gridPosition);
    }

    #region KeyHandlers

    private void KeyDownHandler(object? sender, KeyEventArgs args)
    {
        _keyState.OnKeyDown(args.Key);

        if (args.Key == LockKey)
        {
            _ctrlLock = !_ctrlLock;
        }    
    }
        
    private void KeyUpHandler(object? sender, KeyEventArgs args)
    {
        _keyState.OnKeyUp(args.Key);
    }

    #endregion

    #region Update

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

    #endregion

    private void ResetGame()
    {
        _mineGrid.Reset();
        UpdateAll();

        _started = false;

        _restartDelay.Reset();

        _active = true;
    }

    private void StartGame(Vec2I position)
    {
        _mineGrid.GenerateGame(position, DifficultyModes[_difficultyMode].Mines);

        UpdateRange(_mineGrid.ClearOut(position));

        UpdateFlagInfo();

        _started = true;
    }

    private void Combobox_SelectedChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (sender is not ComboBox comboBox)
        {
            return;
        }

        if (_started || !_active)
        {
            ResetGame();
        }

        if (comboBox.SelectedIndex == _difficultyMode)
        {
            return;
        }

        _difficultyMode = comboBox.SelectedIndex;

        InitializeGrid();
    }

    private void UpdateFlagInfo()
    {
        InfoText.Text = $"Flags: {_mineGrid.FlagsRemaining}";
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

    private void InitializeGrid()
    {
        var mode = DifficultyModes[_difficultyMode];

        MainGrid.Height = mode.Rows    * RectHeight;
        MainGrid.Width  = mode.Columns * RectWidth;

        MainGrid.Children.Clear();

        _mineGrid.Resize(mode.Rows, mode.Columns);
        _gridChildren.Resize(mode.Rows, mode.Columns);

        for (int j = 0; j < mode.Rows; j++)
        {
            MainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                
            for (int i = 0; i < mode.Columns; i++)
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

    private void InitializeInput()
    {
        AddHandler(KeyDownEvent, KeyDownHandler, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent  , KeyUpHandler  , RoutingStrategies.Tunnel);
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