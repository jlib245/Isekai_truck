using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCollision : MonoBehaviour
{
    [Header("Collsion Settings")]
    public int obstacleDamage = 1;
    public int obstaclePenalty = 100;
    public int targetReward = 500;
    public float destroyDelay = 10f;

    [Header("Wrong Target Penalty")]
    [Tooltip("퀘스트 대상이 아닌 용사를 치면 받는 골드 페널티")]
    public int wrongTargetPenalty = 50;

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
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>(); // 이미 충돌한 오브젝트 추적

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

        // 이미 충돌한 오브젝트면 무시 (중복 충돌 방지)
        if (hitObjects.Contains(other))
        {
            return;
        }

        if (other.CompareTag("Target"))
        {
            hitObjects.Add(other); // 충돌 목록에 추가
            HandleTargetHit(other);
        }
        else if (other.CompareTag("Obstacle"))
        {
            hitObjects.Add(other); // 충돌 목록에 추가
            HandleObstacleHit(other);
        }
    }

    void HandleTargetHit(GameObject target)
    {
        // 황금 용사 체크
        HeroType heroType = target.GetComponent<HeroType>();
        bool isGolden = heroType != null && heroType.isGolden;
        int bonusMultiplier = isGolden ? heroType.goldenBonusMultiplier : 1;

        // 퀘스트 체크 및 보상
        bool isQuestTarget = false;
        bool isOriginalQuestTarget = false; // 원래 퀘스트 대상인지
        if (QuestManager.Instance != null)
        {
            isOriginalQuestTarget = QuestManager.Instance.IsQuestTarget(target);

            // 황금 용사면 무조건 퀘스트 진행
            if (isGolden)
            {
                isQuestTarget = true;
                QuestManager.Instance.ForceAddQuestProgress();
            }
            else
            {
                isQuestTarget = QuestManager.Instance.CheckAndUpdateQuest(target);
            }
        }

        // 퀘스트 대상이 아니면 페널티 (황금 용사는 제외)
        if (!isQuestTarget && !isGolden)
        {
            Debug.Log("[PlayerCollision] 퀘스트 대상이 아닌 용사 - 페널티 적용!");
            GameManager.Instance.LoseMoney(wrongTargetPenalty);
            GameManager.Instance.wrongHeroCount++;

            // 페널티 텍스트 표시
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowPenaltyText(wrongTargetPenalty, target.transform.position + Vector3.up * 2f);
            }
        }
        else
        {
            GameManager.Instance.correctHeroCount++;

            // 황금 용사 + 원래 퀘스트 대상일 때만 보너스 골드 지급
            if (isGolden && isOriginalQuestTarget)
            {
                int goldenBonus = targetReward * bonusMultiplier;
                GameManager.Instance.AddMoney(goldenBonus);
                Debug.Log($"[PlayerCollision] 황금 용사 + 퀘스트 대상! 보너스 {goldenBonus}G 획득!");

                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.ShowRewardText(goldenBonus, target.transform.position + Vector3.up * 2.5f);
                }
            }
        }

        // 용사 충돌 사운드 재생 (가벼운 소리)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHeroHitSound();
        }

        // 보상 이펙트 (모든 용사에게 적용)
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayRewardEffect(target.transform.position);
        }

        // 퀘스트 대상일 경우 추가 효과
        if (isQuestTarget)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRewardSound();

            if (FloatingTextManager.Instance != null)
                FloatingTextManager.Instance.ShowFloatingText("용사 획득!", target.transform.position + Vector3.up * 2f, Color.cyan);
        }

        // 충돌 직후 즉시 물리적 충돌 제거 (속도 저하 방지)
        Collider playerCollider = GetComponent<Collider>();

        // 타겟의 모든 Collider 찾아서 충돌 무시 (자식 오브젝트 포함)
        Collider[] targetColliders = target.GetComponentsInChildren<Collider>();
        if (playerCollider != null)
        {
            foreach (Collider col in targetColliders)
            {
                if (col != null)
                {
                    Physics.IgnoreCollision(playerCollider, col, true);
                }
            }
        }

        // 용사의 움직임 스크립트 비활성화 (있다면)
        MonoBehaviour[] scripts = target.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && !(script is HeroType))
            {
                script.enabled = false;
            }
        }

        // Animator 비활성화 (Root Motion이 위치를 조작할 수 있음)
        Animator animator = target.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 더 강한 힘으로 밀어내기 (끼임 완전 방지)
        ApplyHitForce(target, targetHitForce * 1.5f);

        // 추가로 즉시 위치를 조금 밀어내기
        Vector3 pushDirection = (target.transform.position - transform.position).normalized;
        target.transform.position += pushDirection * 0.5f;

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

        // 충돌 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollisionSound();
        }

        // 충돌 VFX 재생
        if (VFXManager.Instance != null)
        {
            Vector3 contactPoint = obstacle.transform.position;
            Vector3 normal = (transform.position - obstacle.transform.position).normalized;
            VFXManager.Instance.PlayCollisionEffect(contactPoint, normal);
        }

        // 골드는 무적 시간과 상관없이 항상 차감
        GameManager.Instance.LoseMoney(obstaclePenalty);
        GameManager.Instance.obstacleHitCount++;

        // 페널티 텍스트 표시
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowPenaltyText(obstaclePenalty, transform.position + Vector3.up * 2f);
        }

        // 무적시간이 아닐 때만 데미지 적용
        if (invincibilityTimer <= 0f)
        {
            GameManager.Instance.TakeDamage(obstacleDamage);
            invincibilityTimer = invincibilityDuration;
            StartCoroutine(BlinkEffect());

            // 데미지 텍스트 표시
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowDamageText(obstacleDamage, transform.position + Vector3.up * 3f);
            }
        }

        // 충돌 직후 즉시 물리적 충돌 제거 (속도 저하 방지)
        Collider obstacleCollider = obstacle.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (obstacleCollider != null && playerCollider != null)
        {
            // 플레이어와 장애물 간의 충돌을 완전히 무시
            Physics.IgnoreCollision(playerCollider, obstacleCollider, true);
        }

        // 힘을 가한 후 즉시 충돌 무시
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
            // 위로 + 앞으로 밀어내기
            Vector3 forceDirection = (Vector3.up * hitForceDirection.y + transform.forward * hitForceDirection.z).normalized;
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