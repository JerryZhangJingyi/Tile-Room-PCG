using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [HideInInspector] public Tile tile;
    [HideInInspector] public int HP;
    [HideInInspector] public int maxHP;
    [HideInInspector] public int mana;
    [HideInInspector] public int maxMana;

    [HideInInspector] public bool hasSpear;

    [HideInInspector] public enum enemyTypes { warrior, archer }
    [HideInInspector] public enemyTypes enemyType;

    [HideInInspector] public enum actionTypes { walk, throwSpear, jump, push, attack }
    [HideInInspector] public actionTypes actionType;
    public void RegenMana()
    {
        if(mana < maxMana)
        {
            mana = maxMana;
        }
    }

    public void SpendMana(int cost)
    {
        mana -= cost;
        if (mana < 0)
        {
            mana = 0;
        }
    }
}
