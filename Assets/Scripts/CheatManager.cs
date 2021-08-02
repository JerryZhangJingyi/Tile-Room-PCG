using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField] private GameplayManager gameplay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameplay.player.HP = gameplay.player.maxHP;
            gameplay.player.mana = gameplay.player.maxMana;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameplay.DespawnEnemy(gameplay.curRoom);
        }
    }
}
