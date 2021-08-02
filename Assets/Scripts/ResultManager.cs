using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    [HideInInspector] public bool won = false;
    private void Awake()
    {
        int num = FindObjectsOfType<GameplayManager>().Length;

        if (num != 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
