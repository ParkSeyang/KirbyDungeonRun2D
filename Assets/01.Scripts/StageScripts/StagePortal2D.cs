using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class StagePortal2D : MonoBehaviour
{
    [Header("Player Filter")]
    [SerializeField] private string playerTag = "Player";

    [Header("Flow")]
    [SerializeField] private bool useStudySceneManager = true;

    [Header("Fallback (when no StudySceneManager)")]
    [SerializeField] private bool loadByNextBuildIndex = true;
    [SerializeField] private string fallbackSceneName = "";

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ===== 예외/가드 =====
        if (used == true)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(playerTag) == false)
        {
            if (other.CompareTag(playerTag) == false)
            {
                return;
            }
        }

        used = true;

        // 1) 로딩/코루틴 전환 흐름이 있으면 그걸 우선 사용
        if (useStudySceneManager == true)
        {
            if (KirbySceneManager.Instance != null)
            {
                KirbySceneManager.Instance.GoNextStage();
                return;
            }
        }

        // 2) 폴백: BuildIndex + 1 로 다음 씬
        if (loadByNextBuildIndex == true)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
                return;
            }
        }

        // 3) 폴백 씬 이름
        if (string.IsNullOrEmpty(fallbackSceneName) == false)
        {
            SceneManager.LoadScene(fallbackSceneName);
        }
    }
}