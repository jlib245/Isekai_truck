using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public SpawnManager spawnManager;
    public PlayerController playerController;

    [Header("Quest Settings")]
    public int baseReward = 1000;
    public int minRequired = 1;
    public int maxRequired = 3;

    [Header("Difficulty Settings")]
    public int difficultyLevel = 1;
    [Tooltip("레벨당 플레이어 속도 증가량")]
    public float speedIncreasePerLevel = 0.5f;
    [Tooltip("레벨당 스폰 간격 감소량")]
    public float spawnIntervalDecreasePerLevel = 0.05f;
    [Tooltip("최소 스폰 간격")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("기본 속도 대비 최대 증가 배율 (예: 2.0 = 2배까지)")]
    public float maxSpeedMultiplier = 2.0f;

    [Header("Time-based Difficulty")]
    [Tooltip("몇 초마다 난이도 증가")]
    public float difficultyIncreaseInterval = 30f;
    private float difficultyTimer = 0f;

    private float basePlayerSpeed = -1f;
    private float baseSpawnInterval = -1f;

    private Quest currentQuest;
    private List<GameObject> availableHeroPrefabs = new List<GameObject>();

    public delegate void QuestUpdateHandler(Quest quest);
    public event QuestUpdateHandler OnQuestUpdated;
    public event QuestUpdateHandler OnQuestCompleted;
    public event QuestUpdateHandler OnQuestStarted;

    public delegate void DifficultyChangeHandler(int level);
    public event DifficultyChangeHandler OnDifficultyChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // SpawnManager가 없으면 찾기
        if (spawnManager == null)
            spawnManager = FindObjectOfType<SpawnManager>();

        // PlayerController가 없으면 찾기
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        // 초기값 저장
        if (spawnManager != null)
            baseSpawnInterval = spawnManager.spawnInterval;
        if (playerController != null)
            basePlayerSpeed = playerController.maxSpeed;

        if (spawnManager != null && spawnManager.targetPrefabs.Length > 0)
        {
            // HeroType이 있는 프리팹만 필터링
            foreach (GameObject prefab in spawnManager.targetPrefabs)
            {
                if (prefab.GetComponent<HeroType>() != null)
                {
                    availableHeroPrefabs.Add(prefab);
                }
            }

            if (availableHeroPrefabs.Count > 0)
            {
                GenerateNewQuest();
            }
            else
            {
                Debug.LogWarning("[QuestManager] HeroType 컴포넌트가 있는 프리팹이 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("[QuestManager] SpawnManager 또는 targetPrefabs가 없습니다!");
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.state != GameState.Playing)
            return;

        // 시간 기반 난이도 증가
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            difficultyTimer = 0f;
            IncreaseDifficulty();
            Debug.Log($"[QuestManager] 시간 경과로 난이도 증가!");
        }
    }

    void GenerateNewQuest()
    {
        if (availableHeroPrefabs.Count == 0)
        {
            Debug.LogWarning("[QuestManager] 사용 가능한 Hero 프리팹이 없습니다!");
            return;
        }

        // 랜덤 프리팹 선택
        GameObject selectedPrefab = availableHeroPrefabs[Random.Range(0, availableHeroPrefabs.Count)];
        int requiredCount = Random.Range(minRequired, maxRequired + 1);
        int reward = baseReward * requiredCount;

        currentQuest = new Quest(selectedPrefab, requiredCount, reward);

        Debug.Log($"[QuestManager] 새 퀘스트 생성: {currentQuest.GetQuestDescription()}, 보상: {reward}G");
        OnQuestStarted?.Invoke(currentQuest);
    }

    public Quest GetCurrentQuest()
    {
        return currentQuest;
    }

    public bool CheckAndUpdateQuest(GameObject hero)
    {
        if (currentQuest == null || currentQuest.IsComplete())
            return false;

        // 프리팹이 일치하는지 확인
        HeroType heroType = hero.GetComponent<HeroType>();
        HeroType questHeroType = currentQuest.targetPrefab.GetComponent<HeroType>();

        if (heroType != null && questHeroType != null && heroType.heroID == questHeroType.heroID)
        {
            currentQuest.AddProgress();
            Debug.Log($"[QuestManager] 퀘스트 진행: {currentQuest.GetProgressText()}");
            OnQuestUpdated?.Invoke(currentQuest);

            if (currentQuest.IsComplete())
            {
                CompleteQuest();
            }

            return true;
        }

        return false;
    }

    void CompleteQuest()
    {
        if (currentQuest == null) return;

        Debug.Log($"[QuestManager] 퀘스트 완료! 보상: {currentQuest.rewardMoney}G");

        // 퀘스트 완료 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayQuestCompleteSound();
        }

        // 보상 지급
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(currentQuest.rewardMoney);

            // 퀘스트 완료 보상 텍스트 표시 (플레이어 위치에)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowRewardText(currentQuest.rewardMoney, player.transform.position + Vector3.up * 3f);
            }
        }

        OnQuestCompleted?.Invoke(currentQuest);

        // 난이도 증가
        IncreaseDifficulty();

        // 새로운 퀘스트 생성
        GenerateNewQuest();
    }

    void IncreaseDifficulty()
    {
        difficultyLevel++;
        Debug.Log($"[QuestManager] 난이도 증가! 레벨: {difficultyLevel}");

        // 플레이어 속도 증가
        if (playerController != null)
        {
            // 초기값이 설정 안 됐으면 현재 값 사용
            if (basePlayerSpeed < 0)
                basePlayerSpeed = playerController.maxSpeed;

            float newSpeed = basePlayerSpeed + (speedIncreasePerLevel * (difficultyLevel - 1));
            float maxSpeed = basePlayerSpeed * maxSpeedMultiplier;
            playerController.maxSpeed = Mathf.Min(newSpeed, maxSpeed);
            Debug.Log($"[QuestManager] 플레이어 속도: {playerController.maxSpeed}");
        }

        // 스폰 간격 감소 (더 빠르게 스폰)
        if (spawnManager != null)
        {
            // 초기값이 설정 안 됐으면 현재 값 사용
            if (baseSpawnInterval < 0)
                baseSpawnInterval = spawnManager.spawnInterval;

            float newInterval = baseSpawnInterval - (spawnIntervalDecreasePerLevel * (difficultyLevel - 1));
            spawnManager.spawnInterval = Mathf.Max(newInterval, minSpawnInterval);
            Debug.Log($"[QuestManager] 스폰 간격: {spawnManager.spawnInterval}초 (base: {baseSpawnInterval})");
        }
        else
        {
            Debug.LogWarning("[QuestManager] spawnManager가 null입니다!");
        }

        OnDifficultyChanged?.Invoke(difficultyLevel);
    }

    public void ResetDifficulty()
    {
        difficultyLevel = 1;
        difficultyTimer = 0f;
        if (playerController != null && basePlayerSpeed > 0)
            playerController.maxSpeed = basePlayerSpeed;
        if (spawnManager != null && baseSpawnInterval > 0)
            spawnManager.spawnInterval = baseSpawnInterval;
    }

    public bool IsQuestTarget(GameObject hero)
    {
        if (currentQuest == null || currentQuest.IsComplete())
            return false;

        HeroType heroType = hero.GetComponent<HeroType>();
        HeroType questHeroType = currentQuest.targetPrefab.GetComponent<HeroType>();

        return heroType != null && questHeroType != null && heroType.heroID == questHeroType.heroID;
    }

    // 황금 용사용: 무조건 퀘스트 1 진행
    public void ForceAddQuestProgress()
    {
        if (currentQuest == null || currentQuest.IsComplete())
            return;

        currentQuest.AddProgress();
        Debug.Log($"[QuestManager] 황금 용사로 퀘스트 강제 진행: {currentQuest.GetProgressText()}");
        OnQuestUpdated?.Invoke(currentQuest);

        if (currentQuest.IsComplete())
        {
            CompleteQuest();
        }
    }
}
