using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamController))]
public class InputController : MonoBehaviour
{
    private TeamController TeamController;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();
    }

    private void Update()
    {
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
    }
}
