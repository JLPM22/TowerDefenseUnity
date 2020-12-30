using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hardcoded agent to train the neural network
/// </summary>
public class TrainingAgent : MonoBehaviour
{
    public bool StaticSwords;
    public bool StaticSwordsPlus;
    public bool FullTraining;
    public TeamController EnemyTeamController;
    public int UpdateSteps = 10;

    private int CurrentSteps;

    private TeamController TeamController;

    public int Action { get; private set; }
    private int TrainingType;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();
        Reset();
    }

    public void Reset()
    {
        if (!FullTraining)
        {
            TrainingType = Random.Range(0, 2);
        }
    }

    private void FixedUpdate()
    {
        if (CurrentSteps == 0) UpdateLogic();
        CurrentSteps = (CurrentSteps + 1) % UpdateSteps;
    }

    private bool SpawnSecondSamurai = false;

    private void UpdateLogic()
    {
        // Input
        const float maxGold = 250.0f;
        const float maxGoldIncrement = 20.0f;
        float gold = Mathf.Clamp01((float)TeamController.Gold / maxGold);                  // Gold
        float enemyGold = Mathf.Clamp01((float)EnemyTeamController.Gold / maxGold);             // Enemy Gold
        float goldPerSecond = (float)TeamController.CurrentGoldPerSecond / maxGoldIncrement;        // Gold per Second
        float enemyGoldPerSecond = (float)EnemyTeamController.CurrentGoldPerSecond / maxGoldIncrement;   // Enemy Gold per Second

        const float maxSwords = 3.0f;
        float swords = Mathf.Clamp01((float)TeamController.Units[0].Count / maxSwords);      // Number Swords out of maxSwords
        float enemySwords = Mathf.Clamp01((float)EnemyTeamController.Units[0].Count / maxSwords); // Number Enemy Swords out ofmaxSwords

        float samurais = (float)TeamController.Units[1].Count > 0 ? 1.0f : 0.0f;               // There are Samurais
        float magos = (float)TeamController.Units[2].Count > 0 ? 1.0f : 0.0f;               // There are Magos
        float kings = (float)TeamController.Units[3].Count > 0 ? 1.0f : 0.0f;               // There are Kings
        float enemySamurais = (float)EnemyTeamController.Units[1].Count > 0 ? 1.0f : 0.0f;          // There are Enemy Samurais
        float enemyMagos = (float)EnemyTeamController.Units[2].Count > 0 ? 1.0f : 0.0f;          // There are Enemy Magos
        float enemyKings = (float)EnemyTeamController.Units[3].Count > 0 ? 1.0f : 0.0f;          // There are Enemy Kings

        float closerDistAlly = GetCloserUnit(TeamController, EnemyTeamController);
        float closerDistEnemy = GetCloserUnit(EnemyTeamController, TeamController);

        if (StaticSwordsPlus)
        {
            if (TeamController.CurrentGoldPerSecond == 5.0f)
            {
                Action = 6;
            }
            else
            {
                Action = 1;
            }
            return;
        }

        if (StaticSwords)
        {
            Action = 1;
            return;
        }

        if (!FullTraining)
        {
            Action = 0;
            // Random
            if (Random.Range(0.0f, 1.0f) < 0.05f)
            {
                Action = 1;
            }
            // Decision
            else if (closerDistEnemy >= 0.8f && swords == 0.0f)
            {
                Action = 1; // Enemy too close
                SpawnSecondSamurai = false;
            }
            else if (closerDistEnemy >= 0.55f && swords <= 0.34f && enemySamurais == 1.0f)
            {
                Action = 1; // Samurai too close
                SpawnSecondSamurai = false;
            }
            else if (SpawnSecondSamurai)
            {
                Action = 2;
                SpawnSecondSamurai = false;
            }
            else if (enemySamurais == 0.0f && enemySwords == 0.0f && enemyMagos == 0.0f && enemyKings == 0.0f && (enemyGold <= 0.2f || goldPerSecond > 0.5f)) Action = 2; // Empty map
            else if (goldPerSecond <= 0.25f) // Try to improve gold
            {
                if (closerDistEnemy < 0.5f) Action = 6;
                else if (swords == 0.0f) Action = 1;
            }
            else
            {
                if (gold >= 1.0f && goldPerSecond <= 0.5f) Action = 6; // Try to improve gold
                else if (enemyMagos == 1.0f && enemySamurais == 1.0f && magos == 0.0f && samurais == 0.0f) Action = 5; // at least one enemy mago and samurai
                else if (enemyMagos == 1.0f && closerDistEnemy >= 0.5f && gold >= 0.75f)
                {
                    Action = 2; // Kill mago with 2 samurais
                    SpawnSecondSamurai = true;
                }
                else if (enemyMagos == 1.0f && closerDistEnemy >= 0.8f) Action = 2; // Mago too close
                else if (enemySwords >= 1.0f && magos == 0.0f) Action = 3; // Mago kill swords
                else if (magos == 1.0f && closerDistAlly > 0.5f) Action = 2; // Help Mago
                else if (gold >= 0.8f && kings == 0.0f) Action = 4;
                else if (kings == 1.0f) Action = 2;
            }
            // Action = 0;
            // if (TrainingType == 0)
            //     Action = 1;
            // else if (TrainingType == 1)
            // {
            //     if (SpawnSecondSamurai)
            //     {
            //         Action = 2;
            //         SpawnSecondSamurai = false;
            //     }
            //     else if (enemyMagos == 1.0f && closerDistEnemy >= 0.5f && gold >= 0.75f && samurais == 0.0f)
            //     {
            //         Action = 2; // Kill mago with 2 samurais
            //         SpawnSecondSamurai = true;
            //     }
            //     else if (enemyMagos == 1.0f && closerDistEnemy >= 0.8f) Action = 2; // Mago too close
            //     else if (closerDistEnemy >= 0.8f && swords == 0.0f)
            //     {
            //         Action = 1; // Enemy too close
            //         SpawnSecondSamurai = false;
            //     }
            //     else if (goldPerSecond <= 0.25f) // Try to improve gold
            //     {
            //         if (closerDistEnemy < 0.5f) Action = 6;
            //         else if (swords == 0.0f) Action = 1;
            //     }
            //     else if (gold >= 1.0f && goldPerSecond <= 0.5f) Action = 6; // Try to improve gold
            //     else if (enemySamurais == 0.0f && enemySwords == 0.0f && enemyMagos == 0.0f && enemyKings == 0.0f && (enemyGold <= 0.2f || goldPerSecond > 0.5f)) Action = 2; // Empty map
            // }
        }
        else
        {
            Action = 0;
            // Decision
            if (closerDistEnemy >= 0.8f && swords == 0.0f)
            {
                Action = 1; // Enemy too close
                SpawnSecondSamurai = false;
            }
            else if (closerDistEnemy >= 0.55f && swords <= 0.34f && enemySamurais == 1.0f)
            {
                Action = 1; // Samurai too close
                SpawnSecondSamurai = false;
            }
            else if (SpawnSecondSamurai)
            {
                Action = 2;
                SpawnSecondSamurai = false;
            }
            else if (enemySamurais == 0.0f && enemySwords == 0.0f && enemyMagos == 0.0f && enemyKings == 0.0f && (enemyGold <= 0.2f || goldPerSecond > 0.5f)) Action = 2; // Empty map
            else if (goldPerSecond <= 0.25f) // Try to improve gold
            {
                if (closerDistEnemy < 0.5f) Action = 6;
                else if (swords == 0.0f) Action = 1;
            }
            else
            {
                if (gold >= 1.0f && goldPerSecond <= 0.5f) Action = 6; // Try to improve gold
                else if (enemyMagos == 1.0f && enemySamurais == 1.0f && magos == 0.0f && samurais == 0.0f) Action = 5; // at least one enemy mago and samurai
                else if (enemyMagos == 1.0f && closerDistEnemy >= 0.5f && gold >= 0.75f)
                {
                    Action = 2; // Kill mago with 2 samurais
                    SpawnSecondSamurai = true;
                }
                else if (enemyMagos == 1.0f && closerDistEnemy >= 0.8f) Action = 2; // Mago too close
                else if (enemySwords >= 1.0f && magos == 0.0f) Action = 3; // Mago kill swords
                else if (magos == 1.0f && closerDistAlly > 0.5f) Action = 2; // Help Mago
                else if (gold >= 0.8f && kings == 0.0f) Action = 4;
                else if (kings == 1.0f) Action = 2;
            }
        }
    }

    /// <summary>
    /// Returns 0 if the closer unit is at source and 1 if its at dest, (0,1) in between
    /// </summary>
    private float GetCloserUnit(TeamController source, TeamController dest)
    {
        float closerDist = 0.0f;
        if (source != null && dest != null)
        {
            Vector2 initPoint = source.transform.position + source.TowerLocationLocal;
            Vector2 endPoint = dest.transform.position + dest.TowerLocationLocal;
            float maxDistance = Vector2.Distance(initPoint, endPoint);
            for (int i = 0; i < source.Units.Length; ++i)
            {
                for (int j = 0; j < source.Units[i].Count; ++j)
                {
                    GameObject unit = source.Units[i][j];
                    if (unit != null)
                    {
                        float dist = Vector2.Distance(unit.transform.position, initPoint) / maxDistance;
                        if (dist > closerDist)
                        {
                            closerDist = dist;
                        }
                    }
                }
            }
        }
        return closerDist;
    }
}
