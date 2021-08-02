using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameplayManager gameplay;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Unit player;
    [SerializeField] private Text HPText;
    [SerializeField] private Text ManaText;
    [SerializeField] private Text depthText;

    [SerializeField] private GameObject throwButton;
    [SerializeField] private GameObject JumpButton;
    [SerializeField] private GameObject PushButton;
    [SerializeField] private GameObject AttackButton;
    [SerializeField] private GameObject cancelButton;

    private void Start()
    {
        throwButton.SetActive(true);
        JumpButton.SetActive(true);
        PushButton.SetActive(true);
        AttackButton.SetActive(true);
        cancelButton.SetActive(false);
    }

    private void Update()
    {
        HPText.text = "HP " + player.HP + "/" + player.maxHP;
        ManaText.text = "Mana " + player.mana + "/" + player.maxMana;
        if(gameplay.curRoom)
        {
            depthText.text = "Depth " + (gameplay.curRoom.roomDepth + 1).ToString();
        }   
    }

    public void ThrowHit()
    {
        player.actionType = Unit.actionTypes.throwSpear;
        HideActionButtons();
    }
    public void JumpHit()
    {
        player.actionType = Unit.actionTypes.jump;
        HideActionButtons();
    }
    public void PushHit()
    {
        player.actionType = Unit.actionTypes.push;
        HideActionButtons();
    }
    public void AttackHit()
    {
        player.actionType = Unit.actionTypes.attack;
        HideActionButtons();
    }

    void HideActionButtons()
    {
        throwButton.SetActive(false);
        JumpButton.SetActive(false);
        PushButton.SetActive(false);
        AttackButton.SetActive(false);
        cancelButton.SetActive(true);
    }

    public void ResetActionButtons()
    {
        throwButton.SetActive(true);
        JumpButton.SetActive(true);
        PushButton.SetActive(true);
        AttackButton.SetActive(true);
        cancelButton.SetActive(false);

        player.actionType = Unit.actionTypes.walk;
    }

    public void HideMainCanvas()
    {
        canvas.enabled = false;
    }

    public void ShowMainCanvas()
    {
        canvas.enabled = true;
    }
}
