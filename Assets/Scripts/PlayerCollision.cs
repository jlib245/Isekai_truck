using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [Header("Collsion Settings")]
    public int obstacleDamage = 1;
    public int obstaclePenalty = 100;
    public int targetReward = 500;
    public float destroyDelay = 10f;

    [Header("Collision Physics")]
    public float targetHitForce = 15f;
    public float obstacleHitForce = 20f;
    public Vector3 hitForceDirection = new Vector3(0f, 2f, 0.5f);

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.1f;
    public Transform truckModel;

    private FollowCamera cam;
    private float invincibilityTimer = 0f;
    private Renderer[] truckRenderers;

    void Start()
    {
        cam = Camera.main.GetComponent<FollowCamera>();
        if (cam == null)
        {
            Debug.LogWarning("PlayerCollision: Main Camera에 FollowCamera 스크립트가 없습니다.");
        }

        // 트럭 모델이 지정되지 않았으면 자동으로 찾기
        if (truckModel == null)
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null)
                truckModel = controller.truckModel;
        }

        // 트럭 모델의 모든 렌더러 찾기
        if (truckModel != null)
            truckRenderers = truckModel.GetComponentsInChildren<Renderer>();

        // 업그레이드된 무적 시간 적용
        ApplyUpgrades();
    }

    void ApplyUpgrades()
    {
        if (UpgradeManager.Instance != null)
        {
            invincibilityDuration = UpgradeManager.Instance.GetInvincibilityDuration();
            Debug.Log($"[PlayerCollision] 업그레이드 적용 - 무적 시간: {invincibilityDuration}초");
        }
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance.state != GameState.Playing)
            return;

        GameObject other = collision.gameObject;

        if (other.CompareTag("Target"))
        {
            HandleTargetHit(other);
        }
        else if (other.CompareTag("Obstacle"))
        {
            HandleObstacleHit(other);
        }
    }

    void HandleTargetHit(GameObject target)
    {
        // 퀘스트 체크 및 보상
        bool isQuestTarget = false;
        if (QuestManager.Instance != null)
        {
            isQuestTarget = QuestManager.Instance.CheckAndUpdateQuest(target);
        }

        // 퀘스트 대상이 아니면 기본 보상금 없음
        if (!isQuestTarget)
        {
            Debug.Log("[PlayerCollision] 퀘스트 대상이 아닌 용사 - 보상금 없음");
        }

        ApplyHitForce(target, targetHitForce);

        // 용사를 3초 후에 GameManager에 저장 (날아가는 모습 보여주기)
        StartCoroutine(CollectHeroAfterDelay(target, 3f));
    }

    IEnumerator CollectHeroAfterDelay(GameObject hero, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hero != null && GameManager.Instance != null)
        {
            GameManager.Instance.AddCollectedHero(hero);
            Debug.Log($"[PlayerCollision] 용사 저장 완료! 총 {GameManager.Instance.GetCollectedHeroes().Count}명");
        }
    }

    void HandleObstacleHit(GameObject obstacle)
    {
        if (cam != null)
            cam.ShakeCamera();

        // 골드는 무적 시간과 상관없이 항상 차감
        GameManager.Instance.LoseMoney(obstaclePenalty);

        // 무적시간이 아닐 때만 데미지 적용
        if (invincibilityTimer <= 0f)
        {
            GameManager.Instance.TakeDamage(obstacleDamage);
            invincibilityTimer = invincibilityDuration;
            StartCoroutine(BlinkEffect());
        }

        ApplyHitForce(obstacle, obstacleHitForce);

        ObstacleDriver driver = obstacle.GetComponent<ObstacleDriver>();
        if (driver != null)
            driver.HitByPlayer();

        // 장애물도 3초 후에 GameManager에 저장 (날아가는 모습 보여주기)
        StartCoroutine(CollectObstacleAfterDelay(obstacle, 3f));
    }

    IEnumerator CollectObstacleAfterDelay(GameObject obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obstacle != null && GameManager.Instance != null)
        {
            GameManager.Instance.AddCollectedObstacle(obstacle);
            Debug.Log($"[PlayerCollision] 장애물 저장 완료! 총 {GameManager.Instance.GetCollectedObstacles().Count}개");
        }
    }

    void ApplyHitForce(GameObject target, float force)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = (hitForceDirection + transform.forward * hitForceDirection.z).normalized;
            rb.AddForce(forceDirection * force, ForceMode.VelocityChange);
        }
    }

    IEnumerator BlinkEffect()
    {
        Debug.Log("[BlinkEffect] 깜빡임 효과 시작");

        if (truckRenderers == null || truckRenderers.Length == 0)
        {
            Debug.LogWarning("[BlinkEffect] truckRenderers가 null이거나 비어있습니다!");
            yield break;
        }

        Debug.Log($"[BlinkEffect] 렌더러 개수: {truckRenderers.Length}, 무적 시간: {invincibilityDuration}초");

        float elapsed = 0f;
        bool isVisible = true;
        int blinkCount = 0;

        while (elapsed < invincibilityDuration)
        {
            isVisible = !isVisible;
            blinkCount++;

            Debug.Log($"[BlinkEffect] 깜빡임 #{blinkCount} - Visible: {isVisible}, 경과 시간: {elapsed:F2}초");

            // 모든 렌더러 On/Off
            foreach (Renderer renderer in truckRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = isVisible;
                    Debug.Log($"[BlinkEffect] Renderer '{renderer.name}' enabled: {isVisible}");
                }
            }

            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        Debug.Log("[BlinkEffect] 깜빡임 효과 종료 - 모든 렌더러 활성화");

        // 깜빡임 종료 후 모든 렌더러 활성화
        foreach (Renderer renderer in truckRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
                Debug.Log($"[BlinkEffect] Renderer '{renderer.name}' 최종 활성화");
            }
        }

        Debug.Log("[BlinkEffect] 깜빡임 효과 완전 종료");
    }
}