using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [HideInInspector] public Vector2Int originBoardPosition;

    [TextArea] public string strRoom;

    [HideInInspector] public List<Tile> tiles = new List<Tile>();
    [HideInInspector] public List<Door> doors = new List<Door>();

    [HideInInspector] public bool isStartRoom = false;
    [HideInInspector] public bool isEndRoom = false;

    [HideInInspector] public int roomDepth;

    public Tile GetTileAt(Vector2Int localPosition)
    {
        foreach(Tile tile in tiles)
        {
            if(tile.roomPosition == localPosition)
            {
                return tile;
            }
        }
        return null;
    }
}
