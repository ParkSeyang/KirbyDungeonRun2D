using System.Collections.Generic;
using UnityEngine;
using EnemySystem.Data;
using EnemySystem.Runtime;

public class EnemySpawnController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private EnemyPrefabsData enemyPrefabsData;

    [Header("Optional Parent (spawned enemies root)")]
    [SerializeField] private Transform spawnRoot;

    [Header("Runtime Track (Debug)")]
    [SerializeField] private List<GameObject> spawnedEnemies = new List<GameObject>();

    public GameObject SpawnEnemy(EnemyKey key, Vector3 position)
    {
        return SpawnEnemy(key, position, Quaternion.identity);
    }

    public GameObject SpawnEnemy(EnemyKey key, Vector3 position, Quaternion rotation)
    {
        if (enemyPrefabsData == null)
        {
            Debug.LogWarning("[EnemySpawnController] enemyPrefabsData is null.");
            return null;
        }

        EnemyPrefabEntry entry = null;
        bool found = enemyPrefabsData.TryGet(key, out entry);

        if (found == false || entry == null)
        {
            Debug.LogWarning("[EnemySpawnController] Entry not found: " + key);
            return null;
        }

        if (entry.EnemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawnController] EnemyPrefab is null for key: " + key);
            return null;
        }

        Transform parent = spawnRoot;

        GameObject obj = (parent != null)
            ? Instantiate(entry.EnemyPrefab, position, rotation, parent)
            : Instantiate(entry.EnemyPrefab, position, rotation);

        if (obj == null)
        {
            return null;
        }

        EnemySpawnedContext ctx = obj.GetComponent<EnemySpawnedContext>();

        if (ctx == null)
        {
            ctx = obj.AddComponent<EnemySpawnedContext>();
        }

        if (ctx != null)
        {
            ctx.InitializeFromEntry(entry);
        }

        if (spawnedEnemies == null)
        {
            spawnedEnemies = new List<GameObject>();
        }

        spawnedEnemies.Add(obj);
        return obj;
    }

    public bool DespawnEnemy(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return false;
        }

        if (spawnedEnemies != null)
        {
            spawnedEnemies.Remove(enemyObject);
        }

        Destroy(enemyObject);
        return true;
    }

    public void DespawnAll()
    {
        if (spawnedEnemies == null)
        {
            return;
        }

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedEnemies[i];

            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawnedEnemies.Clear();
    }
}
