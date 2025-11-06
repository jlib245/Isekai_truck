using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// 게임의 상태를 정의합니다.
public enum GameState
{
    Ready,   // 시작 준비
    Playing, // 플레이 중
    GameOver // 게임 오버
}

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    public GameState state;
    public int score = 0;

    [Header("Player Stats")]
    public int maxHP = 3; // 최대 HP
    public int playerHP;  // 현재 HP

    // UI가 구독할 이벤트
    public delegate void ScoreChangeHandler(int newScore);
    public event ScoreChangeHandler OnScoreChanged;

    public delegate void HPChangeHandler(int newHP, int maxHP); // HP 변경 이벤트
    public event HPChangeHandler OnHPChanged;

    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnStateChanged;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // (필요하다면 씬 로드 시 유지)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // HP 초기화
        playerHP = maxHP; 
        
        SetState(GameState.Ready);
        StartCoroutine(StartGameDelay());
    }

    IEnumerator StartGameDelay()
    {
        // 1초 후 게임 시작
        yield return new WaitForSeconds(1f);
        SetState(GameState.Playing);

        // (중요) UI가 초기 HP 값을 받을 수 있도록 이벤트 호출
        OnHPChanged?.Invoke(playerHP, maxHP);
    }

    // 게임 상태 변경
    void SetState(GameState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(newState);
    }

    // 점수 추가
    public void AddScore(int amount)
    {
        if (state != GameState.Playing) return;
        score += amount;
        // 점수 변경 이벤트를 모든 구독자에게 알림
        OnScoreChanged?.Invoke(score);
    }
    
    // (NEW) 데미지 처리 함수
    public void TakeDamage(int damage)
    {
        if (state != GameState.Playing) return;

        playerHP -= damage;
        
        // UI 업데이트를 위해 HP 변경 이벤트 호출
        OnHPChanged?.Invoke(playerHP, maxHP);

        if (playerHP <= 0)
        {
            playerHP = 0;
            EndGame(); // HP가 0이 되면 게임 오버
        }
    }

    // 게임 오버 처리
    public void EndGame()
    {
        if (state == GameState.GameOver) return;
        SetState(GameState.GameOver);
        Debug.Log("Game Over! Final Score: " + score);
    }

    // 게임 재시작
    public void RestartGame()
    {
        // 현재 씬을 다시 로드하여 게임을 재시작합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}