using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Money Display")]
    public Text totalMoneyText;

    [Header("HP Upgrade")]
    public Text hpLevelText;
    public Text hpEffectText;
    public Text hpCostText;
    public Button hpUpgradeButton;

    [Header("Invincibility Upgrade")]
    public Text invincibilityLevelText;
    public Text invincibilityEffectText;
    public Text invincibilityCostText;
    public Button invincibilityUpgradeButton;

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (hpUpgradeButton != null)
            hpUpgradeButton.onClick.AddListener(OnHPUpgradeClicked);

        if (invincibilityUpgradeButton != null)
            invincibilityUpgradeButton.onClick.AddListener(OnInvincibilityUpgradeClicked);

        // 업그레이드 변경 시 UI 갱신
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChanged += UpdateUI;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null || UpgradeManager.Instance == null)
        {
            Debug.LogWarning("[ShopUI] GameManager 또는 UpgradeManager가 없습니다!");
            return;
        }

        // 총 금액 표시
        if (totalMoneyText != null)
            totalMoneyText.text = "보유 골드: " + GameManager.Instance.totalMoney + "G";

        // HP 업그레이드 정보
        UpdateHPUpgradeUI();

        // 무적 시간 업그레이드 정보
        UpdateInvincibilityUpgradeUI();
    }

    void UpdateHPUpgradeUI()
    {
        int currentLevel = UpgradeManager.Instance.hpLevel;
        int currentHP = UpgradeManager.Instance.GetMaxHP();
        int nextHP = currentHP + UpgradeManager.Instance.hpPerLevel;
        int cost = UpgradeManager.Instance.GetHPUpgradeCost();

        if (hpLevelText != null)
            hpLevelText.text = $"Lv.{currentLevel}";

        if (hpEffectText != null)
            hpEffectText.text = $"최대 HP: {currentHP} → {nextHP}";

        if (hpCostText != null)
            hpCostText.text = $"{cost}G";

        // 구매 가능 여부에 따라 버튼 활성화
        if (hpUpgradeButton != null)
        {
            bool canAfford = GameManager.Instance.totalMoney >= cost;
            hpUpgradeButton.interactable = canAfford;
        }
    }

    void UpdateInvincibilityUpgradeUI()
    {
        int currentLevel = UpgradeManager.Instance.invincibilityLevel;
        float currentDuration = UpgradeManager.Instance.GetInvincibilityDuration();
        float nextDuration = currentDuration + UpgradeManager.Instance.invincibilityPerLevel;
        int cost = UpgradeManager.Instance.GetInvincibilityUpgradeCost();

        if (invincibilityLevelText != null)
            invincibilityLevelText.text = $"Lv.{currentLevel}";

        if (invincibilityEffectText != null)
            invincibilityEffectText.text = $"무적 시간: {currentDuration:F1}초 → {nextDuration:F1}초";

        if (invincibilityCostText != null)
            invincibilityCostText.text = $"{cost}G";

        // 구매 가능 여부에 따라 버튼 활성화
        if (invincibilityUpgradeButton != null)
        {
            bool canAfford = GameManager.Instance.totalMoney >= cost;
            invincibilityUpgradeButton.interactable = canAfford;
        }
    }

    void OnHPUpgradeClicked()
    {
        if (UpgradeManager.Instance != null)
        {
            bool success = UpgradeManager.Instance.PurchaseHPUpgrade();
            if (success)
            {
                Debug.Log("[ShopUI] HP 업그레이드 구매 성공!");
                UpdateUI();
            }
            else
            {
                Debug.Log("[ShopUI] HP 업그레이드 구매 실패!");
            }
        }
    }

    void OnInvincibilityUpgradeClicked()
    {
        if (UpgradeManager.Instance != null)
        {
            bool success = UpgradeManager.Instance.PurchaseInvincibilityUpgrade();
            if (success)
            {
                Debug.Log("[ShopUI] 무적 시간 업그레이드 구매 성공!");
                UpdateUI();
            }
            else
            {
                Debug.Log("[ShopUI] 무적 시간 업그레이드 구매 실패!");
            }
        }
    }

    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChanged -= UpdateUI;
        }
    }
}
