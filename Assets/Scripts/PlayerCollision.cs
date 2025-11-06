using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("충돌 설정")]
    public int obstacleDamage = 1; // 장애물 충돌 시 데미지
    
    [Header("충돌 물리 힘 (VelocityChange)")]
    // (Inspector에서 10~15 정도로 높이고 테스트하세요)
    public float targetHitForce = 15f; // (MODIFIED) 타겟(용사)을 위로 날리는 힘
    public float obstacleHitForce = 20f; // (MODIFIED) 장애물을 위로 날리는 힘
    
    // (DELETED) 측면 충돌 배율 삭제 (더 이상 필요 없음)
    // public float obstacleSideForceMultiplier = 2.0f; 

    // 카메라 쉐이크를 위해 카메라 참조
    private FollowCamera cam;

    void Start()
    {
        cam = Camera.main.GetComponent<FollowCamera>();
        if (cam == null)
        {
            Debug.LogWarning("PlayerCollision: Main Camera에 FollowCamera 스크립트가 없습니다.");
        }
    }

    // 물리 기반 충돌은 OnCollisionEnter에서 감지합니다.
    void OnCollisionEnter(Collision collision)
    {
        // ... (게임 상태 체크는 동일) ...
        if (GameManager.Instance.state != GameState.Playing)
        {
            return;
        }

        GameObject other = collision.gameObject;

        if (other.CompareTag("Target"))
        {
            HandleTargetHit(other);
        }
        else if (other.CompareTag("Obstacle"))
        {
            // (MODIFIED) 충돌 지점 정보(collision)가 더 이상 필요 없습니다.
            HandleObstacleHit(other);
        }
    }

    // 타겟(용사) 충돌 처리
    void HandleTargetHit(GameObject target)
    {
        // 1. 점수 획득
        GameManager.Instance.AddScore(50); 

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // (MODIFIED) "위로 + 약간 앞으로" 날려버릴 방향 벡터
            Vector3 forceDirection = (Vector3.up * 2f + transform.forward * 0.5f).normalized;
            
            // 질량을 무시하고 즉각적인 속도 변화
            rb.AddForce(forceDirection * targetHitForce, ForceMode.VelocityChange);
        }
        
        // 3. 타겟은 10초 뒤에 파괴
        Destroy(target, 10f); 
    }

    // 장애물(차량) 충돌 처리
    void HandleObstacleHit(GameObject obstacle) // (MODIFIED) 'Collision collision' 파라미터 제거
    {
        // 1. 카메라 쉐이크
        if (cam != null)
        {
            cam.ShakeCamera();
        }
        
        // 2. 데미지
        GameManager.Instance.TakeDamage(obstacleDamage);

        // 3. 장애물의 Rigidbody에 '파괴적인' 힘을 가해 날려버립니다.
        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // (MODIFIED) "위로 + 약간 앞으로" 날려버릴 방향 벡터
            Vector3 forceDirection = (Vector3.up * 2f + transform.forward * 0.5f).normalized;
            
            // (MODIFIED) ForceMode.VelocityChange로 강력한 힘을 가함
            rb.AddForce(forceDirection * obstacleHitForce, ForceMode.VelocityChange);

            // (DELETED) 측면 충돌 확인 및 추가 힘 로직 삭제
        }

        // 4. 장애물 AI 스턴 호출 (복원!)
        ObstacleDriver driver = obstacle.GetComponent<ObstacleDriver>();
        if (driver != null)
        {
            driver.HitByPlayer();
        }

        // 5. 장애물도 10초 뒤 파괴
        Destroy(obstacle, 10f);
    }
}