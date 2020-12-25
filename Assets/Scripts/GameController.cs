using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool EnableAgent1 = true;
    public bool EnableAgent2 = true;
    public TeamController Team1;
    public TeamController Team2;
    public TowerController Team1Tower;
    public TowerController Team2Tower;
    public PlayerAgent AgentTeam1;
    public PlayerAgent AgentTeam2;

    private void Start()
    {
        Reset();
    }

    private void OnEnable()
    {
        Team1Tower.OnDestroyed += OnTeam1TowerDestroyed;
        Team2Tower.OnDestroyed += OnTeam2TowerDestroyed;
    }

    private void OnDisable()
    {
        Team1Tower.OnDestroyed -= OnTeam1TowerDestroyed;
        Team2Tower.OnDestroyed -= OnTeam2TowerDestroyed;
    }

    private void Reset()
    {
        Team1.Reset();
        Team2.Reset();
        StopAllCoroutines();
        StartCoroutine(PenalizeTime());
        StartCoroutine(EndEpisodeTime());
    }

    private IEnumerator PenalizeTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (EnableAgent1)
                AgentTeam1.PenalizeTime();
            if (EnableAgent2)
                AgentTeam2.PenalizeTime();
        }
    }

    private IEnumerator EndEpisodeTime()
    {
        yield return new WaitForSeconds(120.0f);
        if (EnableAgent1) AgentTeam1.EndEpisode();
        if (EnableAgent2) AgentTeam2.EndEpisode();
        StopAllCoroutines();
        Reset();
    }

    private void OnTeam1TowerDestroyed()
    {
        if (EnableAgent1)
            AgentTeam1.PenalizeTowerDestroyed();
        if (EnableAgent2)
            AgentTeam2.RewardTowerDestroyed();
        Reset();
    }

    private void OnTeam2TowerDestroyed()
    {
        if (EnableAgent1)
            AgentTeam1.RewardTowerDestroyed();
        if (EnableAgent2)
            AgentTeam2.PenalizeTowerDestroyed();
        Reset();
    }
}
