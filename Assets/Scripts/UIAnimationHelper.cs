using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI 요소에 간단한 애니메이션을 추가하는 헬퍼 클래스
/// 버튼 호버, 클릭 효과 등을 제공
/// </summary>
public class UIAnimationHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Color Animation")]
    [SerializeField] private bool enableColorAnimation = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image image;
    private bool isHovering = false;
    private bool isPressed = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        image = GetComponent<Image>();

        if (image != null && enableColorAnimation)
        {
            image.color = normalColor;
        }
    }

    void Update()
    {
        // 스케일 애니메이션
        if (enableScaleAnimation)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled) return;

        isHovering = true;

        if (enableScaleAnimation && !isPressed)
        {
            targetScale = originalScale * hoverScale;
        }

        if (enableColorAnimation && image != null)
        {
            image.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled) return;

        isHovering = false;

        if (enableScaleAnimation && !isPressed)
        {
            targetScale = originalScale;
        }

        if (enableColorAnimation && image != null)
        {
            image.color = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enabled) return;

        isPressed = true;

        if (enableScaleAnimation)
        {
            targetScale = originalScale * clickScale;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!enabled) return;

        isPressed = false;

        if (enableScaleAnimation)
        {
            targetScale = isHovering ? originalScale * hoverScale : originalScale;
        }
    }

    /// <summary>
    /// 펄스 애니메이션 (주목 효과)
    /// </summary>
    public void PlayPulseAnimation(float duration = 1f, float pulseScale = 1.2f)
    {
        StartCoroutine(PulseCoroutine(duration, pulseScale));
    }

    private IEnumerator PulseCoroutine(float duration, float pulseScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * pulseScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 2f / duration, 1f);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = startScale;
    }
}
