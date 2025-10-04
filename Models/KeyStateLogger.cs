using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace Minesweeper.Models;

internal sealed class KeyStateLogger
{
    private const int KeyStateLength = 173;

    private readonly bool[] _keyStates = new bool[173];

    public KeyStateLogger()
    {
    }

    public static bool KeySupported(Key key)
    {
        return (int)key >= 0 && (int)key < KeyStateLength;
    }

    public bool this[Key key] => IsKeyDown(key);

    public bool IsKeyDown(Key key)
    {
        if (!KeySupported(key))
        {
            throw new ArgumentException("Key is unsupported", nameof(key));
        }
        return _keyStates[(int)key];
    }

    public bool OnKeyDown(Key key)
    {
        if (!KeySupported(key))
        {
            return false;
        }
        _keyStates[(int)key] = true;
        return true;
    }

    public bool OnKeyUp(Key key)
    {
        if (!KeySupported(key))
        {
            return false;
        }
        _keyStates[(int)key] = false;
        return true;
    }

    public void Clear()
    {
        Array.Clear(_keyStates);
    }
}