using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamController : MonoBehaviour
{
    public event Action<Unit> OnUnitDead;

    public Vector3 TowerLocationLocal;
    public bool IsTeam1;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI GoldPerSecondText;
    public int GoldPerSecond = 5;
    public TowerController Tower;

    public GameObject SwordPrefab;
    public GameObject SamuraiPrefab;
    public GameObject MagoPrefab;
    public GameObject KingPrefab;
    public GameObject ExplosionPrefab;

    public int Gold { get; private set; }
    public int CurrentGoldPerSecond { get; private set; }
    public int CurrentGoldIncrement { get; private set; }

    [HideInInspector] public List<GameObject>[] Units = new List<GameObject>[4];

    private void Start()
    {
        for (int i = 0; i < Units.Length; ++i) Units[i] = new List<GameObject>();
        Reset();
    }

    public void Reset()
    {
        Gold = 0;
        CurrentGoldPerSecond = GoldPerSecond;
        CurrentGoldIncrement = 0;
        UpdateGoldUI();
        UpdateGoldPerSecondUI();
        StopAllCoroutines();
        StartCoroutine(GoldLoop());
        ResetUnits();
        Tower.Reset();
    }

    private WaitForSeconds WaitFor1Second = new WaitForSeconds(1.0f);
    public IEnumerator GoldLoop()
    {
        while (true)
        {
            yield return WaitFor1Second;
            AddGold(CurrentGoldPerSecond);
        }
    }

    public bool Spawn(Unit unit)
    {
        if (unit != Unit.Explosion &&
            unit != Unit.IncrementGold &&
            Tower.UnitsInTower > 0) return false;

        int unitCost = UnitCost[(int)unit];
        if (unitCost <= Gold)
        {
            GameObject prefab = null;
            switch (unit)
            {
                case Unit.Sword:
                    prefab = SwordPrefab;
                    SoundController.Instance.PlaySound(SoundController.Sound.Spawn, 2.0f);
                    break;
                case Unit.Samurai:
                    prefab = SamuraiPrefab;
                    SoundController.Instance.PlaySound(SoundController.Sound.Spawn, 2.0f);
                    break;
                case Unit.Mago:
                    prefab = MagoPrefab;
                    SoundController.Instance.PlaySound(SoundController.Sound.Spawn, 2.0f);
                    break;
                case Unit.King:
                    prefab = KingPrefab;
                    SoundController.Instance.PlaySound(SoundController.Sound.Spawn, 2.0f);
                    break;
                case Unit.Explosion:
                    prefab = null;
                    StartCoroutine(Explosion());
                    SoundController.Instance.PlaySound(SoundController.Sound.Explosion, 4.0f);
                    break;
                case Unit.IncrementGold:
                    prefab = null;
                    if (CurrentGoldIncrement < 3)
                    {
                        unitCost = UnitCost[(int)unit + CurrentGoldIncrement];
                        if (unitCost > Gold) return false;
                        CurrentGoldIncrement += 1;
                        CurrentGoldPerSecond += 5;
                        UpdateGoldPerSecondUI();
                        SoundController.Instance.PlaySound(SoundController.Sound.ImproveGold);
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }
            if (prefab != null)
            {
                GameObject unitGO = Instantiate(prefab, transform.position + TowerLocationLocal, Quaternion.identity, transform);
                unitGO.GetComponentInChildren<UnitController>().IsTeam1 = IsTeam1;
                Units[(int)unit].Add(unitGO);
            }

            AddGold(-unitCost);
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        UpdateGoldUI();
    }

    public void UnitKilled(GameObject gO, Unit unit)
    {
        bool removed = Units[(int)unit].Remove(gO);
        for (int i = 0; i < Units.Length; ++i)
            for (int j = 0; j < Units[i].Count; ++j)
                if (Units[i][j] == null) Units[i].RemoveAt(j--);
        OnUnitDead?.Invoke(unit);
        Debug.Assert(removed, "This should not happen");
    }

    private IEnumerator Explosion()
    {
        GameObject gO = Instantiate(ExplosionPrefab, transform.position + new Vector3(-2.32f, 0.449844f, 0.0f), Quaternion.identity, transform);
        foreach (UnitController unit in transform.parent.GetComponentsInChildren<UnitController>())
        {
            unit.AddHealth(-100);
        }
        yield return new WaitForSeconds(5.0f);
        Destroy(gO);
    }

    private void ResetUnits()
    {
        foreach (Transform unitTransform in transform)
        {
            Destroy(unitTransform.gameObject);
        }
    }

    private void UpdateGoldUI()
    {
        GoldText.text = Gold.ToString();
    }

    private void UpdateGoldPerSecondUI()
    {
        GoldPerSecondText.text = "+" + CurrentGoldPerSecond.ToString();
    }

    public enum Unit
    {
        Sword,
        Samurai,
        Mago,
        King,
        Explosion,
        IncrementGold
    }
    public static readonly int[] UnitCost = { 50, 100, 150, 200, 200, 100, 250, 500 };

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + TowerLocationLocal, 0.1f);
    }
}
