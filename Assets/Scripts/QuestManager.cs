using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public SpawnManager spawnManager;

    [Header("Quest Settings")]
    public int baseReward = 1000;
    public int minRequired = 1;
    public int maxRequired = 3;

    private Quest currentQuest;
    private List<GameObject> availableHeroPrefabs = new List<GameObject>();

    public delegate void QuestUpdateHandler(Quest quest);
    public event QuestUpdateHandler OnQuestUpdated;
    public event QuestUpdateHandler OnQuestCompleted;
    public event QuestUpdateHandler OnQuestStarted;

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

        // 보상 지급
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(currentQuest.rewardMoney);
        }

        OnQuestCompleted?.Invoke(currentQuest);

        // 새로운 퀘스트 생성
        GenerateNewQuest();
    }

    public bool IsQuestTarget(GameObject hero)
    {
        if (currentQuest == null || currentQuest.IsComplete())
            return false;

        HeroType heroType = hero.GetComponent<HeroType>();
        HeroType questHeroType = currentQuest.targetPrefab.GetComponent<HeroType>();

        return heroType != null && questHeroType != null && heroType.heroID == questHeroType.heroID;
    }
}
