using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagoController : UnitController
{
    public int DamagePerHit = 25;

    protected override void Attack(GameObject enemy)
    {
        UnitController unit = enemy.GetComponentInParent<UnitController>();
        if (unit != null)
        {
            unit.AddHealth(-DamagePerHit);
            return;
        }
        TowerController tower = enemy.GetComponentInParent<TowerController>();
        if (tower != null)
        {
            tower.AddHealth(-DamagePerHit);
            return;
        }
    }
}
