using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Barracuda;

public class PlayerAgent : Agent
{
    public bool Discrete;
    public bool ReadFromTraining;
    public TeamController EnemyTeamController;

    private TeamController TeamController;
    private TrainingAgent TrainingAgent;
    private float RewardEpisode;
    private float TimeBegin;
    private int NumberUnitsSpawned;
    private int NumberEnemyUnitsKilled;
    private int AllyTowerDamage;
    private int EnemyTowerDamage;
    private int NumberSwords;

    private bool EnemyTowerDestroyed;
    private bool AllyTowerDestroyed;
    private bool TieMatch;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();
        TrainingAgent = GetComponent<TrainingAgent>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnemyTeamController.OnUnitDead += OnEnemyUnitKilled;
        TeamController.OnUnitDead += OnAllyUnitKilled;
        EnemyTeamController.Tower.OnDamaged += OnEnemyTowerDamaged;
        TeamController.Tower.OnDamaged += OnAllyTowerDamaged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EnemyTeamController.OnUnitDead -= OnEnemyUnitKilled;
        TeamController.OnUnitDead -= OnAllyUnitKilled;
        EnemyTeamController.Tower.OnDamaged -= OnEnemyTowerDamaged;
        TeamController.Tower.OnDamaged -= OnAllyTowerDamaged;
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log(name + " - RewardEpisode: " + RewardEpisode);
        RewardEpisode = 0.0f;
        NumberEnemyUnitsKilled = 0;
        NumberUnitsSpawned = 0;
        AllyTowerDamage = 0;
        EnemyTowerDamage = 0;
        NumberSwords = 0;
        EnemyTowerDestroyed = false;
        AllyTowerDestroyed = false;
        TieMatch = false;
        TimeBegin = Time.time;
        for (int i = 0; i < TeamController.Units.Length; ++i) TeamController.Units[i].Clear();
        for (int i = 0; i < EnemyTeamController.Units.Length; ++i) EnemyTeamController.Units[i].Clear();
        if (ReadFromTraining)
        {
            TrainingAgent.Reset();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)TeamController.Tower.Health / (float)TeamController.Tower.InitialHealth); // Tower Health

        const float maxGold = 250.0f;
        const float maxGoldIncrement = 20.0f;
        sensor.AddObservation(Mathf.Clamp01((float)TeamController.Gold / maxGold));                  // Gold
        sensor.AddObservation(Mathf.Clamp01((float)EnemyTeamController.Gold / maxGold));             // Enemy Gold
        sensor.AddObservation((float)TeamController.CurrentGoldPerSecond / maxGoldIncrement);        // Gold per Second
        sensor.AddObservation((float)EnemyTeamController.CurrentGoldPerSecond / maxGoldIncrement);   // Enemy Gold per Second

        const float maxSwords = 3.0f;
        sensor.AddObservation(Mathf.Clamp01((float)TeamController.Units[0].Count / maxSwords));      // Number Swords out of maxSwords
        sensor.AddObservation(Mathf.Clamp01((float)EnemyTeamController.Units[0].Count / maxSwords)); // Number Enemy Swords out of maxSwords

        sensor.AddObservation((float)TeamController.Units[1].Count > 0 ? 1.0f : 0.0f);               // There are Samurais
        sensor.AddObservation((float)TeamController.Units[2].Count > 0 ? 1.0f : 0.0f);               // There are Magos
        sensor.AddObservation((float)TeamController.Units[3].Count > 0 ? 1.0f : 0.0f);               // There are Kings
        sensor.AddObservation((float)EnemyTeamController.Units[1].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Samurais
        sensor.AddObservation((float)EnemyTeamController.Units[2].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Magos
        sensor.AddObservation((float)EnemyTeamController.Units[3].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Kings

        float closerDistAlly = GetCloserUnit(TeamController, EnemyTeamController);
        sensor.AddObservation(closerDistAlly);  // Closer Ally Unit (0 at our tower, 1 at enemy's tower)
        float closerDistEnemy = GetCloserUnit(EnemyTeamController, TeamController);
        sensor.AddObservation(closerDistEnemy); // Closer Enemy Unit (0 at enemy's tower, 1 at our tower)
    }

    // Small Test
    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // sensor.AddObservation((float)TeamController.Tower.Health / (float)TeamController.Tower.InitialHealth); // Tower Health

    //     const float maxGold = 250.0f;
    //     const float maxGoldIncrement = 20.0f;
    //     sensor.AddObservation(Mathf.Clamp01((float)TeamController.Gold / maxGold));                  // Gold
    //     // sensor.AddObservation(Mathf.Clamp01((float)EnemyTeamController.Gold / maxGold));             // Enemy Gold
    //     // sensor.AddObservation((float)TeamController.CurrentGoldPerSecond / maxGoldIncrement);        // Gold per Second
    //     // sensor.AddObservation((float)EnemyTeamController.CurrentGoldPerSecond / maxGoldIncrement);   // Enemy Gold per Second

    //     // const float maxSwords = 3.0f;
    //     const float maxSwords = 1.0f;
    //     // sensor.AddObservation(Mathf.Clamp01((float)TeamController.Units[0].Count / maxSwords));      // Number Swords out of maxSwords
    //     sensor.AddObservation(Mathf.Clamp01((float)EnemyTeamController.Units[0].Count / maxSwords)); // Number Enemy Swords out ofmaxSwords

    //     // sensor.AddObservation((float)TeamController.Units[1].Count > 0 ? 1.0f : 0.0f);               // There are Samurais
    //     // sensor.AddObservation((float)TeamController.Units[2].Count > 0 ? 1.0f : 0.0f);               // There are Magos
    //     // sensor.AddObservation((float)TeamController.Units[3].Count > 0 ? 1.0f : 0.0f);               // There are Kings
    //     sensor.AddObservation((float)EnemyTeamController.Units[1].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Samurais
    //     sensor.AddObservation((float)EnemyTeamController.Units[2].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Magos
    //     sensor.AddObservation((float)EnemyTeamController.Units[3].Count > 0 ? 1.0f : 0.0f);          // There are Enemy Kings

    //     // float closerDistAlly = GetCloserUnit(TeamController, EnemyTeamController);
    //     // sensor.AddObservation(closerDistAlly);  // Closer Ally Unit (0 at our tower, 1 at enemy's tower)
    //     float closerDistEnemy = GetCloserUnit(EnemyTeamController, TeamController);
    //     sensor.AddObservation(closerDistEnemy); // Closer Enemy Unit (0 at enemy's tower, 1 at our tower)
    // }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (EnemyTowerDestroyed)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else if (TieMatch)
        {
            SetReward(0.0f);
            EndEpisode();
        }
        else if (AllyTowerDestroyed)
        {
            SetReward(-1.0f);
            EndEpisode();
        }

        int action;
        if (Discrete)
        {
            action = actions.DiscreteActions[0];
        }
        else
        {
            action = 0;
            float probability = actions.ContinuousActions[0];
            for (int i = 1; i < 7; ++i)
            {
                if (actions.ContinuousActions[i] > probability)
                {
                    action = i;
                    probability = actions.ContinuousActions[i];
                }
            }
        }

        // Action Rewards
        const float maxSwords = 3.0f;
        float nEnemySwords = Mathf.Clamp01((float)EnemyTeamController.Units[0].Count / maxSwords);
        float nSwords = Mathf.Clamp01((float)TeamController.Units[0].Count / maxSwords);
        float enemySamurais = (float)EnemyTeamController.Units[1].Count > 0 ? 1.0f : 0.0f;
        float enemyMagos = (float)EnemyTeamController.Units[2].Count > 0 ? 1.0f : 0.0f;
        const float maxGold = 250.0f;
        float gold = Mathf.Clamp01((float)TeamController.Gold / maxGold);
        const float maxGoldIncrement = 20.0f;
        float incrementGold = (float)TeamController.CurrentGoldPerSecond / maxGoldIncrement;
        float closerDistEnemy = GetCloserUnit(EnemyTeamController, TeamController);
        if (action == 3 && nEnemySwords == 1.0f) AddReward(0.1f);
        if (action == 2 && enemyMagos == 1.0f) AddReward(0.1f);
        if (action == 1 && nSwords < 0.5f && enemySamurais == 1.0f) AddReward(0.1f);
        if (action == 6 && incrementGold == 0.25f && gold >= 0.4f) AddReward(0.1f);
        else if (action == 6 && gold >= 1.0f) AddReward(0.1f);
        if (action == 4 && gold >= 0.8f) AddReward(0.1f);
        if (action == 1 && closerDistEnemy >= 0.8f && nSwords == 0.0f) AddReward(0.1f);

        bool spawned;
        switch (action)
        {
            case 0:
                // Nothing
                // Debug.Log("Nothing");
                break;
            case 1:
                // Sword
                // Debug.Log("Sword");
                spawned = TeamController.Spawn(TeamController.Unit.Sword);
                if (spawned)
                {
                    NumberUnitsSpawned += (NumberSwords >= 3 ? 2 : 1);
                    if (NumberSwords >= 3)
                    {
                        AddReward(-0.1f);
                    }
                    NumberSwords += 1;
                }
                break;
            case 2:
                // Samurai
                // Debug.Log("Samurai");
                spawned = TeamController.Spawn(TeamController.Unit.Samurai);
                if (spawned) NumberUnitsSpawned += 1;
                break;
            case 3:
                // Mago
                // Debug.Log("Mago");
                spawned = TeamController.Spawn(TeamController.Unit.Mago);
                if (spawned) NumberUnitsSpawned += 1;
                break;
            case 4:
                // King
                // Debug.Log("King");
                spawned = TeamController.Spawn(TeamController.Unit.King);
                if (spawned) NumberUnitsSpawned += 1;
                break;
            case 5:
                // Explosion
                // Debug.Log("Explosion");
                int sum = 0;
                for (int i = 1; i < 3; ++i) sum += TeamController.Units[i].Count;
                spawned = false;
                if (sum == 0)
                {
                    spawned = TeamController.Spawn(TeamController.Unit.Explosion);
                }
                if (spawned) NumberUnitsSpawned += 2;
                break;
            case 6:
                // Increment Gold
                // Debug.Log("Increment Gold");
                spawned = TeamController.Spawn(TeamController.Unit.IncrementGold);
                if (spawned) NumberEnemyUnitsKilled += 4;
                break;
        }
    }

    bool alpha1Down, alpha2Down, alpha3Down, alpha4Down, alpha5Down, alpha6Down;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) alpha1Down = true;
        if (Input.GetKeyDown(KeyCode.Alpha2)) alpha2Down = true;
        if (Input.GetKeyDown(KeyCode.Alpha3)) alpha3Down = true;
        if (Input.GetKeyDown(KeyCode.Alpha4)) alpha4Down = true;
        if (Input.GetKeyDown(KeyCode.Alpha5)) alpha5Down = true;
        if (Input.GetKeyDown(KeyCode.Alpha6)) alpha6Down = true;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (Discrete)
        {
            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            if (ReadFromTraining)
            {
                discreteActions[0] = TrainingAgent.Action;
            }
            else
            {
                discreteActions[0] = 0;
                if (alpha1Down)
                {
                    discreteActions[0] = 1;
                }
                if (alpha2Down)
                {
                    discreteActions[0] = 2;
                }
                if (alpha3Down)
                {
                    discreteActions[0] = 3;
                }
                if (alpha4Down)
                {
                    discreteActions[0] = 4;
                }
                if (alpha5Down)
                {
                    discreteActions[0] = 5;
                }
                if (alpha6Down)
                {
                    discreteActions[0] = 6;
                }
            }
        }
        else
        {
            ActionSegment<float> continousActions = actionsOut.ContinuousActions;

            if (ReadFromTraining)
            {
                for (int i = 0; i < 7; ++i)
                {
                    if (i == TrainingAgent.Action) continousActions[i] = 1.0f;
                    else continousActions[i] = 0.0f;
                }
            }
            else
            {
                for (int i = 0; i < 7; ++i)
                {
                    continousActions[i] = 0.0f;
                }
                if (alpha1Down)
                {
                    continousActions[1] = 1.0f;
                }
                if (alpha2Down)
                {
                    continousActions[2] = 1.0f;
                }
                if (alpha3Down)
                {
                    continousActions[3] = 1.0f;
                }
                if (alpha4Down)
                {
                    continousActions[4] = 1.0f;
                }
                if (alpha5Down)
                {
                    continousActions[5] = 1.0f;
                }
                if (alpha6Down)
                {
                    continousActions[6] = 1.0f;
                }
            }
        }
        alpha1Down = alpha2Down = alpha3Down = alpha4Down = alpha5Down = alpha6Down = false;
    }

    public void OnAllyTowerDamaged(int amount)
    {
        AllyTowerDamage += amount;
    }

    public void OnEnemyTowerDamaged(int amount)
    {
        EnemyTowerDamage += amount;
    }

    public void OnAllyUnitKilled(TeamController.Unit unit)
    {
        if (unit == TeamController.Unit.Sword) NumberSwords -= 1;

        // float reward = -0.1f;
        // switch (unit)
        // {
        //     case TeamController.Unit.Sword:
        //         reward = -0.075f;
        //         break;
        //     case TeamController.Unit.Samurai:
        //         reward = -0.15f;
        //         break;
        //     case TeamController.Unit.Mago:
        //         reward = -0.225f;
        //         break;
        //     case TeamController.Unit.King:
        //         reward = -0.3f;
        //         break;
        // }
        // AddReward(reward);
        // RewardEpisode += reward;
    }

    public void OnEnemyUnitKilled(TeamController.Unit unit)
    {
        NumberEnemyUnitsKilled += 1;
        // float reward = 0.05f;
        // switch (unit)
        // {
        //     case TeamController.Unit.Sword:
        //         reward = 0.05f;
        //         break;
        //     case TeamController.Unit.Samurai:
        //         reward = 0.1f;
        //         break;
        //     case TeamController.Unit.Mago:
        //         reward = 0.15f;
        //         break;
        //     case TeamController.Unit.King:
        //         reward = 0.2f;
        //         break;
        // }
        // AddReward(reward);
        // RewardEpisode += reward;
    }

    public void RewardTowerDestroyed()
    {
        // if (NumberUnitsSpawned <= 0) NumberUnitsSpawned = 1000;
        // if (NumberEnemyUnitsKilled <= 0) NumberEnemyUnitsKilled = 1;

        // NumberUnitsSpawned += Mathf.FloorToInt((float)AllyTowerDamage / 50.0f);
        // // Aim to a ratio to 4 enemy units killed per each unit spawned
        // float reward = 0.0f;
        // if (NumberEnemyUnitsKilled > NumberUnitsSpawned)
        // {
        //     reward = 0.5f + (Mathf.Clamp01(((float)NumberEnemyUnitsKilled / (float)NumberUnitsSpawned) / 3.0f) / 2.0f);
        // }
        // else
        // {
        //     reward = 0.5f + (Mathf.Clamp01(((float)NumberUnitsSpawned / (float)NumberEnemyUnitsKilled) / 3.0f) / 2.0f) * -1.0f;
        // }
        // SetReward(reward);
        // RewardEpisode = reward;
        EnemyTowerDestroyed = true;
    }

    public void PenalizeTowerDestroyed()
    {
        // if (NumberUnitsSpawned <= 0) NumberUnitsSpawned = 1000;
        // if (NumberEnemyUnitsKilled <= 0) NumberEnemyUnitsKilled = 1;

        // NumberEnemyUnitsKilled += Mathf.FloorToInt((float)EnemyTowerDamage / 50.0f);
        // float reward = 0.0f;
        // if (NumberEnemyUnitsKilled > NumberUnitsSpawned)
        // {
        //     reward = -0.5f + (Mathf.Clamp01(((float)NumberEnemyUnitsKilled / (float)NumberUnitsSpawned) / 3.0f) / 2.0f);
        // }
        // else
        // {
        //     reward = -0.5f + (Mathf.Clamp01(((float)NumberUnitsSpawned / (float)NumberEnemyUnitsKilled) / 3.0f) / 2.0f) * -1.0f;
        // }
        // SetReward(reward);
        // RewardEpisode = reward;
        AllyTowerDestroyed = true;
    }

    public void Tie()
    {
        // if (NumberUnitsSpawned <= 0) NumberUnitsSpawned = 1000;
        // if (NumberEnemyUnitsKilled <= 0) NumberEnemyUnitsKilled = 1;

        // NumberEnemyUnitsKilled += Mathf.FloorToInt((float)EnemyTowerDamage / 50.0f);
        // NumberUnitsSpawned += Mathf.FloorToInt((float)AllyTowerDamage / 50.0f);
        // float reward = 0.0f;
        // if (NumberEnemyUnitsKilled > NumberUnitsSpawned)
        // {
        //     reward = -0.5f + (Mathf.Clamp01(((float)NumberEnemyUnitsKilled / (float)NumberUnitsSpawned) / 3.0f) / 2.0f);
        // }
        // else
        // {
        //     reward = -0.5f + (Mathf.Clamp01(((float)NumberUnitsSpawned / (float)NumberEnemyUnitsKilled) / 3.0f) / 2.0f) * -1.0f;
        // }

        // SetReward(reward);
        // RewardEpisode = reward;
        TieMatch = true;
    }

    public void PenalizeTime()
    {
        const float reward = 4.0f / 480.0f;
        AddReward(-reward);
        RewardEpisode -= reward;
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
