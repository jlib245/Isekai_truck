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

    [Header("Hero Preview")]
    [Tooltip("프리뷰 용사의 로컬 위치 (카메라 기준)")]
    public Vector3 previewLocalPosition = new Vector3(-1.5f, 0.5f, 3f);
    [Tooltip("프리뷰 용사의 스케일")]
    public float previewScale = 100f;
    [Tooltip("프리뷰 용사의 회전 속도")]
    public float rotationSpeed = 30f;

    private GameObject currentPreviewHero;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

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

    void Update()
    {
        // 프리뷰 용사 회전만 (위치는 카메라 자식이라 자동으로 따라감)
        if (currentPreviewHero != null)
        {
            currentPreviewHero.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    void UpdateQuestUI(Quest quest)
    {
        if (quest == null)
        {
            if (questPanel != null)
                questPanel.SetActive(false);
            DestroyPreviewHero();
            return;
        }

        if (questPanel != null)
            questPanel.SetActive(true);

        if (questTitleText != null)
            questTitleText.text = "Quest";

        if (questDescriptionText != null)
            questDescriptionText.text = quest.GetQuestDescription();

        if (questProgressText != null)
            questProgressText.text = quest.GetProgressText();

        if (questRewardText != null)
            questRewardText.text = $"Reward: {quest.rewardMoney}G";

        // 프리뷰 용사 생성/업데이트
        SpawnPreviewHero(quest.targetPrefab);
    }

    void SpawnPreviewHero(GameObject heroPrefab)
    {
        if (heroPrefab == null) return;

        // 기존 프리뷰 제거
        DestroyPreviewHero();

        // 새 프리뷰 생성 - 카메라의 자식으로 설정하여 흔들림 방지
        currentPreviewHero = Instantiate(heroPrefab);
        currentPreviewHero.name = "QuestPreviewHero";

        // 불필요한 컴포넌트 제거
        DisableGameplayComponents(currentPreviewHero);

        // 카메라의 자식으로 설정 (카메라와 함께 움직임)
        if (mainCamera != null)
        {
            currentPreviewHero.transform.SetParent(mainCamera.transform);
        }

        // 스케일 설정
        currentPreviewHero.transform.localScale = Vector3.one * previewScale;

        // 로컬 위치 설정 (카메라 기준 상대 위치)
        SetPreviewLocalPosition();

        Debug.Log($"[QuestUI] 프리뷰 용사 생성: {heroPrefab.name}");
    }

    void SetPreviewLocalPosition()
    {
        if (currentPreviewHero == null) return;

        currentPreviewHero.transform.localPosition = previewLocalPosition;
    }

    void DisableGameplayComponents(GameObject hero)
    {
        // Rigidbody 제거 또는 비활성화
        Rigidbody rb = hero.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Collider 비활성화
        foreach (Collider col in hero.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // 기타 게임플레이 스크립트 비활성화
        foreach (MonoBehaviour script in hero.GetComponents<MonoBehaviour>())
        {
            if (script != this && !(script is Animator))
            {
                script.enabled = false;
            }
        }

        // Animator는 유지하고 랜덤 애니메이션 재생
        Animator animator = hero.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;

            // 랜덤 애니메이션 재생
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            if (animator.runtimeAnimatorController != null)
            {
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    AnimationClip randomClip = clips[Random.Range(0, clips.Length)];
                    animator.Play(randomClip.name, 0, Random.Range(0f, 1f));
                }
            }
        }
    }

    void DestroyPreviewHero()
    {
        if (currentPreviewHero != null)
        {
            Destroy(currentPreviewHero);
            currentPreviewHero = null;
        }
    }

    void OnQuestCompleted(Quest quest)
    {
        Debug.Log("[QuestUI] 퀘스트 완료!");

        // 프리뷰 히어로 위치에서 축하 파티클 이펙트 재생 (카메라 자식으로)
        if (currentPreviewHero != null)
        {
            PlayCelebrationEffect();
        }
    }

    [Header("Celebration Effect")]
    [Tooltip("축하 파티클 프리팹")]
    public GameObject celebrationParticlePrefab;
    [Tooltip("축하 파티클 크기")]
    public float celebrationScale = 1f;

    void PlayCelebrationEffect()
    {
        if (celebrationParticlePrefab == null)
        {
            Debug.LogWarning("[QuestUI] 축하 파티클 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 프리뷰 히어로 위치에 파티클 생성 (카메라 자식으로)
        GameObject particle = Instantiate(celebrationParticlePrefab, currentPreviewHero.transform.position, Quaternion.identity);

        if (mainCamera != null)
        {
            particle.transform.SetParent(mainCamera.transform);
            particle.transform.localPosition = previewLocalPosition;
            particle.transform.localScale = Vector3.one * celebrationScale;
        }

        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(particle, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(particle, 3f);
        }
    }

    void OnDestroy()
    {
        DestroyPreviewHero();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= UpdateQuestUI;
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestUI;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }
}
