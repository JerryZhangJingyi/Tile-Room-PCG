using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField] private Text winText;
    [SerializeField] private Text loseText;
    private ResultManager resultManager;
    private void Awake()
    {
        winText.enabled = false;
        loseText.enabled = false;

        resultManager = GameObject.FindGameObjectWithTag("ResultManager").GetComponent<ResultManager>();

    }
    // Start is called before the first frame update
    void Start()
    {
        if(resultManager.won == false)
        {
            loseText.enabled = true;
        }
        else
        {
            winText.enabled = true;
        }
    }
    public void ToGame()
    {
        SceneManager.LoadScene(0);
    }
}
