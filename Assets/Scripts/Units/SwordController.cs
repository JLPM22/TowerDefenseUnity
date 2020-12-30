using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : UnitController
{
    public int DamagePerHit = 20;

    protected override void Attack(GameObject enemy)
    {
        UnitController unit = enemy.GetComponentInParent<UnitController>();
        if (unit != null)
        {
            unit.AddHealth(-DamagePerHit);
            SoundController.Instance.PlaySound(SoundController.Sound.SwordAttack, 4.0f);
            return;
        }
        TowerController tower = enemy.GetComponentInParent<TowerController>();
        if (tower != null)
        {
            tower.AddHealth(-DamagePerHit);
            SoundController.Instance.PlaySound(SoundController.Sound.SwordAttack, 4.0f);
            return;
        }
    }
}
