using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Text moneyText;
    public Text hpText;

    void Start()
    {
        Debug.Log("[UIManager] Start() 호출됨");
        StartCoroutine(WaitForGameManager());
    }

    IEnumerator WaitForGameManager()
    {
        Debug.Log("[UIManager] WaitForGameManager 시작");

        // GameManager가 준비될 때까지 대기
        while (GameManager.Instance == null)
        {
            Debug.Log("[UIManager] GameManager 대기 중...");
            yield return null;
        }

        Debug.Log("[UIManager] GameManager 발견, 이벤트 구독 시작");

        // 이벤트 구독
        GameManager.Instance.OnMoneyChanged += UpdateMoney;
        GameManager.Instance.OnHPChanged += UpdateHP;

        Debug.Log("[UIManager] 이벤트 구독 완료, UI 초기화 시작");

        // UI 초기화 (GameManager의 현재 값 반영)
        InitializeUI();
    }

    void InitializeUI()
    {
        Debug.Log($"[UIManager] InitializeUI 호출 - moneyText null? {moneyText == null}, hpText null? {hpText == null}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("[UIManager] GameManager.Instance가 null입니다!");
            return;
        }

        // GameManager의 현재 값으로 UI 업데이트
        if (moneyText != null)
        {
            moneyText.text = $"보상금: {GameManager.Instance.money}G";
            Debug.Log($"[UIManager] moneyText 업데이트: {moneyText.text}");
        }
        else
        {
            Debug.LogWarning("[UIManager] moneyText가 null입니다!");
        }

        if (hpText != null)
        {
            hpText.text = $"HP: {GameManager.Instance.playerHP} / {GameManager.Instance.maxHP}";
            Debug.Log($"[UIManager] hpText 업데이트: {hpText.text}");
        }
        else
        {
            Debug.LogWarning("[UIManager] hpText가 null입니다!");
        }

        Debug.Log($"[UIManager] UI 초기화 완료 - Money: {GameManager.Instance.money}G, HP: {GameManager.Instance.playerHP}/{GameManager.Instance.maxHP}");
    }

    void UpdateMoney(int newMoney)
    {
        Debug.Log($"[UIManager] UpdateMoney 호출 - newMoney: {newMoney}");
        if (moneyText != null)
        {
            moneyText.text = "보상금: " + newMoney + "G";
            Debug.Log($"[UIManager] moneyText 업데이트됨: {moneyText.text}");
        }
        else
        {
            Debug.LogWarning("[UIManager] UpdateMoney - moneyText가 null입니다!");
        }
    }

    void UpdateHP(int newHP, int maxHP)
    {
        Debug.Log($"[UIManager] UpdateHP 호출 - newHP: {newHP}, maxHP: {maxHP}");
        if (hpText != null)
        {
            hpText.text = $"HP: {newHP} / {maxHP}";
            Debug.Log($"[UIManager] hpText 업데이트됨: {hpText.text}");
        }
        else
        {
            Debug.LogWarning("[UIManager] UpdateHP - hpText가 null입니다!");
        }
    }

    void OnDestroy()
    {
        Debug.Log("[UIManager] OnDestroy 호출됨");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoney;
            GameManager.Instance.OnHPChanged -= UpdateHP;
            Debug.Log("[UIManager] 이벤트 구독 해제 완료");
        }
    }
}