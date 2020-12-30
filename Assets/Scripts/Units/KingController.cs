using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingController : UnitController
{
    public int DamagePerHit = 20;
    public int HealAmount = 50;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Heal());
    }

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

    private IEnumerator Heal()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            AddHealth(HealAmount);
        }
    }
}
