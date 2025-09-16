using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    public float spawnDensity = 1.0f;
    public List<Monster> monstersToSpawn;

    private List<EnemySpawn> unitSpawnCache = new List<EnemySpawn>();

    private class EnemySpawn
    {
        public EnemySpawn(Vector3 position, Unit monster, bool isEmpowered = false)
        {
            this.position = position;
            this.monster = monster;
            this.isEmpowered = isEmpowered;
        }
        public Vector3 position;
        public Unit monster;
        public bool isEmpowered;
    }

    private void Start()
    {
        GenerateSpawns();
    }

    private void Update()
    {
        TrySpawnEnemies();
    }

    private Monster GetMonsterToSpawn(int totalSpawnLiklihood, int maximumRoomLevel)
    {
        bool validMonster = false;
        for (int i = 0; i < monstersToSpawn.Count; i++)
        {
            if (monstersToSpawn[i].spawnLevel <= maximumRoomLevel)
            {
                validMonster = true;
                break;
            }
        }
        if (!validMonster)
        {
            Debug.Log("There are no valid monsters to spawn with the minimum spawn level needed for a room.");
            return null;
        }

        int remainingSpawns = monstersToSpawn.Count;
        Monster monsterToSpawn = null;
        while (monsterToSpawn == null)
        {
            int value = Random.Range(0, totalSpawnLiklihood);
            for (int i = 0; i < monstersToSpawn.Count; i++)
            {
                if (value <= monstersToSpawn[i].spawnLikelihood)
                {
                    if (monstersToSpawn[i].spawnLevel <= maximumRoomLevel)
                        monsterToSpawn = monstersToSpawn[i];
                    break;
                }
                else
                    value -= monstersToSpawn[i].spawnLikelihood;
            }
        }
        return monsterToSpawn;
    }

    public void GenerateSpawns() 
    {
        int totalMonsterLikelihood = 0;
        for (int i = 0; i < monstersToSpawn.Count; i++)
        {
            totalMonsterLikelihood += monstersToSpawn[i].spawnLikelihood;
        }

        MonsterSpawnArea[] spawnLocations = GameObject.FindObjectsByType<MonsterSpawnArea>(FindObjectsSortMode.None); 

        float modifier = 0.02f;
        float total = 0;
        foreach (MonsterSpawnArea trigger in spawnLocations)
        {
            total += trigger.transform.localScale.x * trigger.transform.localScale.y * trigger.spawnDensity;
        }

        foreach (MonsterSpawnArea trigger in spawnLocations)
        {
            int toSpawn = (int)(trigger.GetSpawnAreaSize() * spawnDensity * modifier * trigger.spawnDensity);
            for (int i = 0; i < toSpawn; i++)
            {
                bool isEmpowered = trigger.empoweredMonsters > 0;
                trigger.empoweredMonsters--;
                unitSpawnCache.Add(new EnemySpawn(Utilities.GetValidNavMeshPosition(trigger.GetRandomVectorInCollider()),
                    GetMonsterToSpawn(totalMonsterLikelihood, trigger.maximumSpawnLevel), isEmpowered));
            }
        }
    }

    private void TrySpawnEnemies()
    {
        for (int i = 0; i < unitSpawnCache.Count; i++)
        {
            if (Vector3.Distance(GameManager.player.transform.position, unitSpawnCache[i].position) < 10)
            {
                GameManager.spawner.SpawnUnit(unitSpawnCache[i].monster, unitSpawnCache[i].position, unitSpawnCache[i].isEmpowered);
                unitSpawnCache.RemoveAt(i);
                i--;
            }
        }
    }




    public void SpawnMonsterWithEffect(Monster monster, Vector3 location, GameObject effect, float duration, GameObject finalEffect)
    {
        GameObject obj = ObjectPooler.InstantiatePooled(effect, location, Quaternion.identity);
        Destroy(obj, duration + .5f);
        StartCoroutine(SpawnMonsterWithEffectCoroutine(monster, location, duration, finalEffect));
    }

    private IEnumerator SpawnMonsterWithEffectCoroutine(Monster monster, Vector3 location, float duration, GameObject effect)
    {
        yield return new WaitForSeconds(duration);
        Monster m = Instantiate(monster, location, Quaternion.identity);
    }

    public void SpawnUnit(Unit unit, Vector3 position, bool empowered = false)
    {
        if (unit != null)
        {
            UnitSpawnEffect spawnEffect = null;
            if (unit is Monster) spawnEffect = ((Monster)unit).spawnEffect;
            if (spawnEffect != null)
            {
                SpawnUnitWithEffect(unit, position, spawnEffect, empowered);
            }
            else
            {
                Unit u = GameObject.Instantiate(unit, position, Quaternion.identity);
                if (u is Monster && empowered) ((Monster)u).Empower();
            }
        }
    }

    public void SpawnUnitWithEffect(Unit unit, Vector3 position, UnitSpawnEffect spawnEffect, bool empowered = false)
    {
        if (unit == null) return;
        if (spawnEffect == null) return;
        GameManager.instance.StartCoroutine(SpawnUnitCoroutine(unit, position, spawnEffect, empowered));
    }

    private IEnumerator SpawnUnitCoroutine(Unit unit, Vector3 position, UnitSpawnEffect spawnEffect, bool empowered = false)
    {
        float duration = spawnEffect.GetComponent<UnitSpawnEffect>().effectDuration;
        ObjectPooler.InstantiatePooled(spawnEffect.gameObject, position, Quaternion.identity);
        yield return new WaitForSeconds(duration);
        Unit u = GameObject.Instantiate(unit, position, Quaternion.identity);
        if (u is Monster && empowered) ((Monster)u).Empower();
    }
}
