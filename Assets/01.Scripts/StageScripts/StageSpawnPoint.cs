using UnityEngine;

[DisallowMultipleComponent]
public class StageSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "A";

    public string SpawnId
    {
        get { return spawnId; }
    }
}