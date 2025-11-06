using UnityEngine;
using System.Collections; // (NEW) 코루틴을 위해 추가

public class FollowCamera : MonoBehaviour
{
    public Transform target; // 플레이어(트럭)의 Transform
    public float smoothSpeed = 10f; // 카메라가 따라가는 부드러운 속도

    // (NEW) 카메라 쉐이크 설정
    [Header("Camera Shake")]
    public float shakeDuration = 0.3f; // 흔들림 지속 시간
    public float shakeAmount = 0.5f;   // 흔들림 강도

    private Vector3 originalOffset; // (MODIFIED) 초기 오프셋 저장용
    private bool isShaking = false; // (NEW) 현재 흔들리는 중인지 확인

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("FollowCamera: Target이 할당되지 않았습니다!");
            return;
        }
        
        // (MODIFIED) 초기 오프셋을 'originalOffset'에 저장
        originalOffset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return; 
        }

        // (NEW) 쉐이크 중에는 부드러운 이동 로직을 실행하지 않습니다.
        if (isShaking)
        {
            return;
        }

        // (MODIFIED) 'originalOffset'을 사용
        Vector3 desiredPosition = target.position + originalOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    // (NEW) 카메라를 흔드는 공개 함수
    public void ShakeCamera()
    {
        if (isShaking) return; // 이미 흔들리는 중이면 중복 실행 방지
        StartCoroutine(ShakeRoutine());
    }

    // (NEW) 실제 카메라를 흔드는 코루틴
    IEnumerator ShakeRoutine()
    {
        isShaking = true;
        float timer = 0f;

        while (timer < shakeDuration)
        {
            // 목표 위치(오프셋) 주변으로 랜덤하게 흔들림
            // Random.insideUnitSphere는 (0,0,0) 중심의 반지름 1 구체 내 랜덤 위치를 반환
            Vector3 shakePos = Random.insideUnitSphere * shakeAmount;

            // 타겟 위치 + 원래 오프셋 + 흔들림 값을 카메라 위치로 설정
            transform.position = target.position + originalOffset + shakePos;

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 흔들림이 끝나면 정확한 위치로 복귀
        transform.position = target.position + originalOffset;
        isShaking = false;
    }
}