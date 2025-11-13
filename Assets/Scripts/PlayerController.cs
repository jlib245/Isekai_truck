using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 200f;
    public float maxSpeed = 10f;
    public float laneChangeSpeed = 15f;
    public float laneDistance = 3.5f;

    [Header("Visuals")]
    public Transform truckModel;
    public float rotationAngle = 15f;
    public float rotationSpeed = 10f;

    private const int MIN_LANE = -2;
    private const int MAX_LANE = 2;
    private const int CENTER_LANE = 0;

    private int currentLane = CENTER_LANE;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool isPlaying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody not found!");
            return;
        }

        targetPosition = rb.position;

        if (truckModel == null)
            truckModel = transform;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChange;
            HandleGameStateChange(GameManager.Instance.state);
        }
        else
        {
            Debug.LogError("PlayerController: GameManager.Instance is not available!");
            HandleGameStateChange(GameState.Playing);
        }
    }

    void HandleGameStateChange(GameState newState)
    {
        isPlaying = (newState == GameState.Playing);
        rb.isKinematic = !isPlaying;
    }

    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ChangeLane(1);
    }

    void FixedUpdate()
    {
        if (!isPlaying) return;

        ApplyLaneChangeForce();
        ApplyForwardMovement();
        UpdateVisualRotation();
    }

    void ApplyLaneChangeForce()
    {
        float xVel = (targetPosition.x - rb.position.x) * laneChangeSpeed;
        Vector3 velChange = new Vector3(xVel, rb.velocity.y, rb.velocity.z) - rb.velocity;

        velChange.x = Mathf.Clamp(velChange.x, -laneChangeSpeed, laneChangeSpeed);
        velChange.y = 0;
        velChange.z = 0;

        rb.AddForce(velChange, ForceMode.VelocityChange);
    }

    void ApplyForwardMovement()
    {
        if (rb.velocity.z < maxSpeed)
            rb.AddForce(0, 0, moveForce, ForceMode.Force);
    }

    void UpdateVisualRotation()
    {
        float xDifference = targetPosition.x - rb.position.x;
        float turnPercent = Mathf.Clamp(xDifference / laneDistance, -1f, 1f);
        float targetYRotation = turnPercent * rotationAngle;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);

        truckModel.rotation = Quaternion.Slerp(truckModel.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    void ChangeLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, MIN_LANE, MAX_LANE);
        targetPosition.x = currentLane * laneDistance;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleGameStateChange;
    }
}