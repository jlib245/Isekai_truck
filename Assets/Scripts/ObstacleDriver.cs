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

    [Header("속도 랜덤화")]
    public bool randomizeSpeed = true;
    public float minSpeedMultiplier = 0.7f;
    public float maxSpeedMultiplier = 1.3f;

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

    [Header("시각적 회전")]
    public float rotationAngle = 15f;
    public float rotationSpeed = 10f;
    public Transform visualModel; // 시각적 회전용 모델 (없으면 자동으로 찾음)

    private Rigidbody rb;
    private float targetX;
    private bool isHit = false;
    private int currentLane;
    private float laneChangeTimer;
    private bool isChangingLane = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 물리 회전 고정

        // PlayerController에서 laneDistance 가져오기
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            laneDistance = player.laneDistance;
        }

        // 시각적 모델 자동 찾기 (지정 안 됐으면 첫 번째 자식 사용)
        if (visualModel == null && transform.childCount > 0)
        {
            visualModel = transform.GetChild(0);
        }

        // 초기 회전 설정 (이동 방향에 맞게)
        if (visualModel != null)
        {
            float initialYRotation = moveTowardsPlayer ? 180f : 0f;
            visualModel.rotation = Quaternion.Euler(0, initialYRotation, 0);
        }

        // 속도 랜덤화
        if (randomizeSpeed)
        {
            float speedMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
            driveForce *= speedMultiplier;
            maxDriveSpeed *= speedMultiplier;
        }

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

        // 충돌 회피 체크 (차선 변경 중이면 멈추지 않음)
        bool shouldStop = false;
        if (enableAvoidance && !isChangingLane)
        {
            shouldStop = CheckForObstacles();
        }

        // 차선 변경 완료 체크 (거의 도착했을 때만)
        float distanceToTarget = Mathf.Abs(targetX - rb.position.x);
        if (isChangingLane && distanceToTarget < 0.1f)
        {
            isChangingLane = false;
        }

        // 차선 변경 타이머 (차선 변경 중이 아닐 때만)
        if (enableLaneChange && !isChangingLane)
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

        // 시각적 회전 업데이트
        UpdateVisualRotation(xDiff);
    }

    void UpdateVisualRotation(float xDifference)
    {
        if (visualModel == null) return;

        // xDifference 기반으로 회전량 계산 (-1 ~ 1 범위로 정규화)
        // 부호 반전: 오른쪽으로 이동할 때 오른쪽으로 기울어야 함
        float turnPercent = Mathf.Clamp(-xDifference / laneDistance, -1f, 1f);
        float targetYRotation = turnPercent * rotationAngle;

        // 이동 방향에 따라 기본 Y 회전 설정 (플레이어 방향이면 180도)
        float baseYRotation = moveTowardsPlayer ? 180f : 0f;

        Quaternion targetRotation = Quaternion.Euler(0, baseYRotation + targetYRotation, 0);
        visualModel.rotation = Quaternion.Slerp(visualModel.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
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
        // 이미 차선 변경 중이면 무시
        if (isChangingLane)
            return false;

        int newLane = currentLane + direction;

        // 범위 체크
        if (newLane < minLane || newLane > maxLane)
            return false;

        // 차선 변경 중이면 옆 체크 스킵
        if (isChangingLane)
        {
            return false;
        }

        // 옆 차선에 뭐가 있는지 확인
        Vector3 sideDirection = direction > 0 ? Vector3.right : Vector3.left;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (!Physics.Raycast(rayOrigin, sideDirection, sideRayDistance))
        {
            // 비어있으면 차선 변경
            currentLane = newLane;
            targetX = currentLane * laneDistance;
            isChangingLane = true;
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
