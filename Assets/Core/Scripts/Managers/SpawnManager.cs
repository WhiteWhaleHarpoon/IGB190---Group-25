using MyUtilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the spawning of enemies in the game, including the generation of spawn points
/// and the spawning of specific monsters.
/// </summary>
public class SpawnManager
{
    private readonly List<EnemySpawn> unitSpawnCache = new List<EnemySpawn>();

    /// <summary>
    /// Initializes the SpawnManager and generates initial enemy spawns.
    /// </summary>
    public SpawnManager(int totalMonsters, Monster[] spawnableMonsters)
    {
        GenerateSpawns(totalMonsters, spawnableMonsters);
    }

    /// <summary>
    /// Represents an enemy spawn with position, monster type, and empowerment status.
    /// </summary>
    private class EnemySpawn
    {
        public Vector3 Position { get; }
        public Unit Monster { get; }
        public bool IsEmpowered { get; }

        public EnemySpawn(Vector3 position, Unit monster, bool isEmpowered = false)
        {
            Position = position;
            Monster = monster;
            IsEmpowered = isEmpowered;
        }
    }

    /// <summary>
    /// Generates a list of enemy spawns based on the number of required spawns and available monsters.
    /// </summary>
    public void GenerateSpawns(int requiredSpawns, Monster[] spawnableMonsters)
    {
        

        int totalMonsterLikelihood = 0;
        foreach (Monster monster in spawnableMonsters)
        {
            totalMonsterLikelihood += monster.spawnLikelihood;
        }

        MonsterSpawnArea[] spawnLocations = GameObject.FindObjectsByType<MonsterSpawnArea>(FindObjectsSortMode.None);
        float totalDensity = 0;
        foreach (MonsterSpawnArea area in spawnLocations)
        {
            totalDensity += area.CalculateBoxSpawnDensity();
            
        }

        foreach (MonsterSpawnArea area in spawnLocations)
        {
            int toSpawn = Mathf.RoundToInt((area.CalculateBoxSpawnDensity() / totalDensity) * requiredSpawns);

            for (int i = 0; i < toSpawn; i++)
            {
                bool isEmpowered = area.empoweredMonsters > 0;
                area.empoweredMonsters--;
                Vector3 spawnPosition = Utilities.GetValidNavMeshPosition(area.GetRandomVectorInCollider());
                Monster monsterToSpawn = GetMonsterToSpawn(totalMonsterLikelihood, spawnableMonsters);
                unitSpawnCache.Add(new EnemySpawn(spawnPosition, monsterToSpawn, isEmpowered));
            }
        }
    }

    /// <summary>
    /// Determines which monster to spawn based on their likelihood.
    /// </summary>
    private Monster GetMonsterToSpawn(int totalSpawnLikelihood, Monster[] spawnableMonsters)
    {
        int value = Random.Range(0, totalSpawnLikelihood);

        foreach (Monster monster in spawnableMonsters)
        {
            if (value < monster.spawnLikelihood)
                return monster;
            value -= monster.spawnLikelihood;
        }

        return null;
    }

    /// <summary>
    /// Attempts to spawn enemies from the cache if they are within range of the player.
    /// </summary>
    private void TrySpawnEnemies()
    {
        for (int i = 0; i < unitSpawnCache.Count; i++)
        {
            if (Vector3.Distance(GameManager.player.transform.position, unitSpawnCache[i].Position) < 10)
            {
                SpawnUnit(unitSpawnCache[i].Monster, unitSpawnCache[i].Position, unitSpawnCache[i].IsEmpowered);
                unitSpawnCache.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Spawns a unit at the specified position, optionally with an empowerment effect.
    /// </summary>
    public void SpawnUnit(Unit unit, Vector3 position, bool empowered = false)
    {
        if (unit != null)
        {
            UnitSpawnEffect spawnEffect = (unit as Monster)?.spawnEffect;

            if (spawnEffect != null)
            {
                GameManager.spawner.SpawnUnitWithEffect(unit, position, spawnEffect, empowered);
            }
            else
            {
                Unit spawnedUnit = GameObject.Instantiate(unit, position, Quaternion.identity);
                if (spawnedUnit is Monster monster && empowered)
                {
                    monster.Empower();
                }
            }
        }
    }
}