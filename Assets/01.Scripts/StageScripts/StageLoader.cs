using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class StageLoader : MonoBehaviour
{
    public static StageLoader Instance { get; private set; }

    private string pendingSpawnId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoadStage(string sceneName, string spawnId)
    {
        // ===== 예외/가드 =====
        if (string.IsNullOrEmpty(sceneName) == true)
        {
            return;
        }

        pendingSpawnId = spawnId;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject playerObject = ResolvePlayerObject();

        if (playerObject == null)
        {
            return;
        }

        StageSpawnPoint spawnPoint = FindSpawnPoint(pendingSpawnId);

        if (spawnPoint == null)
        {
            return;
        }

        playerObject.transform.position = spawnPoint.transform.position;

        Rigidbody2D rb = playerObject.GetComponentInChildren<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0.0f;
        }
    }

    private GameObject ResolvePlayerObject()
    {
        if (PlayerController.localPlayer != null)
        {
            return PlayerController.localPlayer.gameObject;
        }

        if (Player.instance != null)
        {
            return Player.instance.gameObject;
        }

        return null;
    }

    private StageSpawnPoint FindSpawnPoint(string spawnId)
    {
        StageSpawnPoint[] points = Object.FindObjectsOfType<StageSpawnPoint>();

        if (points == null || points.Length <= 0)
        {
            return null;
        }

        if (string.IsNullOrEmpty(spawnId) == true)
        {
            return points[0];
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].SpawnId == spawnId)
            {
                return points[i];
            }
        }

        return points[0];
    }
}
