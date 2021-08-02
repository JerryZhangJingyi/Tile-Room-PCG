using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TileBoardGenerator : MonoBehaviour
{
    public int victoryDepth;
    [SerializeField] private int seed;
    [SerializeField] private int maxTries;

    [SerializeField] private Material yellow, red, green;
    [SerializeField] private Material lavaMat;
    [SerializeField] private GameplayManager gameplayManager;

    private int tasks = 0;
    private bool doorsRemoved = false;

    public List<TextAsset> roomAssets;
    [HideInInspector] public TileBoard board;

    [System.Serializable]
    public struct TileData
    {
        public char tileType;
        public Tile tilePrefab;
    }

    public List<TileData> tileDatas;

    private void Awake()
    {
        Random.InitState(seed);
        Generate();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Generate();
    }

    // Update is called once per frame
    void Update()
    {
        //clean up when all tasks are finished
        if(tasks == 0 && doorsRemoved == false)
        {
            doorsRemoved = true;
            RemoveUnconnectedDoors();
            ApplyRoomColor();
            gameplayManager.MoveToRoom(board.rooms[0]);
            //RemoveOverlappedWalls();
        }
    }

    public void Generate()
    {
        // create new tileboard
        GameObject tileObject = new GameObject("Tile Board");
        board = tileObject.AddComponent<TileBoard>();

        // create starting room
        int roomIndex = 0;
        string strRoom = roomAssets[roomIndex].text;
        Room startRoom = CreateRoomFromString(strRoom, Vector2Int.zero);
        startRoom.isStartRoom = true;
        board.rooms.Add(startRoom);
        startRoom.roomDepth = 0;

        // create new room for each unconnected door in start room
        StartCoroutine(GenerateConnectedRooms(startRoom, victoryDepth));
    }

    private Room CreateRoomFromString(string strRoom, Vector2Int roomOriginBoardPos)
    {
        // create a new room
        GameObject roomObject = new GameObject($"Room {board.rooms.Count}");
        //roomObject.transform.SetParent(board.transform);

        Room room = roomObject.AddComponent<Room>();

        room.strRoom = strRoom;
        //board.rooms.Add(room);

        // iterate through each character in room string and spawn a tile matching that type
        Vector2Int roomPosition = Vector2Int.zero;

        for (int i = 0; i < strRoom.Length; i++)
        {
            char tileType = strRoom[i];

            if (tileType == '\n')
            {
                roomPosition.x = 0;
                roomPosition.y++;

                continue;
            }

            Tile tile = SpawnTile(tileType, room, roomPosition);

            roomPosition.x++;
        }

        return room;
    }

    private float GetTileRotation(char tileType)
    {
        float rot = 0;
        switch(tileType)
        {
            case 'N':
                rot = 90;
                break;
            case 'S':
                rot = -90;
                break;
            case 'E':
                rot = 0;
                break;
            case 'W':
                rot = 180;
                break;

        }
        return rot;
    }

    private Tile SpawnTile(char tileType, Room room, Vector2Int roomPosition)
    {
        TileData tileData = tileDatas.Find(td => td.tileType == tileType);

        if (tileData.tilePrefab == null)
        {
            return null;
        }

        Tile tile = Instantiate(tileData.tilePrefab, room.transform);
        tile.roomPosition = roomPosition;
        tile.transform.localPosition = new Vector3(roomPosition.x, 0, roomPosition.y);
        float yRot = GetTileRotation(tileType);
        tile.transform.rotation = Quaternion.Euler(0, yRot, 0);

        tile.tileType = tileType;
        tile.room = room;
        room.tiles.Add(tile);

        Door door = tile.GetComponent<Door>();
        if (door != null)
        {
            room.doors.Add(door);
            door.tile = tile;
        }

        return tile;
    }

    IEnumerator GenerateConnectedRooms(Room room, int depth)
    {
        tasks++;

        for (int i = 0; i < room.doors.Count; i++)
        {
            if (room.doors[i].connected == false)
            {
                int tries = 2;
                for (int x = 0; x < tries; x++)
                {
                    string strRoom = roomAssets[Random.Range(0, roomAssets.Count)].text;
                    Room generatedRoom = CreateRoomFromString(strRoom, Vector2Int.zero);

                    int random = Random.Range(0, 4);

                    //reset room
                    generatedRoom.transform.position = Vector3.zero;
                    generatedRoom.transform.rotation = Quaternion.identity;

                    //rotate room
                    Vector3 targetEuler = room.doors[i].transform.eulerAngles;
                    Vector3 doorEuler = generatedRoom.doors[random].transform.eulerAngles;
                    float deltaAngle = Mathf.DeltaAngle(doorEuler.y, targetEuler.y);
                    Quaternion currentRoomRotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);

                    generatedRoom.transform.rotation = currentRoomRotation * Quaternion.Euler(0, 180, 0);

                    //position room
                    Vector3 roomOffset = generatedRoom.doors[random].transform.position - generatedRoom.transform.position;
                    generatedRoom.transform.position = room.doors[i].transform.position - roomOffset;

                    //check overlap
                    bool overlapped = false;
                    int overlappedParts = 0;

                    yield return new WaitForSeconds(0.00f);
                    
                    for (int j = 0; j < board.rooms.Count; j++)
                    {
                        
                        if (!board.rooms[j].gameObject.Equals(generatedRoom.gameObject) && !board.rooms[j].gameObject.Equals(room.gameObject))
                        {
                            
                            foreach (Collider col in board.rooms[j].GetComponentsInChildren<Collider>())
                            {
                                
                                foreach (Collider col2 in generatedRoom.GetComponentsInChildren<Collider>())
                                {
                                    if (col.bounds.Intersects(col2.bounds))
                                    {
                                        overlappedParts++;
                                        break;
                                    }
                                }

                                if(overlappedParts > 5)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if(overlappedParts > 5)
                    {
                        overlapped = true;
                    }

                    //generate successful
                    if (overlapped == false)
                    {

                        room.doors[i].connected = true;
                        generatedRoom.doors[random].connected = true;

                        room.doors[i].connectedDoor = generatedRoom.doors[random];
                        generatedRoom.doors[random].connectedDoor = room.doors[i];
                        generatedRoom.doors[random].gameObject.SetActive(false);

                        board.rooms.Add(generatedRoom);
                        generatedRoom.roomDepth = victoryDepth - depth;
                        // recursive 
                        if (depth == 1)
                        {
                            generatedRoom.isEndRoom = true;
                            tasks--;
                            yield break;
                        }
                        else
                        {
                            //allow up to 10 tasks to run at the same time
                            if(tasks < 10)
                            {
                                StartCoroutine(GenerateConnectedRooms(generatedRoom, depth - 1));
                            }
                            
                        }
                        break;
                    }
                    else //generate failed
                    {
                        Destroy(generatedRoom.transform.gameObject);
                    }

                }
            }
        }
        tasks--;
    }

    private void RemoveUnconnectedDoors()
    {

        foreach(Room room in board.rooms)
        {
            foreach(Door door in room.doors)
            {
                if(door.connected == false)
                {
                    //Debug.Log(door.gameObject.GetComponent<Tile>().roomPosition);
                    SpawnTile('-', room, door.gameObject.GetComponent<Tile>().roomPosition);
                    Destroy(door.gameObject);
                }
            }
        }
    }

    private void ApplyRoomColor()
    {
        foreach(Room room in board.rooms)
        {
            if(room.isStartRoom == false && room.isEndRoom == false)
            {
                foreach(MeshRenderer renderer in room.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.material = green;
                }
            }
            else if(room.isStartRoom == true)
            {
                foreach (MeshRenderer renderer in room.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.material = yellow;
                }
            }
            else if(room.isEndRoom == true)
            {
                foreach (MeshRenderer renderer in room.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.material = red;
                }
            }

            foreach (Tile tile in room.tiles)
            {
                if(tile.tileType == 't')
                {
                    foreach (MeshRenderer renderer in tile.gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        renderer.material = lavaMat;
                    }
                }
            }
        }
    }

    private void RemoveOverlappedWalls()
    {
        for(int i = 0; i < board.rooms.Count; i++)
        {
            for(int j = i + 1; j < board.rooms.Count; j++)
            {
                foreach (Collider col in board.rooms[i].GetComponentsInChildren<Collider>())
                {

                    foreach (Collider col2 in board.rooms[j].GetComponentsInChildren<Collider>())
                    {
                        if (col.bounds.Intersects(col2.bounds))
                        {
                            Destroy(col2.transform.gameObject);
                        }
                    }
                }
            }
        }
    }

}
