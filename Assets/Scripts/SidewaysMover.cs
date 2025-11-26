using UnityEngine;

public class SidewaysMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;
    public float raycastDistance = 1.5f;

    [Header("Boundaries")]
    public int minLane = -2;
    public int maxLane = 2;
    private float minX;
    private float maxX;
    private float laneDistance;

    [Header("Animation Settings")]
    public bool useAnimation = true;
    public string walkAnimationName = "locom_m_basicWalk_30f"; // 걷기 애니메이션 이름 (이동 시 고정)
    public bool rotateToMoveDirection = true; // 이동 방향으로 회전

    private int moveDirection = 0; // -1: 왼쪽, 0: 정지, 1: 오른쪽
    private int lastMoveDirection = 0;
    private float directionTimer;
    private float directionLockTimer = 0f; // 방향 고정 타이머
    private Animator animator;
    private AnimationClip[] allClips; // 랜덤 애니메이션용

    void Start()
    {
        // PlayerController에서 laneDistance 가져오기
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            laneDistance = player.laneDistance;
        }
        else
        {
            laneDistance = 3.5f; // 기본값
        }

        // 경계 계산
        minX = minLane * laneDistance;
        maxX = maxLane * laneDistance;

        // 랜덤한 시작 방향
        moveDirection = Random.Range(-1, 2);
        directionTimer = changeDirectionInterval;

        // Animator 가져오기
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // 랜덤 애니메이션용 클립 목록 가져오기
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            allClips = animator.runtimeAnimatorController.animationClips;
        }
    }

    void Update()
    {
        // 방향 고정 타이머 감소
        if (directionLockTimer > 0f)
        {
            directionLockTimer -= Time.deltaTime;
        }

        // 일정 시간마다 방향 변경 (고정 타이머가 0일 때만)
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f && directionLockTimer <= 0f)
        {
            ChangeDirection();
            directionTimer = changeDirectionInterval + Random.Range(-0.5f, 0.5f);
        }

        // 이동 처리
        if (moveDirection != 0)
        {
            TryMove();
        }

        // 애니메이션 및 회전 처리
        if (useAnimation && animator != null)
        {
            UpdateAnimation();
        }
    }

    void ChangeDirection()
    {
        // 랜덤하게 방향 변경 (-1, 0, 1)
        moveDirection = Random.Range(-1, 2);

        // 방향 변경 후 최소 1초간 고정
        directionLockTimer = 1f;
    }

    void UpdateAnimation()
    {
        // 방향이 바뀌었을 때만 애니메이션 변경
        if (moveDirection != lastMoveDirection)
        {
            lastMoveDirection = moveDirection;

            if (moveDirection != 0)
            {
                // 이동 중: 걷기 애니메이션 (고정) - 즉시 전환
                animator.Play(walkAnimationName, 0, 0f);

                // 이동 방향으로 회전
                if (rotateToMoveDirection)
                {
                    float targetAngle = moveDirection > 0 ? 90f : -90f;
                    transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
                }
            }
            else
            {
                // 대기 중: 랜덤 애니메이션 (춤, idle 등)
                PlayRandomIdleAnimation();
            }
        }
    }

    void PlayRandomIdleAnimation()
    {
        if (allClips == null || allClips.Length == 0) return;

        // 걷기/뛰기 애니메이션 제외하고 랜덤 선택
        AnimationClip randomClip = allClips[Random.Range(0, allClips.Length)];
        animator.CrossFadeInFixedTime(randomClip.name, 0.3f);
    }

    void TryMove()
    {
        Vector3 rayDirection = moveDirection > 0 ? Vector3.right : Vector3.left;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // 약간 위에서 발사

        // Raycast로 옆에 장애물 확인
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, raycastDistance))
        {
            // 뭔가 있으면 정지 (방향 전환 안 함)
            if (hit.collider.gameObject != gameObject)
            {
                moveDirection = 0; // 그냥 정지
                directionLockTimer = 0.5f;
                return;
            }
        }

        // 경계 체크 - 현재 위치와 이동 방향 고려
        float currentX = transform.position.x;

        // 왼쪽 경계 근처에서 왼쪽으로 가려고 하면
        if (currentX <= minX + 0.1f && moveDirection < 0)
        {
            moveDirection = 0; // 정지
            directionLockTimer = 0.5f;
            return;
        }
        // 오른쪽 경계 근처에서 오른쪽으로 가려고 하면
        if (currentX >= maxX - 0.1f && moveDirection > 0)
        {
            moveDirection = 0; // 정지
            directionLockTimer = 0.5f;
            return;
        }

        // 이동
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    // 디버그용 - Scene 뷰에서 Raycast 시각화
    void OnDrawGizmosSelected()
    {
        if (moveDirection == 0) return;

        Vector3 rayDirection = moveDirection > 0 ? Vector3.right : Vector3.left;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayOrigin, rayDirection * raycastDistance);
    }
}
