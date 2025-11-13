using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState state;
    public int score = 0;

    [Header("Player Stats")]
    public int maxHP = 3;
    public int playerHP;

    [Header("Game Settings")]
    public float gameStartDelay = 1f;

    public delegate void ScoreChangeHandler(int newScore);
    public event ScoreChangeHandler OnScoreChanged;

    public delegate void HPChangeHandler(int newHP, int maxHP);
    public event HPChangeHandler OnHPChanged;

    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerHP = maxHP;
        SetState(GameState.Ready);
        StartCoroutine(StartGameDelay());
    }

    IEnumerator StartGameDelay()
    {
        yield return new WaitForSeconds(gameStartDelay);
        SetState(GameState.Playing);
        OnHPChanged?.Invoke(playerHP, maxHP);
    }

    void SetState(GameState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void AddScore(int amount)
    {
        if (state != GameState.Playing) return;
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void TakeDamage(int damage)
    {
        if (state != GameState.Playing) return;

        playerHP -= damage;
        OnHPChanged?.Invoke(playerHP, maxHP);

        if (playerHP <= 0)
        {
            playerHP = 0;
            EndGame();
        }
    }

    public void EndGame()
    {
        if (state == GameState.GameOver) return;
        SetState(GameState.GameOver);
        Debug.Log("Game Over! Final Score: " + score);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}