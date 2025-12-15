using UnityEngine;
using UnityEngine.SceneManagement;

public class KirbySceneManager : MonoBehaviour
{
    public static KirbySceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "Start";
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string loadingSceneName = "Loading";

    [Header("Stage Scene Names (index = stageNumber-1)")]
    [SerializeField] private string[] stageSceneNames;

    [Header("Spawn")]
    [SerializeField] private string defaultSpawnId = "A";

    private int selectedStageIndex = -1;
    private string pendingSpawnId;

    private void Awake()
    {
        // ===== 예외/가드 =====
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            pendingSpawnId = defaultSpawnId;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeNextScene()
    {
        // ===== 예외/가드 =====
        string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (current == startSceneName)
        {
            LoadScene(lobbySceneName);
            return;
        }

        if (current == loadingSceneName)
        {
            string stageName = GetSelectedStageSceneName();
            if (string.IsNullOrEmpty(stageName) == true)
            {
                LoadScene(lobbySceneName);
                return;
            }

            LoadScene(stageName);
            return;
        }

        // stage에서 호출되면 "다음 스테이지"로
        if (IsStageScene(current) == true)
        {
            GoNextStage();
            return;
        }

        // 그 외는 로비로
        LoadScene(lobbySceneName);
    }

    // LobbyObject가 호출하는 함수 시그니처 그대로 유지 :contentReference[oaicite:4]{index=4}
    public void SelectStage(int stageNumber)
    {
        SelectStage(stageNumber, defaultSpawnId);
    }

    public void SelectStage(int stageNumber, string spawnId)
    {
        // ===== 예외/가드 =====
        if (stageSceneNames == null || stageSceneNames.Length <= 0)
        {
            return;
        }

        int index = stageNumber - 1;

        if (index < 0 || index >= stageSceneNames.Length)
        {
            return;
        }

        selectedStageIndex = index;
        pendingSpawnId = string.IsNullOrEmpty(spawnId) == true ? defaultSpawnId : spawnId;

        LoadScene(loadingSceneName);
    }

    // stageObject가 호출하는 함수 시그니처 그대로 유지 :contentReference[oaicite:5]{index=5}
    public void SetGameResult(bool isAnswer)
    {
        // 1차 빌드 기준: 정답/오답은 일단 "다음 스테이지 이동" 혹은 "로비 복귀"로만 쓴다.
        if (isAnswer == true)
        {
            GoNextStage();
            return;
        }

        LoadScene(lobbySceneName);
    }

    public void GoNextStage()
    {
        // ===== 예외/가드 =====
        if (stageSceneNames == null || stageSceneNames.Length <= 0)
        {
            return;
        }

        if (selectedStageIndex < 0)
        {
            selectedStageIndex = 0;
        }
        else
        {
            selectedStageIndex += 1;
        }

        if (selectedStageIndex >= stageSceneNames.Length)
        {
            // 마지막 스테이지 끝나면 로비로
            LoadScene(lobbySceneName);
            return;
        }

        pendingSpawnId = defaultSpawnId;
        LoadScene(loadingSceneName);
    }

    private void LoadScene(string sceneName)
    {
        // ===== 예외/가드 =====
        if (string.IsNullOrEmpty(sceneName) == true)
        {
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 스테이지 씬 로드 직후: 플레이어 위치를 SpawnPoint로 이동
        if (IsStageScene(scene.name) == false)
        {
            return;
        }

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
            StageSpawnPoint p = points[i];
            if (p != null && p.SpawnId == spawnId)
            {
                return p;
            }
        }

        return points[0];
    }

    private bool IsStageScene(string sceneName)
    {
        if (stageSceneNames == null)
        {
            return false;
        }

        for (int i = 0; i < stageSceneNames.Length; i++)
        {
            if (stageSceneNames[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private string GetSelectedStageSceneName()
    {
        if (stageSceneNames == null || stageSceneNames.Length <= 0)
        {
            return null;
        }

        if (selectedStageIndex < 0 || selectedStageIndex >= stageSceneNames.Length)
        {
            return null;
        }

        return stageSceneNames[selectedStageIndex];
    }
}
