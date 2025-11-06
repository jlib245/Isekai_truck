using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 200f;  // Z축으로 가하는 '힘' (ForceMode.Force)
    public float maxSpeed = 10f;    // 최대 속도
    public float laneChangeSpeed = 15f; // 차선 변경 속도
    public float laneDistance = 3.5f;   // 차선 간 거리 (SpawnManager와 일치해야 함)

    [Header("Visuals")]
    public Transform truckModel; // 트럭의 시각적 모델 (메쉬가 있는 부분)
    public float rotationAngle = 15f; // 차선 변경 시 최대 회전 각도
    public float rotationSpeed = 10f; // 회전 부드러움 속도

    private int currentLane = 0; // 0 = 중앙, -1/1 = 좌우, -2/2 = 가장자리
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool isPlaying = false; // (NEW) 게임 상태 캐싱용

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody not found!");
            return;
        }

        targetPosition = rb.position; // 현재 위치로 목표 위치 초기화

        // truckModel이 할당되지 않았다면, 자기 자신을 사용 (단순한 구조일 경우)
        if (truckModel == null)
        {
            truckModel = transform;
        }

        // GameManager의 게임 상태 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChange;
            // (NEW) 초기 상태 설정
            HandleGameStateChange(GameManager.Instance.state); 
        }
        else
        {
            Debug.LogError("PlayerController: GameManager.Instance is not available!");
            // 게임 매니저가 없으면 일단 플레이 가능하게 설정 (테스트용)
            HandleGameStateChange(GameState.Playing);
        }
    }

    // 게임 상태에 따라 플레이어 제어 활성화/비활성화
    void HandleGameStateChange(GameState newState)
    {
        isPlaying = (newState == GameState.Playing);
        
        // (NEW) 게임이 플레이 중이 아니면, Rigidbody를 멈춥니다. (물리 비활성화)
        if (!isPlaying)
        {
            rb.isKinematic = true; 
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    void Update()
    {
        // (MODIFIED) 게임 상태가 Playing이 아니면 입력을 받지 않음
        if (!isPlaying) return;

        // 좌우 입력 감지
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }
    }

    // 물리 기반 이동은 FixedUpdate에서 처리
    void FixedUpdate()
    {
        // (MODIFIED) 게임 상태가 Playing이 아니면 물리 업데이트 중지
        if (!isPlaying) return;
        
        // 1. 목표 X 위치 계산 (차선 변경)
        // 목표 X좌표(targetPosition.x)와 현재 X좌표(rb.position.x)의 차이를 기반으로
        // 차선 변경에 필요한 X축 속도(xVel)를 계산합니다.
        float xVel = (targetPosition.x - rb.position.x) * laneChangeSpeed;
        
        // 2. 현재 Y, Z 속도 유지
        // X축 속도만 강제로 변경하고, Y축과 Z축 속도는 현재 값을 유지합니다.
        float yVel = rb.velocity.y;
        float zVel = rb.velocity.z;

        // 3. X축 속도 즉시 변경 (Y, Z는 유지)
        // 현재 속도(rb.velocity)와 목표 속도(Vector3(xVel, yVel, zVel))의 차이(velChange)를 계산합니다.
        Vector3 velChange = new Vector3(xVel, yVel, zVel) - rb.velocity;
        
        // (MODIFIED) Y, Z축 변경은 0으로 막고 X축 변경만 허용
        velChange.x = Mathf.Clamp(velChange.x, -laneChangeSpeed, laneChangeSpeed); // X축 속도 제한
        velChange.y = 0; // Y축 속도는 변경하지 않음
        velChange.z = 0; // Z축 속도는 아래에서 Force로 제어하므로 여기서 변경하지 않음

        // ForceMode.VelocityChange를 사용해 질량과 관계없이 즉시 X축 속도를 변경
        rb.AddForce(velChange, ForceMode.VelocityChange);


        // 4. Z축 속도 변경 (항상 전진)
        // 현재 전진 속도가 최대 속도(maxSpeed)보다 느릴 때만 전진 힘(moveForce)을 가합니다.
        if (rb.velocity.z < maxSpeed)
        {
            rb.AddForce(0, 0, moveForce, ForceMode.Force);
        }

        // 5. 시각적 회전 (차선 변경)
        // 목표 위치(targetPosition.x)와 현재 위치(rb.position.x)의 차이를 계산
        float xDifference = targetPosition.x - rb.position.x;
        
        // 차이를 -1.0 ~ 1.0 사이로 정규화 (최대 차이는 laneDistance)
        // (xDifference / laneDistance)는 한 차선만큼 차이날 때 1.0이 됩니다.
        float turnPercent = Mathf.Clamp(xDifference / laneDistance, -1f, 1f); 

        // 목표 회전 각도 계산
        float targetYRotation = turnPercent * rotationAngle;

        // 목표 회전값(Quaternion) 생성
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);

        // truckModel의 현재 회전값을 목표 회전값으로 부드럽게(Slerp) 변경
        truckModel.rotation = Quaternion.Slerp(truckModel.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    void ChangeLane(int direction)
    {
        // 5차선 (-2, -1, 0, 1, 2) 범위를 넘지 않도록 Clamp
        int newLane = Mathf.Clamp(currentLane + direction, -2, 2);
        
        // 현재 차선 업데이트
        currentLane = newLane;
        
        // 목표 X좌표 계산 (예: -2 * 3.5f = -7f)
        targetPosition.x = currentLane * laneDistance;
    }

    // GameManager 이벤트 구독 해제 (메모리 누수 방지)
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChange;
        }
    }
}