using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    public Text questTitleText;
    public Text questDescriptionText;
    public Text questProgressText;
    public Text questRewardText;
    public GameObject questPanel;

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += UpdateQuestUI;
            QuestManager.Instance.OnQuestUpdated += UpdateQuestUI;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;

            // 현재 퀘스트가 있으면 표시
            Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null)
            {
                UpdateQuestUI(currentQuest);
            }
        }
        else
        {
            Debug.LogWarning("[QuestUI] QuestManager를 찾을 수 없습니다!");
        }
    }

    void UpdateQuestUI(Quest quest)
    {
        if (quest == null)
        {
            if (questPanel != null)
                questPanel.SetActive(false);
            return;
        }

        if (questPanel != null)
            questPanel.SetActive(true);

        if (questTitleText != null)
            questTitleText.text = "퀘스트";

        if (questDescriptionText != null)
            questDescriptionText.text = quest.GetQuestDescription();

        if (questProgressText != null)
            questProgressText.text = quest.GetProgressText();

        if (questRewardText != null)
            questRewardText.text = $"보상: {quest.rewardMoney}G";
    }

    void OnQuestCompleted(Quest quest)
    {
        Debug.Log("[QuestUI] 퀘스트 완료!");
        // 여기에 완료 애니메이션이나 효과 추가 가능
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= UpdateQuestUI;
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestUI;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }
}
