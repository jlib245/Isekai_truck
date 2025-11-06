using UnityEngine;
using UnityEngine.UI; // 기본 UI Text 사용
// using TMPro; // TextMeshPro를 사용한다면 이걸로 변경하고 Text -> TextMeshProUGUI

public class UIManager : MonoBehaviour
{
    // Inspector에서 UI 요소들을 연결해주세요.
    public Text scoreText;       // 점수 텍스트 (TextMeshProUGUI 사용 시: public TextMeshProUGUI scoreText;)
    public Text hpText;          // (NEW) HP 텍스트
    public GameObject gameOverPanel; // 게임 오버 패널

    void Start()
    {
        // GameManager의 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnStateChanged += OnStateChanged;
            GameManager.Instance.OnHPChanged += UpdateHP; // (NEW) HP 이벤트 구독
        }

        // 초기화
        if (scoreText != null) scoreText.text = "Score: 0";
        if (hpText != null) hpText.text = "HP: ..."; // HP 초기 텍스트
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // 점수 UI 업데이트 (이벤트 핸들러)
    void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }

    // (NEW) HP UI 업데이트 (이벤트 핸들러)
    void UpdateHP(int newHP, int maxHP)
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {newHP} / {maxHP}";
        }
    }

    // 게임 상태 변경 처리 (이벤트 핸들러)
    void OnStateChanged(GameState newState) // 'GameManager.GameState' -> 'GameState'로 수정
    {
        if (newState == GameState.GameOver)
        {
            ShowGameOver();
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // UIManager가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnHPChanged -= UpdateHP; // (NEW) HP 이벤트 구독 해제
        }
    }
}