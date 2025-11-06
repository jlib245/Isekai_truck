using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleDriver : MonoBehaviour
{
    [Header("AI 주행 설정")]
    public float driveForce = 50f; // (MODIFIED) 전진하려는 '힘' (VelocityChange -> Force)
    public float maxDriveSpeed = 5f; // (NEW) AI의 최대 주행 속도
    public float laneCorrectionForce = 20f; // (MODIFIED) 원래 차선으로 복귀하려는 '힘'
    public float damping = 2f; // (NEW) 차선 복귀 시 좌우 흔들림을 잡는 제동력

    // (DELETED) 스턴(일시정지) 관련 변수 삭제
    // public float stunDuration = 1.0f;
    // private float stunTimer = 0f;
    
    private Rigidbody rb;
    private float targetX; // 이 차가 지켜야 할 X축 위치 (차선)
    private bool isHit = false; // (NEW) 플레이어에게 맞았는지 여부 (영구)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 스폰된 위치의 X좌표를 '내 차선'으로 기억합니다.
        targetX = transform.position.x;
    }

    void FixedUpdate()
    {
        // (MODIFIED) isHit이 true이면 AI 로직(주행, 차선복귀)을 *영구히* 실행하지 않음
        if (isHit)
        {
            return; 
        }

        // (DELETED) 스턴 타이머 로직 삭제
        // if (stunTimer > 0) ...

        // GameManager가 Playing 상태가 아니면 멈춥니다.
        if (GameManager.Instance.state != GameState.Playing)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        
        // --- AI 주행 로직 (Force 기반) ---

        // 1. 전진 (Z축)
        float zForce = 0;
        // 최대 속도보다 느릴 때만 전진 힘을 가함
        if (rb.velocity.z < maxDriveSpeed) 
        {
            zForce = driveForce;
        }

        // 2. 차선 복귀 (X축)
        // 현재 위치(rb.position.x)와 원래 차선(targetX)의 차이를 계산합니다.
        float xDiff = targetX - rb.position.x;
        // 차선으로 복귀하려는 힘(Force)과 현재 속도를 줄이는 제동(Damping)을 계산
        float xForce = (xDiff * laneCorrectionForce) - (rb.velocity.x * damping);

        // 3. 힘 적용 (MODIFIED -> ForceMode.Force)
        rb.AddForce(xForce, 0, zForce, ForceMode.Force);
    }

    // (MODIFIED) PlayerCollision이 이 함수를 호출하여 AI를 *영구히* 비활성화합니다.
    public void HitByPlayer()
    {
        isHit = true;
    }
}