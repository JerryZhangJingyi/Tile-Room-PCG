using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    [HideInInspector] public Tile tile;
    [HideInInspector] public Unit player;

    [HideInInspector] public List<int> upGrades = new List<int>();
    [HideInInspector] public List<int> selectedUpGrades = new List<int>();

    public Dropdown upgradeDropdown;
    // Start is called before the first frame update
    void Start()
    {
        upgradeDropdown = GameObject.FindGameObjectWithTag("UpgradeDropdown").GetComponent<Dropdown>();

        upGrades.Add(0);
        upGrades.Add(1);
        upGrades.Add(2);
        upGrades.Add(3);
        while (selectedUpGrades.Count != 3)
        {
            int random = Random.Range(0, upGrades.Count);
            selectedUpGrades.Add(upGrades[random]);
            upGrades.RemoveAt(random);
        }

        for(int i = 0; i < upgradeDropdown.options.Count; i++)
        {
            if(selectedUpGrades[i] == 0)
            {
                upgradeDropdown.options[i].text = "Restore HP to Full";
            }
            else if (selectedUpGrades[i] == 1)
            {
                upgradeDropdown.options[i].text = "Increase Max HP";
            }
            else if (selectedUpGrades[i] == 2)
            {
                upgradeDropdown.options[i].text = "Increase Max Mana";
            }
            else if (selectedUpGrades[i] == 3)
            {
                upgradeDropdown.options[i].text = "Restore Mana to Full";
            }
        }
    }
}
