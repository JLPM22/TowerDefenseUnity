using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEvents : MonoBehaviour
{
    private UnitController UnitController;

    private void Awake()
    {
        UnitController = GetComponentInParent<UnitController>();
    }

    public void OnAnimationAttackEnded()
    {
        if (!UnitController.Dead)
            UnitController.OnAnimationAttackEnded();
    }
}
