using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TeamController))]
public class InputController : MonoBehaviour
{
    public Button SwordButton, SamuraiButton, MagoButton, KingButton, ExplosionButton, GoldButton;
    public TextMeshProUGUI GoldText;

    private TeamController TeamController;
    private int LastGoldIncrement = -1;

    private EventTrigger SwordET, SamuraiET, MagoET, KingET, ExplosionET, GoldET;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();

        SwordET = SwordButton.GetComponent<EventTrigger>();
        SamuraiET = SamuraiButton.GetComponent<EventTrigger>();
        MagoET = MagoButton.GetComponent<EventTrigger>();
        KingET = KingButton.GetComponent<EventTrigger>();
        ExplosionET = ExplosionButton.GetComponent<EventTrigger>();
        GoldET = GoldButton.GetComponent<EventTrigger>();
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TeamController.Spawn(TeamController.Unit.Sword);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TeamController.Spawn(TeamController.Unit.Samurai);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TeamController.Spawn(TeamController.Unit.Mago);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TeamController.Spawn(TeamController.Unit.King);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TeamController.Spawn(TeamController.Unit.Explosion);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TeamController.Spawn(TeamController.Unit.IncrementGold);
        }

        // Buttons
        SwordButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Sword];
        SwordET.enabled = SwordButton.interactable;
        SamuraiButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Samurai];
        SamuraiET.enabled = SwordButton.interactable;
        MagoButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Mago];
        MagoET.enabled = MagoButton.interactable;
        KingButton.interactable = TeamController.Tower.UnitsInTower <= 0 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.King];
        KingET.enabled = KingButton.interactable;
        ExplosionButton.interactable = TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.Explosion];
        ExplosionET.enabled = ExplosionButton.interactable;
        GoldButton.interactable = TeamController.CurrentGoldIncrement < 3 && TeamController.Gold >= TeamController.UnitCost[(int)TeamController.Unit.IncrementGold + TeamController.CurrentGoldIncrement];
        GoldET.enabled = GoldButton.interactable;

        // Text
        if (TeamController.CurrentGoldIncrement != LastGoldIncrement)
        {
            if (TeamController.CurrentGoldIncrement >= 3) GoldText.text = "";
            else GoldText.text = TeamController.UnitCost[(int)TeamController.Unit.IncrementGold + TeamController.CurrentGoldIncrement].ToString();
            LastGoldIncrement = TeamController.CurrentGoldIncrement;
        }
    }
}
