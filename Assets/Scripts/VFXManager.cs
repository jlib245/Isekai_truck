using UnityEngine;

/// <summary>
/// 게임 전체의 VFX(파티클 이펙트)를 관리하는 싱글톤 매니저
/// 충돌, 보상 획득 등 다양한 시각 효과를 제공
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Prefabs")]
    [Tooltip("충돌 시 재생할 파티클 (폭발, 먼지 등)")]
    [SerializeField] private GameObject collisionParticlePrefab;

    [Tooltip("보상 획득 시 재생할 파티클 (반짝임, 별 등)")]
    [SerializeField] private GameObject rewardParticlePrefab;

    [Tooltip("퀘스트 완료 시 재생할 파티클 (축하 효과)")]
    [SerializeField] private GameObject questCompleteParticlePrefab;

    [Header("Particle Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private GameObject[] collisionParticlePool;
    private GameObject[] rewardParticlePool;
    private GameObject[] questCompleteParticlePool;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        // 파티클 오브젝트 풀 초기화 (성능 최적화)
        collisionParticlePool = new GameObject[poolSize];
        rewardParticlePool = new GameObject[poolSize];
        questCompleteParticlePool = new GameObject[poolSize];

        // 풀 생성은 프리팹이 있을 때만
        if (collisionParticlePrefab != null)
            CreatePool(collisionParticlePrefab, collisionParticlePool);

        if (rewardParticlePrefab != null)
            CreatePool(rewardParticlePrefab, rewardParticlePool);

        if (questCompleteParticlePrefab != null)
            CreatePool(questCompleteParticlePrefab, questCompleteParticlePool);
    }

    private void CreatePool(GameObject prefab, GameObject[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = Instantiate(prefab, transform);
            pool[i].SetActive(false);
        }
    }

    #region Particle Playback Methods

    /// <summary>
    /// 충돌 파티클 재생
    /// </summary>
    public void PlayCollisionEffect(Vector3 position, Vector3 normal)
    {
        if (collisionParticlePrefab == null)
        {
            Debug.LogWarning("[VFXManager] 충돌 파티클 프리팹이 설정되지 않았습니다!");
            return;
        }

        GameObject particle = GetPooledParticle(collisionParticlePool);
        if (particle != null)
        {
            particle.transform.position = position;
            particle.transform.rotation = Quaternion.LookRotation(normal);
            particle.SetActive(true);

            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                StartCoroutine(DeactivateAfterPlay(particle, ps));
            }
        }
    }

    /// <summary>
    /// 보상 획득 파티클 재생
    /// </summary>
    public void PlayRewardEffect(Vector3 position)
    {
        if (rewardParticlePrefab == null)
        {
            Debug.LogWarning("[VFXManager] 보상 파티클 프리팹이 설정되지 않았습니다!");
            return;
        }

        GameObject particle = GetPooledParticle(rewardParticlePool);
        if (particle != null)
        {
            particle.transform.position = position;
            particle.SetActive(true);

            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                StartCoroutine(DeactivateAfterPlay(particle, ps));
            }
        }
    }

    /// <summary>
    /// 퀘스트 완료 파티클 재생
    /// </summary>
    public void PlayQuestCompleteEffect(Vector3 position)
    {
        if (questCompleteParticlePrefab == null)
        {
            Debug.LogWarning("[VFXManager] 퀘스트 완료 파티클 프리팹이 설정되지 않았습니다!");
            return;
        }

        GameObject particle = GetPooledParticle(questCompleteParticlePool);
        if (particle != null)
        {
            particle.transform.position = position;
            particle.SetActive(true);

            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                StartCoroutine(DeactivateAfterPlay(particle, ps));
            }
        }
    }

    #endregion

    #region Object Pool Helper Methods

    private GameObject GetPooledParticle(GameObject[] pool)
    {
        if (pool == null) return null;

        // 비활성화된 파티클 찾기
        foreach (GameObject obj in pool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // 모든 파티클이 사용 중이면 첫 번째 것 재사용
        return pool.Length > 0 ? pool[0] : null;
    }

    private System.Collections.IEnumerator DeactivateAfterPlay(GameObject particle, ParticleSystem ps)
    {
        // 파티클 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);

        if (particle != null)
        {
            particle.SetActive(false);
        }
    }

    #endregion

    #region Simple VFX (Without Particle System)

    /// <summary>
    /// 간단한 플래시 효과 (파티클 시스템 없이)
    /// </summary>
    public void PlayFlashEffect(SpriteRenderer renderer, Color flashColor, float duration = 0.1f)
    {
        if (renderer != null)
        {
            StartCoroutine(FlashCoroutine(renderer, flashColor, duration));
        }
    }

    private System.Collections.IEnumerator FlashCoroutine(SpriteRenderer renderer, Color flashColor, float duration)
    {
        Color originalColor = renderer.color;
        renderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        renderer.color = originalColor;
    }

    #endregion
}
