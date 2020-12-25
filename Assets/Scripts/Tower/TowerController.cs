using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerController : MonoBehaviour
{
    public event Action OnDestroyed;

    public int InitialHealth = 1000;
    public RawImage HealthBar;

    public int Health { get; private set; }

    private Vector3 InitLocalPosition;

    private void Awake()
    {
        InitLocalPosition = transform.localPosition;
        Reset();
    }

    public void Reset()
    {
        Health = InitialHealth;
        AddHealth(0); // Update state
        transform.localPosition = InitLocalPosition;
    }

    public void AddHealth(int amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, InitialHealth);

        if (Health == 0)
        {
            // Dead
            StartCoroutine(Die());
            if (HealthBar.gameObject.activeSelf) HealthBar.gameObject.SetActive(false);
        }
        else if (Health == InitialHealth)
        {
            // Full life
            if (HealthBar.gameObject.activeSelf) HealthBar.gameObject.SetActive(false);
        }
        else
        {
            // Injured
            if (!HealthBar.gameObject.activeSelf) HealthBar.gameObject.SetActive(true);
            Vector3 localScale = HealthBar.rectTransform.localScale;
            localScale.x = (float)Health / InitialHealth;
            HealthBar.rectTransform.localScale = localScale;
        }
    }

    private IEnumerator Die()
    {
        Vector3 localPosition = transform.localPosition;
        while (localPosition.y > -5.0f)
        {
            localPosition.y -= Time.deltaTime * 2.0f;
            transform.localPosition = localPosition;
            yield return null;
        }
        OnDestroyed?.Invoke();
    }

    public int UnitsInTower { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<UnitController>() != null)
        {
            UnitsInTower += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<UnitController>() != null)
        {
            UnitsInTower -= 1;
        }
    }
}
