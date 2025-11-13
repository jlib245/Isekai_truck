using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text hpText;
    public GameObject gameOverPanel;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnStateChanged += OnStateChanged;
            GameManager.Instance.OnHPChanged += UpdateHP;
        }

        InitializeUI();
    }

    void InitializeUI()
    {
        if (scoreText != null) scoreText.text = "Score: 0";
        if (hpText != null) hpText.text = "HP: ...";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + newScore;
    }

    void UpdateHP(int newHP, int maxHP)
    {
        if (hpText != null)
            hpText.text = $"HP: {newHP} / {maxHP}";
    }

    void OnStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver)
            ShowGameOver();
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnHPChanged -= UpdateHP;
        }
    }
}