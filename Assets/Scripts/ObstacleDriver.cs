using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleDriver : MonoBehaviour
{
    [Header("AI 주행 설정")]
    public float driveForce = 50f;
    public float maxDriveSpeed = 5f;
    public float laneCorrectionForce = 20f;
    public float damping = 2f;
    public bool moveTowardsPlayer = true; // true: 플레이어 방향으로 (뒤로), false: 앞으로

    [Header("차선 변경 설정")]
    public bool enableLaneChange = true;
    public float laneChangeInterval = 3f;
    public float laneDistance = 3f;
    public int minLane = -2;
    public int maxLane = 2;

    [Header("충돌 회피 설정")]
    public bool enableAvoidance = true;
    public float frontRayDistance = 5f;
    public float sideRayDistance = 2f;

    private Rigidbody rb;
    private float targetX;
    private bool isHit = false;
    private int currentLane;
    private float laneChangeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetX = transform.position.x;
        currentLane = Mathf.RoundToInt(transform.position.x / laneDistance);
        laneChangeTimer = laneChangeInterval + Random.Range(-1f, 1f);
    }

    void FixedUpdate()
    {
        if (isHit) return;

        if (GameManager.Instance.state != GameState.Playing)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // 충돌 회피 체크
        bool shouldStop = false;
        if (enableAvoidance)
        {
            shouldStop = CheckForObstacles();
        }

        // 차선 변경 타이머
        if (enableLaneChange)
        {
            laneChangeTimer -= Time.fixedDeltaTime;
            if (laneChangeTimer <= 0f)
            {
                TryRandomLaneChange();
                laneChangeTimer = laneChangeInterval + Random.Range(-1f, 1f);
            }
        }

        // 이동 방향 결정
        float direction = moveTowardsPlayer ? -1f : 1f;
        float currentSpeed = rb.velocity.z * direction; // 현재 속도 (방향 고려)

        // 이동 힘 (앞에 장애물 있으면 멈춤)
        float zForce = 0f;
        if (!shouldStop && currentSpeed < maxDriveSpeed)
        {
            zForce = driveForce * direction;
        }

        // 차선 보정 힘
        float xDiff = targetX - rb.position.x;
        float xForce = (xDiff * laneCorrectionForce) - (rb.velocity.x * damping);

        rb.AddForce(xForce, 0, zForce, ForceMode.Force);
    }

    bool CheckForObstacles()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = moveTowardsPlayer ? Vector3.back : Vector3.forward;

        // 이동 방향에 Target이나 다른 장애물 있는지 확인
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, frontRayDistance))
        {
            if (hit.collider.CompareTag("Target") || hit.collider.CompareTag("Obstacle"))
            {
                // 차선 변경 시도
                if (enableLaneChange && TryChangeLane())
                {
                    return false; // 차선 변경 성공하면 계속 이동
                }
                return true; // 차선 변경 실패하면 정지
            }
        }

        return false;
    }

    void TryRandomLaneChange()
    {
        // 25% 확률로 차선 변경
        if (Random.value < 0.25f)
        {
            int direction = Random.Range(0, 2) == 0 ? -1 : 1;
            TryChangeLaneDirection(direction);
        }
    }

    bool TryChangeLane()
    {
        // 왼쪽 또는 오른쪽으로 차선 변경 시도
        int[] directions = { -1, 1 };

        // 랜덤 순서
        if (Random.value > 0.5f)
        {
            directions = new int[] { 1, -1 };
        }

        foreach (int dir in directions)
        {
            if (TryChangeLaneDirection(dir))
            {
                return true;
            }
        }

        return false;
    }

    bool TryChangeLaneDirection(int direction)
    {
        int newLane = currentLane + direction;

        // 범위 체크
        if (newLane < minLane || newLane > maxLane)
            return false;

        // 옆 차선에 뭐가 있는지 확인
        Vector3 sideDirection = direction > 0 ? Vector3.right : Vector3.left;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (!Physics.Raycast(rayOrigin, sideDirection, sideRayDistance))
        {
            // 비어있으면 차선 변경
            currentLane = newLane;
            targetX = currentLane * laneDistance;
            return true;
        }

        return false;
    }

    public void HitByPlayer()
    {
        isHit = true;
    }

    // 디버그 시각화
    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 moveDir = moveTowardsPlayer ? Vector3.back : Vector3.forward;

        // 이동 방향 Ray (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, moveDir * frontRayDistance);

        // 좌우 Ray (파랑)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin, Vector3.right * sideRayDistance);
        Gizmos.DrawRay(origin, Vector3.left * sideRayDistance);
    }
}