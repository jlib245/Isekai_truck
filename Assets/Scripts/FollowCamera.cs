using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeAmount = 0.5f;

    private Vector3 originalOffset;
    private bool isShaking = false;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("FollowCamera: Target이 할당되지 않았습니다!");
            return;
        }

        originalOffset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null || isShaking) return;

        Vector3 desiredPosition = target.position + originalOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void ShakeCamera()
    {
        if (!isShaking)
            StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        isShaking = true;
        float timer = 0f;

        while (timer < shakeDuration)
        {
            Vector3 shakePos = Random.insideUnitSphere * shakeAmount;
            transform.position = target.position + originalOffset + shakePos;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position + originalOffset;
        isShaking = false;
    }
}