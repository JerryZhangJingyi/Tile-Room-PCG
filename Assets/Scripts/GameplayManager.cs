using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private TileBoardGenerator generator;

    public Unit player;
    [SerializeField] private Camera camera;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private GameObject goalPrefab;
    private Upgrade upgrade;
    private Goal goal;
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject spear;
    private List<Unit> enemies = new List<Unit>();

    private List<Unit> curRoomUnits = new List<Unit>();

    [HideInInspector] public Room curRoom;

    [SerializeField] private Material highlightedMat;
    [SerializeField] private Material playerMat, archerMat, warriorMat, damagedMat;
    private Tile highlightedTile;
    private Material prevTileMat;

    private ResultManager resultManager;

    // Start is called before the first frame update
    void Start()
    {
        resultManager = GameObject.FindGameObjectWithTag("ResultManager").GetComponent<ResultManager>();

        if (camera == null)
        {
            camera = Camera.main;
        }
        player.HP = 3;
        player.maxHP = 3;
        player.mana = 5;
        player.maxMana = 5;
        player.hasSpear = true;

        player.gameObject.GetComponentInChildren<MeshRenderer>().material = playerMat;

        spear.SetActive(false);
        upgradeCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessPlayerInput();

        if(player.HP <= 0)
        {
            Lose();
        }
    }

    public void MoveToRoom(Room room, Door entryDoor = null)
    {
        curRoom = room;
        //move player to start tile
        Tile startTile;
        if (entryDoor)
        {
            startTile = entryDoor.tile;
        }
        else
        {
            startTile = room.tiles.Find(t => t.IsFloorTile);
        }

        MoveUnitToTile(player, startTile, false);

        // move camera
        FocusCameraOnRoom(room);

        //spawn units
        SpawnEnemy(room);

        //spawn upgrade
        SpawnUpgrade(room);

        //spawn goal
        if(room.roomDepth == generator.victoryDepth - 1)
        {
            SpawnGoal(room);
        }

        //restore mana
        player.RegenMana();
    }

    void FocusCameraOnRoom(Room room)
    {      
        foreach(Tile tile in room.tiles)
        {
            if (tile.tileType == 'c')
            {
                //camera.transform.position = tile.transform.position + Vector3.up * 15f;
                StartCoroutine(PlayMoveEffect(camera.transform, tile.transform.position + Vector3.up * 15f, 0.5f));
                break;
            }

        }
    }

    void ProcessPlayerInput()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            Tile tile = objectHit.parent.GetComponent<Tile>();

            if (tile != null)
            {
                switch(player.actionType)
                {
                    case Unit.actionTypes.walk:
                        if(CanMoveTo(player, tile))
                        {
                            HighlightTile(tile);
                            if (Input.GetMouseButtonDown(1)) // right mouse down
                            {
                                MoveUnitToTile(player, tile);
                                uiManager.ResetActionButtons();
                                HandleEnemyAI();
                            }
                        }
                        break;

                    case Unit.actionTypes.jump:
                        if (CanJumpTo(player, tile))
                        {
                            HighlightTile(tile);
                            if (Input.GetMouseButtonDown(1)) // right mouse down
                            {
                                MoveUnitToTile(player, tile);
                                uiManager.ResetActionButtons();
                                HandleEnemyAI();
                            }
                        }
                        break;

                    case Unit.actionTypes.attack:
                        if(CanAttack(player, tile))
                        {
                            HighlightTile(tile);
                            if (Input.GetMouseButtonDown(1)) // right mouse down
                            {
                                AttackEnemy(tile.unit);
                                uiManager.ResetActionButtons();
                                HandleEnemyAI();
                            }
                        }
                        break;

                    case Unit.actionTypes.throwSpear:
                        if (CanThrow(player, tile))
                        {
                            HighlightTile(tile);
                            if (Input.GetMouseButtonDown(1)) // right mouse down
                            {
                                ThrowSpear(tile);
                                uiManager.ResetActionButtons();
                                HandleEnemyAI();
                            }
                        }
                        break;

                    case Unit.actionTypes.push:
                        if (CanPush(player, tile))
                        {
                            HighlightTile(tile);
                            if (Input.GetMouseButtonDown(1)) // right mouse down
                            {
                                Push(player, tile);
                                uiManager.ResetActionButtons();
                                //HandleEnemyAI();
                            }
                        }
                        break;
                }
                
            }
            // Do something with the object that was hit by the raycast.
        }
    }

    bool CanMoveTo(Unit unit, Tile tile)
    {
        if(upgradeCanvas.enabled == true)
        {
            return false;
        }

        if(tile == null || !(tile.IsFloorTile || tile.IsDoorTile))
        {
            return false;
        }

        if(tile.unit != null)
        {
            return false;
        }

        if(unit.tile != null && !unit.tile.IsAdjacentTo(tile))
        {
            return false;
        }

        return true;
    }

    bool CanJumpTo(Unit unit, Tile tile)
    {
        if (upgradeCanvas.enabled == true)
        {
            return false;
        }

        if (tile == null || !(tile.IsFloorTile || tile.IsDoorTile))
        {
            return false;
        }

        if (tile.unit != null)
        {
            return false;
        }

        if (unit.tile != null && !unit.tile.IsTwoRadius(tile))
        {
            return false;
        }

        if(player.mana <= 2)
        {
            return false;
        }

        return true;
    }

    void MoveUnitToTile(Unit unit, Tile targetTile, bool entering = true)
    {
        Tile fromTile = null;

        if(unit.tile != null)
        {
            fromTile = unit.tile;
            unit.tile.unit = null;
        }
        
        unit.tile = targetTile;
        targetTile.unit = unit;

        //unit.transform.position = targetTile.transform.position;
        StartCoroutine(PlayMoveEffect(unit.transform, targetTile.transform.position, 0.5f));

        if(entering)
        {
            OnUnitEnteredTile(unit, targetTile, fromTile);
        }

        if(player.actionType == Unit.actionTypes.jump)
        {
            player.SpendMana(2);
        }
    }

    void OnUnitEnteredTile(Unit unit, Tile to, Tile from)
    {
        if(to.IsDoorTile)
        {
            DespawnEnemy(from.room);
            DespawnUpgrade(from.room);
            DespawnGoal(from.room);

            Door door = to.GetComponent<Door>();
            door.connectedDoor.gameObject.SetActive(true);
            Room nextRoom = door.connectedDoor.tile.room;
            MoveToRoom(nextRoom, door.connectedDoor);
        }

        if(unit == player && to.hasSpear == true)
        {
            CollectSpear(to);
        }

        if(unit == player && to.upgrade != null)
        {
            ShowUpgrade();
        }

        if (unit == player && to.goal != null)
        {
            CollectGoal();
        }

        if(unit != player && to.IsLavaTile)
        {
            AttackEnemy(unit);
        }
    }

    void HighlightTile(Tile tile)
    {
        if(tile == highlightedTile)
        {
            return;
        }

        if(highlightedTile != null)
        {
            Renderer renderer = highlightedTile.GetComponentInChildren<Renderer>();
            renderer.material = prevTileMat;
        }

        if(tile == null)
        {
            highlightedTile = tile;
            return;
        }

        if(!(tile.IsFloorTile || tile.IsFloorTile))
        {
            return;
        }

        highlightedTile = tile;
        Renderer rnd = highlightedTile.GetComponentInChildren<Renderer>();
        prevTileMat = rnd.material;
        rnd.material = highlightedMat;
    }

    void SpawnUpgrade(Room room)
    {
        bool upgradeSpawned = false;

        while (upgradeSpawned == false)
        {
            int random = Random.Range(0, room.tiles.Count);

            if (room.tiles[random].upgrade == null && room.tiles[random].IsFloorTile)
            {
                GameObject upgradeObject = Instantiate(upgradePrefab, room.tiles[random].transform.position, room.tiles[random].transform.rotation);

                upgrade = upgradeObject.GetComponent<Upgrade>();
                room.tiles[random].upgrade = upgrade;
                upgrade.tile = room.tiles[random];

                upgradeSpawned = true;
            }
        }
    }

    void ShowUpgrade()
    {
        upgradeCanvas.enabled = true;
        uiManager.HideMainCanvas();
    }

    public void CollectUpgrade()
    {
        if(upgrade.selectedUpGrades[upgrade.upgradeDropdown.value] == 0)
        {
            player.HP = player.maxHP;
        }
        else if (upgrade.selectedUpGrades[upgrade.upgradeDropdown.value] == 1)
        {
            player.maxHP++;
        }
        else if (upgrade.selectedUpGrades[upgrade.upgradeDropdown.value] == 2)
        {
            player.maxMana++;
        }
        else if (upgrade.selectedUpGrades[upgrade.upgradeDropdown.value] == 3)
        {
            player.mana = player.maxMana;
        }

        upgrade.tile.upgrade = null;
        Destroy(upgrade.gameObject);
        upgradeCanvas.enabled = false;
        uiManager.ShowMainCanvas();
    }

    void DespawnUpgrade(Room room)
    {
        if(upgrade != null)
        {
            Destroy(upgrade.gameObject);
        }
        foreach (Tile tile in room.tiles)
        {
            tile.upgrade = null;
        }
    }

    void SpawnGoal(Room room)
    {
        bool goalSpawned = false;

        while (goalSpawned == false)
        {
            int random = Random.Range(0, room.tiles.Count);

            if (room.tiles[random].upgrade == null && room.tiles[random].IsFloorTile)
            {
                GameObject goalObject = Instantiate(goalPrefab, room.tiles[random].transform.position, room.tiles[random].transform.rotation);

                goal = goalObject.GetComponent<Goal>();
                room.tiles[random].goal = goal;
                goal.tile = room.tiles[random];

                goalSpawned = true;
            }
        }
    }

    void CollectGoal()
    {
        Win();
    }

    void DespawnGoal(Room room)
    {
        if (goal != null)
        {
            Destroy(goal.gameObject);
        }
        foreach (Tile tile in room.tiles)
        {
            tile.goal = null;
        }
    }

    void SpawnEnemy(Room room)
    {
        int i = 0;
        while(i < room.roomDepth + 1)
        {
            int random = Random.Range(0, room.tiles.Count);
            int randomType = Random.Range(0, 2);
            if (room.tiles[random].unit == null && room.tiles[random].IsFloorTile)
            {
                GameObject enemyObject = Instantiate(enemyPrefab, room.tiles[random].transform.position, room.tiles[random].transform.rotation);

                Unit enemy = enemyObject.GetComponent<Unit>();
                room.tiles[random].unit = enemy;
                enemy.tile = room.tiles[random];

                if(randomType == 0)
                {
                    enemy.enemyType = Unit.enemyTypes.warrior;
                    enemy.gameObject.GetComponentInChildren<MeshRenderer>().material = warriorMat;
                }
                else if(randomType == 1)
                {
                    enemy.enemyType = Unit.enemyTypes.archer;
                    enemy.gameObject.GetComponentInChildren<MeshRenderer>().material = archerMat;
                }

                enemy.HP = 1;
                enemies.Add(enemy);
                i++;
            }
        }
    }

    public void DespawnEnemy(Room room)
    {
        enemies.Clear();
        foreach (Tile tile in room.tiles)
        {
            if(tile.unit != null && tile.unit != player)
            {
                Destroy(tile.unit.gameObject);
                tile.unit = null;
            }
        }
    }

    void HandleEnemyAI()
    {
        foreach (Unit enemy in enemies)
        {
            switch(enemy.enemyType)
            {
                case Unit.enemyTypes.warrior:
                    if(enemy.tile.IsAdjacentTo(player.tile))
                    {
                        player.HP -= 1;
                        StartCoroutine(PlayDamagedEffect());
                    }
                    else
                    {
                        Tile upTile = enemy.tile;
                        Tile downTile = enemy.tile;
                        Tile leftTile = enemy.tile;
                        Tile rightTile = enemy.tile;

                        foreach (Tile tile in curRoom.tiles)
                        {
                            if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x < enemy.tile.roomPosition.x)
                            {
                                leftTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x > enemy.tile.roomPosition.x)
                            {
                                rightTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y > enemy.tile.roomPosition.y)
                            {
                                upTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y < enemy.tile.roomPosition.y)
                            {
                                downTile = tile;
                            }
                        }

                        if (player.tile.roomPosition.x > enemy.tile.roomPosition.x)
                        {
                            if (CanMoveTo(enemy, rightTile))
                            {
                                MoveUnitToTile(enemy, rightTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.x < enemy.tile.roomPosition.x)
                        {
                            if (CanMoveTo(enemy, leftTile))
                            {
                                MoveUnitToTile(enemy, leftTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.y > enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, upTile))
                            {
                                MoveUnitToTile(enemy, upTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.y < enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, downTile))
                            {
                                MoveUnitToTile(enemy, downTile);
                            }
                            
                        }
                    }
                    break;

                case Unit.enemyTypes.archer:
                    if (enemy.tile.IsTwoRadius(player.tile))
                    {
                        player.HP -= 1;
                        StartCoroutine(PlayDamagedEffect());
                    }
                    else if(enemy.tile.IsAdjacentTo(player.tile))
                    {
                        Tile upTile = enemy.tile;
                        Tile downTile = enemy.tile;
                        Tile leftTile = enemy.tile;
                        Tile rightTile = enemy.tile;

                        foreach (Tile tile in curRoom.tiles)
                        {
                            if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x < enemy.tile.roomPosition.x)
                            {
                                leftTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x > enemy.tile.roomPosition.x)
                            {
                                rightTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y > enemy.tile.roomPosition.y)
                            {
                                upTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y < enemy.tile.roomPosition.y)
                            {
                                downTile = tile;
                            }
                        }

                        if (player.tile.roomPosition.x < enemy.tile.roomPosition.x)
                        {
                            if(CanMoveTo(enemy, rightTile))
                            {
                                MoveUnitToTile(enemy, rightTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.x > enemy.tile.roomPosition.x)
                        {
                            if (CanMoveTo(enemy, leftTile))
                            {
                                MoveUnitToTile(enemy, leftTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.y < enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, upTile))
                            {
                                MoveUnitToTile(enemy, upTile);
                            }
                           
                        }
                        else if (player.tile.roomPosition.y > enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, downTile))
                            {
                                MoveUnitToTile(enemy, downTile);
                            }
                            
                        }
                    }
                    else
                    {
                        Tile upTile = enemy.tile;
                        Tile downTile = enemy.tile;
                        Tile leftTile = enemy.tile;
                        Tile rightTile = enemy.tile;

                        foreach (Tile tile in curRoom.tiles)
                        {
                            if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x < enemy.tile.roomPosition.x)
                            {
                                leftTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.x > enemy.tile.roomPosition.x)
                            {
                                rightTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y > enemy.tile.roomPosition.y)
                            {
                                upTile = tile;
                            }
                            else if (tile.IsAdjacentTo(enemy.tile) && tile.roomPosition.y < enemy.tile.roomPosition.y)
                            {
                                downTile = tile;
                            }
                        }

                        if (player.tile.roomPosition.x > enemy.tile.roomPosition.x)
                        {
                            if (CanMoveTo(enemy, rightTile))
                            {
                                MoveUnitToTile(enemy, rightTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.x < enemy.tile.roomPosition.x)
                        {
                            if (CanMoveTo(enemy, leftTile))
                            {
                                MoveUnitToTile(enemy, leftTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.y > enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, upTile))
                            {
                                MoveUnitToTile(enemy, upTile);
                            }
                            
                        }
                        else if (player.tile.roomPosition.y < enemy.tile.roomPosition.y)
                        {
                            if (CanMoveTo(enemy, downTile))
                            {
                                MoveUnitToTile(enemy, downTile);
                            }
                            
                        }
                    }
                    break;
            }  
        }
    }

    bool CanAttack(Unit player, Tile tile)
    {
        if (upgradeCanvas.enabled == true)
        {
            return false;
        }

        if (player.tile.IsAdjacentTo(tile) == false)
        {
            return false;
        }

        if(tile.unit == null)
        {
            return false;
        }

        if(tile.unit.enemyType == Unit.enemyTypes.warrior || tile.unit.enemyType == Unit.enemyTypes.archer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CanThrow(Unit player, Tile tile)
    {
        if (upgradeCanvas.enabled == true)
        {
            return false;
        }

        if (player.hasSpear == false)
        {
            return false;
        }

        if(player.tile.IsTwoRadius(tile) || player.tile.IsThreeUnits(tile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CanPush(Unit player, Tile tile)
    {
        if (upgradeCanvas.enabled == true)
        {
            return false;
        }

        if (player.mana < 3)
        {
            return false;
        }

        if (player.tile.IsAdjacentTo(tile) == false)
        {
            return false;
        }

        if (tile.unit == null)
        {
            return false;
        }

        if (tile.unit.enemyType == Unit.enemyTypes.warrior || tile.unit.enemyType == Unit.enemyTypes.archer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CanBePushedTo(Unit unit, Tile tile)
    {
        if (upgradeCanvas.enabled == true)
        {
            return false;
        }

        if (tile == null || !(tile.IsFloorTile || tile.IsLavaTile))
        {
            return false;
        }

        if (tile.unit != null)
        {
            return false;
        }

        if (unit.tile != null && !unit.tile.IsAdjacentTo(tile))
        {
            return false;
        }

        return true;
    }

    void AttackEnemy(Unit enemy)
    {
        enemy.HP -= 1;
        if(enemy.HP <= 0)
        {
            enemy.tile.unit = null;
            enemies.Remove(enemy);
            Destroy(enemy.gameObject);
        } 
    }

    void ThrowSpear(Tile tile)
    {
        spear.SetActive(true);
        spear.transform.position = tile.transform.position;
        player.hasSpear = false;
        tile.hasSpear = true;
        if(tile.unit)
        {
            AttackEnemy(tile.unit);
        }
    }

    void CollectSpear(Tile tile)
    {
        spear.SetActive(false);
        player.hasSpear = true;
        tile.hasSpear = false;
    }

    void Push(Unit player, Tile tile)
    {
        player.SpendMana(2);

        Tile upTile = tile;
        Tile downTile = tile;
        Tile leftTile = tile;
        Tile rightTile = tile;

        foreach (Tile t in curRoom.tiles)
        {
            if (t.IsAdjacentTo(tile) && t.roomPosition.x < tile.roomPosition.x)
            {
                leftTile = t;
            }
            else if (t.IsAdjacentTo(tile) && t.roomPosition.x > tile.roomPosition.x)
            {
                rightTile = t;
            }
            else if (t.IsAdjacentTo(tile) && t.roomPosition.y > tile.roomPosition.y)
            {
                upTile = t;
            }
            else if (t.IsAdjacentTo(tile) && t.roomPosition.y < tile.roomPosition.y)
            {
                downTile = t;
            }
        }

        if (player.tile.roomPosition.x < tile.roomPosition.x)
        {
            if (CanBePushedTo(tile.unit, rightTile))
            {
                MoveUnitToTile(tile.unit, rightTile);
            }

        }
        else if (player.tile.roomPosition.x > tile.roomPosition.x)
        {
            if (CanBePushedTo(tile.unit, leftTile))
            {
                MoveUnitToTile(tile.unit, leftTile);
            }

        }
        else if (player.tile.roomPosition.y < tile.roomPosition.y)
        {
            if (CanBePushedTo(tile.unit, upTile))
            {
                MoveUnitToTile(tile.unit, upTile);
            }

        }
        else if (player.tile.roomPosition.y > tile.roomPosition.y)
        {
            if (CanBePushedTo(tile.unit, downTile))
            {
                MoveUnitToTile(tile.unit, downTile);
            }

        }
    }

    void Win()
    {
        resultManager.won = true;
        SceneManager.LoadScene(1);
    }
    void Lose()
    {
        resultManager.won = false;
        SceneManager.LoadScene(1);
    }

    IEnumerator PlayDamagedEffect()
    {
        player.gameObject.GetComponentInChildren<MeshRenderer>().material = damagedMat;
        yield return new WaitForSeconds(0.3f);
        player.gameObject.GetComponentInChildren<MeshRenderer>().material = playerMat;
        yield return new WaitForSeconds(0.3f);
        player.gameObject.GetComponentInChildren<MeshRenderer>().material = damagedMat;
        yield return new WaitForSeconds(0.3f);
        player.gameObject.GetComponentInChildren<MeshRenderer>().material = playerMat;
    }


    IEnumerator PlayMoveEffect(Transform target, Vector3 to, float duration)
    {
        float time = 0;
        Vector3 start = target.position;

        while(target && time <= duration)
        {
            //move obj
            float t = Mathf.Clamp01(time / duration);
            target.position = Vector3.Lerp(start, to, t);
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        target.position = to;
    }
}
