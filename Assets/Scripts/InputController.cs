using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(TeamController))]
public class InputController : MonoBehaviour
{
    public Button SwordButton, SamuraiButton, MagoButton, KingButton, ExplosionButton, GoldButton;
    public TextMeshProUGUI GoldText;

    private TeamController TeamController;
    private int LastGoldIncrement = -1;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();
    }

    private void OnEnable()
    {
        SwordButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.Sword));
        SamuraiButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.Samurai));
        MagoButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.Mago));
        KingButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.King));
        ExplosionButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.Explosion));
        GoldButton.onClick.AddListener(() => TeamController.Spawn(TeamController.Unit.IncrementGold));
    }

    private void OnDisable()
    {
        SwordButton.onClick.RemoveAllListeners();
        SamuraiButton.onClick.RemoveAllListeners();
        MagoButton.onClick.RemoveAllListeners();
        KingButton.onClick.RemoveAllListeners();
        ExplosionButton.onClick.RemoveAllListeners();
        GoldButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Input
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TeamController.AddGold(100);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            TeamController.Spawn(TeamController.Unit.Sword);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            TeamController.Spawn(TeamController.Unit.Samurai);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            TeamController.Spawn(TeamController.Unit.Mago);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            TeamController.Spawn(TeamController.Unit.King);
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            TeamController.Spawn(TeamController.Unit.Explosion);
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            TeamController.Spawn(TeamController.Unit.IncrementGold);
        }

        // Buttons
        SwordButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Sword];
        SamuraiButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Samurai];
        MagoButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Mago];
        KingButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.King];
        ExplosionButton.interactable = TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Explosion];
        GoldButton.interactable = TeamController.CurrentGoldIncrement < 3 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.IncrementGold + TeamController.CurrentGoldIncrement];

        // Text
        if (TeamController.CurrentGoldIncrement != LastGoldIncrement)
        {
            if (TeamController.CurrentGoldIncrement >= 3) GoldText.text = "";
            else GoldText.text = TeamController.UnitCost[(int)TeamController.Unit.IncrementGold + TeamController.CurrentGoldIncrement].ToString();
            LastGoldIncrement = TeamController.CurrentGoldIncrement;
        }
    }
}
