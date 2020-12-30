using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerController : MonoBehaviour
{
    public event Action OnDestroyed;
    public event Action<int> OnDamaged;

    public int InitialHealth = 1000;
    public RawImage HealthBar;

    public int Health { get; private set; }

    private Vector3 InitLocalPosition;
    private SpriteRenderer Image;
    private Color ImageColor;

    private void Awake()
    {
        Image = GetComponentInChildren<SpriteRenderer>();
        ImageColor = Image.color;
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
        int previousHealth = Health;
        Health = Mathf.Clamp(Health + amount, 0, InitialHealth);
        OnDamaged?.Invoke(previousHealth - Health);

        if (Health == 0)
        {
            // Dead
            StartCoroutine(Die());
            if (HealthBar.gameObject.activeSelf) HealthBar.gameObject.SetActive(false);
            // Effect
            StartCoroutine(DamageEffect());
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
            // Effect
            StartCoroutine(DamageEffect());
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

    private WaitForSeconds WaitDamage = new WaitForSeconds(0.25f);
    private IEnumerator DamageEffect()
    {
        Image.color = new Color(1.0f, ImageColor.g / 4.0f, ImageColor.b / 4.0f, ImageColor.a);
        yield return WaitDamage;
        Image.color = ImageColor;
    }
}
