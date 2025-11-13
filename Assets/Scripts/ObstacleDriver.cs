using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleDriver : MonoBehaviour
{
    [Header("AI 주행 설정")]
    public float driveForce = 50f;
    public float maxDriveSpeed = 5f;
    public float laneCorrectionForce = 20f;
    public float damping = 2f;

    private Rigidbody rb;
    private float targetX;
    private bool isHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetX = transform.position.x;
    }

    void FixedUpdate()
    {
        if (isHit) return;

        if (GameManager.Instance.state != GameState.Playing)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float zForce = rb.velocity.z < maxDriveSpeed ? driveForce : 0;
        float xDiff = targetX - rb.position.x;
        float xForce = (xDiff * laneCorrectionForce) - (rb.velocity.x * damping);

        rb.AddForce(xForce, 0, zForce, ForceMode.Force);
    }

    public void HitByPlayer()
    {
        isHit = true;
    }
}