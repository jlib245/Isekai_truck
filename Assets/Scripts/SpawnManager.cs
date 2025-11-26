using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] targetPrefabs;
    public GameObject[] obstaclePrefabs;
    public Transform playerTransform;
    public MapSpawner mapSpawner;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.8f;
    public float spawnHeightY = 0.5f;

    private const int MIN_LANE = -2;
    private const int MAX_LANE = 2;

    private PlayerController playerController;
    private float laneDistance; 

    void Start()
    {
        if (!InitializePlayer())
            return;

        StartCoroutine(SpawnRoutine());
    }

    bool InitializePlayer()
    {
        GameObject player = playerTransform != null
            ? playerTransform.gameObject
            : GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("SpawnManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return false;
        }

        if (playerTransform == null)
            playerTransform = player.transform;

        playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("SpawnManager: Player 오브젝트에서 'PlayerController' 컴포넌트를 찾을 수 없습니다!");
            return false;
        }

        laneDistance = playerController.laneDistance;
        return true;
    }

    IEnumerator SpawnRoutine()
    {
        while (GameManager.Instance.state != GameState.Playing)
            yield return new WaitForSeconds(1f);

        while (GameManager.Instance.state == GameState.Playing)
        {
            float spawnAheadDistance = mapSpawner != null
                ? mapSpawner.tilesToMaintain * mapSpawner.tileLength
                : 50f;

            float spawnZ = playerTransform.position.z + spawnAheadDistance;
            GameObject[] prefabsToSpawn = Random.Range(0f, 1f) < obstacleSpawnChance
                ? obstaclePrefabs
                : targetPrefabs;

            SpawnObject(prefabsToSpawn, spawnZ);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject(GameObject[] prefabs, float spawnZ)
    {
        if (prefabs.Length == 0) return;

        int randomLane = Random.Range(MIN_LANE, MAX_LANE + 1);
        float spawnX = randomLane * laneDistance;
        Vector3 spawnPosition = new Vector3(spawnX, spawnHeightY, spawnZ);

        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];
        Instantiate(prefabToSpawn, spawnPosition, prefabToSpawn.transform.rotation);
    }
}