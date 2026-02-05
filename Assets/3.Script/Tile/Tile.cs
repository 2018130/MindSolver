using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile : IComparable<Tile>
{
    public Vector2Int index;
    public int f, g, h;
    public bool closed = false;
    public bool canMove = false;

    public Tile preTile;

    public Tile(Vector2Int index, bool canMove = false)
    {
        this.index = index;
        this.canMove = canMove;
    }

    public int CompareTo(Tile other)
    {
        return f > other.f ? 1 : -1;
    }
}