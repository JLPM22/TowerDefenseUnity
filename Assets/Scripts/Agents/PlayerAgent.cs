using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    public TeamController EnemyTeamController;

    private TeamController TeamController;
    private float RewardEpisode;

    private void Awake()
    {
        TeamController = GetComponent<TeamController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnemyTeamController.OnUnitDead += OnEnemyUnitKilled;
        TeamController.OnUnitDead += OnAllyUnitKilled;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EnemyTeamController.OnUnitDead -= OnEnemyUnitKilled;
        TeamController.OnUnitDead += OnAllyUnitKilled;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("RewardEpisode: " + RewardEpisode);
        RewardEpisode = 0.0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        const float maxGold = 2000.0f;
        const float maxGoldPerSecond = 20.0f;
        float totalUnits = TeamController.Units[0].Count +
                           TeamController.Units[1].Count +
                           TeamController.Units[2].Count +
                           TeamController.Units[3].Count;
        if (totalUnits == 0) totalUnits = 1.0f;
        sensor.AddObservation(TeamController.Gold / maxGold);                // Gold
        sensor.AddObservation(EnemyTeamController.Gold / maxGold);           // Enemy Gold
        sensor.AddObservation(TeamController.CurrentGoldPerSecond / maxGoldPerSecond); // Gold/Second
        sensor.AddObservation(EnemyTeamController.CurrentGoldPerSecond / maxGoldPerSecond); // Enemy Gold/Second
        sensor.AddObservation(TeamController.Units[0].Count / totalUnits);   // Number Swords
        sensor.AddObservation(TeamController.Units[1].Count / totalUnits);   // Number Samurais
        sensor.AddObservation(TeamController.Units[2].Count / totalUnits);   // Number Magos
        sensor.AddObservation(TeamController.Units[3].Count / totalUnits);   // Number Kings
        float totalEnemyUnits = EnemyTeamController.Units[0].Count +
                                EnemyTeamController.Units[1].Count +
                                EnemyTeamController.Units[2].Count +
                                EnemyTeamController.Units[3].Count;
        if (totalEnemyUnits == 0) totalEnemyUnits = 1.0f;
        sensor.AddObservation(EnemyTeamController.Units[0].Count / totalEnemyUnits); // Number Enemy Swords
        sensor.AddObservation(EnemyTeamController.Units[1].Count / totalEnemyUnits); // Number Enemy Samurais
        sensor.AddObservation(EnemyTeamController.Units[2].Count / totalEnemyUnits); // Number Enemy Magos
        sensor.AddObservation(EnemyTeamController.Units[3].Count / totalEnemyUnits); // Number Enemy Kings
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = (int)actions.DiscreteActions[0];
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
                break;
            case 2:
                // Samurai
                // Debug.Log("Samurai");
                spawned = TeamController.Spawn(TeamController.Unit.Samurai);
                break;
            case 3:
                // Mago
                // Debug.Log("Mago");
                spawned = TeamController.Spawn(TeamController.Unit.Mago);
                break;
            case 4:
                // King
                // Debug.Log("King");
                spawned = TeamController.Spawn(TeamController.Unit.King);
                break;
            case 5:
                // Explosion
                // Debug.Log("Explosion");
                spawned = TeamController.Spawn(TeamController.Unit.Explosion);
                break;
            case 6:
                // Increment Gold
                // Debug.Log("Increment Gold");
                spawned = TeamController.Spawn(TeamController.Unit.IncrementGold);
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
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

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
        alpha1Down = alpha2Down = alpha3Down = alpha4Down = alpha5Down = alpha6Down = false;
    }

    public void OnAllyUnitKilled()
    {
        AddReward(-0.1f);
        RewardEpisode -= 0.1f;
    }

    public void OnEnemyUnitKilled()
    {
        AddReward(0.05f); // Maintain rewards between -1 and 1 to have a stable training
        RewardEpisode += 0.05f;
    }

    public void RewardTowerDestroyed()
    {
        AddReward(1);
        RewardEpisode += 1;
        EndEpisode();
    }

    public void PenalizeTowerDestroyed()
    {
        // AddReward(-1);
        // RewardEpisode -= 1;
        EndEpisode();
    }

    public void PenalizeTime()
    {
        // AddReward(-0.01f);
        // RewardEpisode -= 0.01f;
    }
}
