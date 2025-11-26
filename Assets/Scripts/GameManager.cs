using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    public int money = 0;           // 현재 게임에서 번 돈
    public int totalMoney = 0;      // 누적 금액 (게임 간 유지)

    [Header("Player Stats")]
    public int maxHP = 3;
    public int playerHP;

    private const string TOTAL_MONEY_KEY = "TotalMoney";

    [Header("Game Settings")]
    public float gameStartDelay = 1f;

    [Header("Hero Collection")]
    public Transform heroStorageParent;
    public Vector3 storageAreaCenter = new Vector3(0, 0, -30);
    public float heroSpacing = 2f;

    private List<GameObject> collectedHeroes = new List<GameObject>();
    private List<GameObject> collectedObstacles = new List<GameObject>();

    public delegate void MoneyChangeHandler(int newMoney);
    public event MoneyChangeHandler OnMoneyChanged;

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
            LoadTotalMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyUpgrades();
        playerHP = maxHP;
        SetState(GameState.Ready);
        StartCoroutine(StartGameDelay());
    }

    void ApplyUpgrades()
    {
        // UpgradeManager에서 업그레이드된 maxHP 적용
        if (UpgradeManager.Instance != null)
        {
            maxHP = UpgradeManager.Instance.GetMaxHP();
            Debug.Log($"[GameManager] 업그레이드 적용 - 최대 HP: {maxHP}");
        }
    }

    void LoadTotalMoney()
    {
        totalMoney = PlayerPrefs.GetInt(TOTAL_MONEY_KEY, 0);
        Debug.Log($"[GameManager] 누적 금액 로드: {totalMoney}G");
    }

    public void SaveTotalMoney()
    {
        PlayerPrefs.SetInt(TOTAL_MONEY_KEY, totalMoney);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] 누적 금액 저장: {totalMoney}G");
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

    public void AddMoney(int amount)
    {
        if (state != GameState.Playing) return;
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public void LoseMoney(int amount)
    {
        if (state != GameState.Playing) return;
        money -= amount;
        if (money < 0) money = 0;
        OnMoneyChanged?.Invoke(money);
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

    public void AddCollectedHero(GameObject hero)
    {
        collectedHeroes.Add(hero);

        // 일단 화면 밖으로 이동 (GameOverScene에서 다시 배치될 것임)
        hero.transform.position = new Vector3(0, -1000, 0);
        hero.transform.rotation = Quaternion.Euler(0, 0, 0);

        // DontDestroyOnLoad로 씬 전환 시에도 유지
        DontDestroyOnLoad(hero);

        // Rigidbody 비활성화
        Rigidbody rb = hero.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 키네마틱이 아닌 경우에만 속도 초기화
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }

        Debug.Log($"[GameManager] 용사 저장 완료. 총 {collectedHeroes.Count}명");
    }

    public void AddCollectedObstacle(GameObject obstacle)
    {
        collectedObstacles.Add(obstacle);

        // 일단 화면 밖으로 이동
        obstacle.transform.position = new Vector3(0, -1000, 0);
        obstacle.transform.rotation = Quaternion.Euler(0, 0, 0);

        // DontDestroyOnLoad로 씬 전환 시에도 유지
        DontDestroyOnLoad(obstacle);

        // Rigidbody 비활성화
        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 키네마틱이 아닌 경우에만 속도 초기화
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }

        Debug.Log($"[GameManager] 장애물 저장 완료. 총 {collectedObstacles.Count}개");
    }

    public List<GameObject> GetCollectedHeroes()
    {
        return collectedHeroes;
    }

    public List<GameObject> GetCollectedObstacles()
    {
        return collectedObstacles;
    }

    public Vector3 GetStorageAreaCenter()
    {
        return storageAreaCenter;
    }

    public void EndGame()
    {
        if (state == GameState.GameOver) return;
        SetState(GameState.GameOver);

        // 현재 게임에서 번 돈을 누적 금액에 추가
        totalMoney += money;
        SaveTotalMoney();

        Debug.Log($"Game Over! 이번 게임 수익: {money}G, 총 누적: {totalMoney}G");

        // GameOverScene으로 전환
        StartCoroutine(LoadGameOverScene());
    }

    IEnumerator LoadGameOverScene()
    {
        Debug.Log("[GameManager] LoadGameOverScene 코루틴 시작");
        // 3초 대기 (날아가는 용사/장애물 보여주기)
        yield return new WaitForSeconds(3f);
        Debug.Log("[GameManager] GameOverScene 로딩 시도...");
        SceneManager.LoadScene("GameOverScene");
        Debug.Log("[GameManager] SceneManager.LoadScene 호출 완료");
    }

    public void RestartGame()
    {
        // 게임 재시작 시 용사/장애물 목록 초기화
        foreach (GameObject hero in collectedHeroes)
        {
            if (hero != null) Destroy(hero);
        }
        foreach (GameObject obstacle in collectedObstacles)
        {
            if (obstacle != null) Destroy(obstacle);
        }
        collectedHeroes.Clear();
        collectedObstacles.Clear();

        // 상태 초기화 (현재 게임 돈만 리셋, 누적 금액은 유지)
        money = 0;
        playerHP = maxHP;
        SetState(GameState.Ready);

        // 씬 로드 후 게임 시작
        StartCoroutine(RestartGameSequence());
    }

    IEnumerator RestartGameSequence()
    {
        // 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        // 씬 로드 완료 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // UIManager가 이벤트를 구독할 때까지 대기
        float waitTime = 0f;
        while (OnMoneyChanged == null && waitTime < 2f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        // UI 업데이트를 위한 이벤트 발생 (씬 로드 후)
        OnMoneyChanged?.Invoke(money);
        OnHPChanged?.Invoke(playerHP, maxHP);

        // 게임 시작 딜레이
        yield return new WaitForSeconds(gameStartDelay);

        SetState(GameState.Playing);

        Debug.Log("Game Restarted!");
    }
}