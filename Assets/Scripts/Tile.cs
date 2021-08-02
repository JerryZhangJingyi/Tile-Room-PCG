using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public char tileType;

    [HideInInspector] public Vector2Int roomPosition;
    [HideInInspector] public Room room;

    [HideInInspector] public Unit unit;
    [HideInInspector] public Upgrade upgrade;
    [HideInInspector] public Goal goal;
    [HideInInspector] public bool hasSpear = false;

    [HideInInspector] public Vector2Int BoardPosition
    {
        get
        {
            return roomPosition + room.originBoardPosition;
        }
    }

    public bool IsAdjacentTo(Tile tile)
    {
        if(room != tile.room)
        {
            return false;
        }
        Vector2Int delta = roomPosition - tile.roomPosition;

        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
        return distance == 1;
    }

    public bool IsTwoRadius(Tile tile)
    {
        if (room != tile.room)
        {
            return false;
        }
        Vector2Int delta = roomPosition - tile.roomPosition;

        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
        return distance == 2;
    }

    public bool IsThreeUnits(Tile tile)
    {
        if (room != tile.room)
        {
            return false;
        }
        Vector2Int delta = roomPosition - tile.roomPosition;

        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
        return distance == 3;
    }

    public bool IsFloorTile => tileType == ' ' || tileType == 'c';

    public bool IsLavaTile => tileType == 't';

    public bool IsDoorTile => tileType == 'N' || tileType == 'S' || tileType == 'W' || tileType == 'E';

}

