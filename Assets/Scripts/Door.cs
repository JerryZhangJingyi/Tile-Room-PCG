using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [HideInInspector] public Tile tile;
    [HideInInspector] public Door connectedDoor;
    [HideInInspector] public bool connected = false;
}
