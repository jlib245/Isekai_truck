using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 3D 공간에 떠다니는 텍스트를 생성하고 관리하는 매니저
/// 점수 획득, 데미지 등을 시각화
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Prefab")]
    [Tooltip("떠다니는 텍스트 프리팹 (Canvas > Text)")]
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatDistance = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float lifetime = 1.5f;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> textPool;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void InitializePool()
    {
        textPool = new Queue<GameObject>();

        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("[FloatingTextManager] floatingTextPrefab이 설정되지 않았습니다!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(floatingTextPrefab, transform);
            obj.SetActive(false);
            textPool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 3D 월드 좌표에 떠다니는 텍스트 생성 (플레이어 따라가기)
    /// </summary>
    public void ShowFloatingText(string text, Vector3 worldPosition, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("[FloatingTextManager] floatingTextPrefab이 없어서 텍스트를 표시할 수 없습니다!");
            return;
        }

        GameObject textObj = GetPooledText();
        if (textObj == null) return;

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 플레이어의 자식으로 만들어서 따라가게 함
        if (player != null)
        {
            textObj.transform.SetParent(player.transform);
            textObj.transform.position = worldPosition;
        }
        else
        {
            textObj.transform.position = worldPosition;
        }

        textObj.SetActive(true);

        Text textComponent = textObj.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
        }

        StartCoroutine(AnimateFloatingText(textObj, player));
    }

    /// <summary>
    /// 보상 획득 텍스트 표시
    /// </summary>
    public void ShowRewardText(int amount, Vector3 worldPosition)
    {
        ShowFloatingText($"+{amount}G", worldPosition, Color.yellow);
    }

    /// <summary>
    /// 데미지 텍스트 표시
    /// </summary>
    public void ShowDamageText(int damage, Vector3 worldPosition)
    {
        ShowFloatingText($"-{damage} HP", worldPosition, Color.red);
    }

    /// <summary>
    /// 페널티 텍스트 표시
    /// </summary>
    public void ShowPenaltyText(int penalty, Vector3 worldPosition)
    {
        ShowFloatingText($"-{penalty}G", worldPosition, new Color(1f, 0.5f, 0f)); // 오렌지색
    }

    private GameObject GetPooledText()
    {
        if (textPool.Count > 0)
        {
            GameObject obj = textPool.Dequeue();
            return obj;
        }

        // 풀이 비었으면 새로 생성
        if (floatingTextPrefab != null)
        {
            return Instantiate(floatingTextPrefab, transform);
        }

        return null;
    }

    private IEnumerator AnimateFloatingText(GameObject textObj, GameObject player)
    {
        Vector3 startLocalPosition = textObj.transform.localPosition;
        Vector3 endLocalPosition = startLocalPosition + Vector3.up * floatDistance;

        Text textComponent = textObj.GetComponentInChildren<Text>();
        CanvasGroup canvasGroup = textObj.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = textObj.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        canvasGroup.alpha = 1f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // 로컬 좌표로 위로 떠오르기 (플레이어와 함께 이동)
            textObj.transform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);

            // 페이드 아웃 (후반부에만)
            if (t > 0.5f)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            }

            // 카메라를 향하도록 회전
            if (mainCamera != null)
            {
                textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - mainCamera.transform.position);
            }

            yield return null;
        }

        // 오브젝트 비활성화 및 풀로 반환
        textObj.transform.SetParent(transform); // 부모를 다시 FloatingTextManager로
        textObj.SetActive(false);
        textPool.Enqueue(textObj);
    }
}
