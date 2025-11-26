using UnityEngine;

[System.Serializable]
public class Quest
{
    public GameObject targetPrefab;  // 찾아야 할 용사 프리팹
    public int requiredCount = 1;    // 필요한 수량
    public int currentCount = 0;     // 현재 수집한 수량
    public int rewardMoney = 1000;   // 보상금

    public Quest(GameObject prefab, int count, int reward)
    {
        targetPrefab = prefab;
        requiredCount = count;
        currentCount = 0;
        rewardMoney = reward;
    }

    public bool IsComplete()
    {
        return currentCount >= requiredCount;
    }

    public void AddProgress()
    {
        if (!IsComplete())
            currentCount++;
    }

    public string GetQuestDescription()
    {
        HeroType heroType = targetPrefab.GetComponent<HeroType>();
        string heroName = heroType != null ? heroType.heroName : "용사";
        return $"{heroName} {requiredCount}명 이세계로 보내기";
    }

    public string GetProgressText()
    {
        return $"({currentCount}/{requiredCount})";
    }
}
