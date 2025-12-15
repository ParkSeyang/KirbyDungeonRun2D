using UnityEngine;
using EnemySystem.Data;

public class EnemySpawnTester : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private EnemySpawnController spawnController;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Keys to Spawn")]
    [SerializeField] private EnemyKey key1 = EnemyKey.NormalEnemy;
    [SerializeField] private EnemyKey key2 = EnemyKey.NormalEnemy2;
    [SerializeField] private EnemyKey key3 = EnemyKey.CutterEnemy;
    [SerializeField] private EnemyKey key4 = EnemyKey.SwordEnemy;
    [SerializeField] private EnemyKey key5 = EnemyKey.NormalEnemy3;

    private void Awake()
    {
        if (spawnController == null)
        {
            spawnController = GetComponent<EnemySpawnController>();
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    private void Update()
    {
        if (spawnController == null)
        {
            return;
        }

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;

        if (Input.GetKeyDown(KeyCode.Alpha1) == true) { spawnController.SpawnEnemy(key1, pos); }
        if (Input.GetKeyDown(KeyCode.Alpha2) == true) { spawnController.SpawnEnemy(key2, pos); }
        if (Input.GetKeyDown(KeyCode.Alpha3) == true) { spawnController.SpawnEnemy(key3, pos); }
        if (Input.GetKeyDown(KeyCode.Alpha4) == true) { spawnController.SpawnEnemy(key4, pos); }
        if (Input.GetKeyDown(KeyCode.Alpha5) == true) { spawnController.SpawnEnemy(key5, pos); }
    }
}