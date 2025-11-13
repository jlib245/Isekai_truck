using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Collsion Settings")]
    public int obstacleDamage = 1;
    public int targetScore = 50;
    public float destroyDelay = 10f;

    [Header("Collision Physics")]
    public float targetHitForce = 15f;
    public float obstacleHitForce = 20f;
    public Vector3 hitForceDirection = new Vector3(0f, 2f, 0.5f);

    private FollowCamera cam;

    void Start()
    {
        cam = Camera.main.GetComponent<FollowCamera>();
        if (cam == null)
        {
            Debug.LogWarning("PlayerCollision: Main Camera에 FollowCamera 스크립트가 없습니다.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance.state != GameState.Playing)
            return;

        GameObject other = collision.gameObject;

        if (other.CompareTag("Target"))
        {
            HandleTargetHit(other);
        }
        else if (other.CompareTag("Obstacle"))
        {
            HandleObstacleHit(other);
        }
    }

    void HandleTargetHit(GameObject target)
    {
        GameManager.Instance.AddScore(targetScore);
        ApplyHitForce(target, targetHitForce);
        Destroy(target, destroyDelay);
    }

    void HandleObstacleHit(GameObject obstacle)
    {
        if (cam != null)
            cam.ShakeCamera();

        GameManager.Instance.TakeDamage(obstacleDamage);
        ApplyHitForce(obstacle, obstacleHitForce);

        ObstacleDriver driver = obstacle.GetComponent<ObstacleDriver>();
        if (driver != null)
            driver.HitByPlayer();

        Destroy(obstacle, destroyDelay);
    }

    void ApplyHitForce(GameObject target, float force)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = (hitForceDirection + transform.forward * hitForceDirection.z).normalized;
            rb.AddForce(forceDirection * force, ForceMode.VelocityChange);
        }
    }
}