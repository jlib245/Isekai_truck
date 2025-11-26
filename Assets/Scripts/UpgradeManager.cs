using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Levels")]
    public int hpLevel = 0;
    public int invincibilityLevel = 0;

    [Header("Upgrade Settings")]
    public int baseHPCost = 500;
    public int baseInvincibilityCost = 300;
    public float costMultiplier = 1.5f;  // 레벨당 가격 증가 배율

    [Header("HP Upgrade")]
    public int baseMaxHP = 3;
    public int hpPerLevel = 1;

    [Header("Invincibility Upgrade")]
    public float baseInvincibilityDuration = 1.5f;
    public float invincibilityPerLevel = 0.3f;

    private const string HP_LEVEL_KEY = "HPLevel";
    private const string INVINCIBILITY_LEVEL_KEY = "InvincibilityLevel";

    public delegate void UpgradeHandler();
    public event UpgradeHandler OnUpgradeChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadUpgrades()
    {
        hpLevel = PlayerPrefs.GetInt(HP_LEVEL_KEY, 0);
        invincibilityLevel = PlayerPrefs.GetInt(INVINCIBILITY_LEVEL_KEY, 0);
        Debug.Log($"[UpgradeManager] 업그레이드 로드 - HP Lv.{hpLevel}, 무적 Lv.{invincibilityLevel}");
    }

    void SaveUpgrades()
    {
        PlayerPrefs.SetInt(HP_LEVEL_KEY, hpLevel);
        PlayerPrefs.SetInt(INVINCIBILITY_LEVEL_KEY, invincibilityLevel);
        PlayerPrefs.Save();
        Debug.Log($"[UpgradeManager] 업그레이드 저장 - HP Lv.{hpLevel}, 무적 Lv.{invincibilityLevel}");
    }

    // HP 업그레이드 비용
    public int GetHPUpgradeCost()
    {
        return Mathf.RoundToInt(baseHPCost * Mathf.Pow(costMultiplier, hpLevel));
    }

    // 무적 시간 업그레이드 비용
    public int GetInvincibilityUpgradeCost()
    {
        return Mathf.RoundToInt(baseInvincibilityCost * Mathf.Pow(costMultiplier, invincibilityLevel));
    }

    // 현재 최대 HP
    public int GetMaxHP()
    {
        return baseMaxHP + (hpLevel * hpPerLevel);
    }

    // 현재 무적 시간
    public float GetInvincibilityDuration()
    {
        return baseInvincibilityDuration + (invincibilityLevel * invincibilityPerLevel);
    }

    // HP 업그레이드 구매
    public bool PurchaseHPUpgrade()
    {
        int cost = GetHPUpgradeCost();

        if (GameManager.Instance != null && GameManager.Instance.totalMoney >= cost)
        {
            GameManager.Instance.totalMoney -= cost;
            GameManager.Instance.SaveTotalMoney();

            hpLevel++;
            SaveUpgrades();

            Debug.Log($"[UpgradeManager] HP 업그레이드 구매! Lv.{hpLevel}, 비용: {cost}G");
            OnUpgradeChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("[UpgradeManager] 돈이 부족합니다!");
            return false;
        }
    }

    // 무적 시간 업그레이드 구매
    public bool PurchaseInvincibilityUpgrade()
    {
        int cost = GetInvincibilityUpgradeCost();

        if (GameManager.Instance != null && GameManager.Instance.totalMoney >= cost)
        {
            GameManager.Instance.totalMoney -= cost;
            GameManager.Instance.SaveTotalMoney();

            invincibilityLevel++;
            SaveUpgrades();

            Debug.Log($"[UpgradeManager] 무적 시간 업그레이드 구매! Lv.{invincibilityLevel}, 비용: {cost}G");
            OnUpgradeChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("[UpgradeManager] 돈이 부족합니다!");
            return false;
        }
    }
}
