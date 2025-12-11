using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 중 일시정지 메뉴를 관리하는 스크립트
/// ESC 키로 일시정지/재개 가능
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    void Start()
    {
        // 버튼 이벤트 연결
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // 시작 시 메뉴 숨기기
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // ESC 키로 일시정지 토글
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // 게임 시간 정지

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Debug.Log("[PauseMenu] 게임 일시정지");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f; // 게임 시간 재개

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Debug.Log("[PauseMenu] 게임 재개");
    }

    public void RestartGame()
    {
        // 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Time.timeScale = 1f; // 시간 되돌리기
        isPaused = false;

        // 현재 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Debug.Log("[PauseMenu] 게임 재시작");
    }

    public void GoToMainMenu()
    {
        // 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Time.timeScale = 1f; // 시간 되돌리기
        isPaused = false;

        // 타이틀 씬으로 이동
        SceneManager.LoadScene("TitleScene");

        Debug.Log("[PauseMenu] 메인 메뉴로 이동");
    }

    void OnDestroy()
    {
        // 씬 전환 시 시간 스케일 복원
        Time.timeScale = 1f;
    }
}
