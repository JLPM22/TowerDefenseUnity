using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UnitController : MonoBehaviour
{
    public static readonly int Team1Layer = 8;
    public static readonly int Team2Layer = 9;

    public bool IsTeam1;
    public float Speed = 1.0f;
    public float AttackSpeed = 1.0f;
    public float AttackDistance = 0.5f;
    public float StopDistance = 0.5f;
    public float StopDistanceSamurai = 0.5f;
    public float RaycastOffset = 0.1f;
    public int InitialHealth = 100;
    public RawImage HealthBar;
    public TeamController.Unit Unit;

    private TeamController TeamController;
    private Vector3 MovementDir;
    private Animator Animator;

    private int IsWalkingID;
    private int AttackID;
    private int DieID;
    private int EnemyTeamLayer;

    private GameObject Enemy;
    private int Health;

    protected virtual void Awake()
    {
        TeamController = GetComponentInParent<TeamController>();
    }

    private void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        IsWalkingID = Animator.StringToHash("isWalking");
        AttackID = Animator.StringToHash("attack");
        DieID = Animator.StringToHash("die");
        MovementDir = IsTeam1 ? Vector3.right : Vector3.left;
        EnemyTeamLayer = IsTeam1 ? Team2Layer : Team1Layer;
        Animator.SetFloat("attackSpeed", AttackSpeed);

        Health = InitialHealth;
        HealthBar.gameObject.SetActive(false);

        foreach (Transform t in GetComponentsInChildren<Transform>()) t.gameObject.layer = IsTeam1 ? Team1Layer : Team2Layer;

        if (!IsTeam1)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = -1;
            transform.localScale = localScale;
        }
    }

    private void Update()
    {
        if (Health == 0 || Dead) return;

        // Raycast
        // Debug.DrawRay(transform.position + MovementDir * RaycastOffset, MovementDir.normalized * AttackDistance, Color.red);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + MovementDir * RaycastOffset, MovementDir, AttackDistance, 1 << Team1Layer | 1 << Team2Layer);
        if (hits.Length > 0)
        {
            RaycastHit2D hit = new RaycastHit2D();
            for (int i = 0; i < hits.Length; ++i)
            {
                UnitController unitController = hits[i].collider.GetComponentInParent<UnitController>();
                TowerController towerController = hits[i].collider.GetComponentInParent<TowerController>();
                bool isEnemyDead = (unitController != null && unitController.Health == 0) || (towerController != null && towerController.Health == 0);
                if (hits[i].collider != null)
                {
                    hit = hits[i];
                    if (hit.collider.gameObject.layer != EnemyTeamLayer) break;
                    if (!isEnemyDead) break;
                }
            }
            if (hit.collider != null)
            {
                UnitController unitController = hit.collider.GetComponentInParent<UnitController>();
                TowerController towerController = hit.collider.GetComponentInParent<TowerController>();
                bool isEnemyDead = (unitController != null && unitController.Health == 0) || (towerController != null && towerController.Health == 0);

                if (hit.collider.gameObject.layer == EnemyTeamLayer && !isEnemyDead)
                {
                    // Attack
                    AttackAnim();
                    Enemy = hit.collider.gameObject;
                }
                else
                {
                    Enemy = null;
                    float distance = Vector2.Distance(hit.point, (Vector2)(transform.position + MovementDir * RaycastOffset));
                    float stopDistance = unitController is SamuraiController ? StopDistanceSamurai : StopDistance;
                    if (distance <= stopDistance && !isEnemyDead && towerController == null)
                    {
                        // Stop
                        IdleAnim();
                    }
                    else
                    {
                        transform.position += MovementDir * Speed * Time.deltaTime;
                        MoveAnim();
                    }
                }
            }
        }
        else
        {
            // Move
            transform.position += MovementDir * Speed * Time.deltaTime;
            MoveAnim();
        }
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

    public void OnAnimationAttackEnded()
    {
        if (Enemy != null)
        {
            Attack(Enemy);
        }
    }

    protected abstract void Attack(GameObject enemy);

    private void AttackAnim()
    {
        Animator.SetBool(DieID, false);
        Animator.SetBool(AttackID, true);
        Animator.SetBool(IsWalkingID, false);
    }
    private void MoveAnim()
    {
        Animator.SetBool(DieID, false);
        Animator.SetBool(AttackID, false);
        Animator.SetBool(IsWalkingID, true);
    }
    private void IdleAnim()
    {
        Animator.SetBool(DieID, false);
        Animator.SetBool(AttackID, false);
        Animator.SetBool(IsWalkingID, false);
    }
    private void DieAnim()
    {
        Animator.SetBool(DieID, true);
        Animator.SetBool(AttackID, false);
        Animator.SetBool(IsWalkingID, false);
    }

    public bool Dead { get; private set; }
    private IEnumerator Die()
    {
        if (!Dead)
        {
            Dead = true;
            DieAnim();
            yield return new WaitForSeconds(2.0f);
            TeamController?.UnitKilled(gameObject, Unit);
            Destroy(gameObject);
        }
    }
}
