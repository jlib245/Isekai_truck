using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] targetPrefabs;    // 용사 프리팹 배열
    public GameObject[] obstaclePrefabs;  // 장애물(차량) 프리팹 배열

    public Transform playerTransform; // 플레이어 위치 (스폰 거리 계산용)
    
    [Header("Spawn Settings")]
    // (DELETED) PlayerController.cs의 laneDistance와 *반드시* 같은 값이어야 합니다.
    // public float laneDistance = 3.5f; 
    public float spawnAheadDistance = 50f; // 플레이어보다 얼마나 앞에서 스폰할지
    public float spawnInterval = 1.5f;  // 스폰 간격 (초)
    
    private float lastSpawnZ = 0f; // (NOTE: 이 변수는 현재 로직에서 사용되지 않음)
    
    // (NEW) PlayerController에서 읽어올 변수들
    private PlayerController playerController;
    private float laneDistance; 

    void Start()
    {
        // (NEW) 플레이어 오브젝트 찾기
        GameObject player = null;
        if (playerTransform != null)
        {
            player = playerTransform.gameObject;
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // (NEW) 플레이어 null 체크
        if (player == null)
        {
            Debug.LogError("SpawnManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        // (NEW) PlayerController 컴포넌트 가져오기
        playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("SpawnManager: Player 오브젝트에서 'PlayerController' 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // (NEW) PlayerController로부터 laneDistance 값 읽어오기
        laneDistance = playerController.laneDistance;


        // 스폰 코루틴 시작
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 게임이 Playing 상태가 될 때까지 1초마다 확인
        while (GameManager.Instance.state != GameState.Playing)
        {
            yield return new WaitForSeconds(1f);
        }

        // 게임이 Playing 상태일 때, spawnInterval 간격으로 계속 스폰
        while (true)
        {
            // (참고: 현재 로직은 플레이어 위치와 상관없이 시간 간격으로만 스폰합니다)
            // 플레이어보다 앞쪽에 스폰 위치(Z) 결정
            float spawnZ = playerTransform.position.z + spawnAheadDistance;

            // 80% 확률로 장애물, 20% 확률로 타겟 스폰 (예시)
            if (Random.Range(0f, 1f) < 0.8f)
            {
                SpawnObject(obstaclePrefabs, spawnZ);
            }
            else
            {
                SpawnObject(targetPrefabs, spawnZ);
            }
            
            // 다음 스폰까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject(GameObject[] prefabs, float spawnZ)
    {
        if (prefabs.Length == 0) return;

        // 5차선 중 랜덤한 X 위치 계산 (-2 ~ 2) * laneDistance
        // PlayerController의 laneDistance와 -2, 2 제한을 참고
        int randomLane = Random.Range(-2, 3); // -2, -1, 0, 1, 2
        
        // (MODIFIED) 이제 이 laneDistance는 PlayerController에서 가져온 값입니다.
        float spawnX = randomLane * laneDistance; 

        // 스폰 위치
        // (Y=0.5f로 설정하여 바닥에 살짝 떠서 스폰되도록 함)
        Vector3 spawnPosition = new Vector3(spawnX, 0.5f, spawnZ);

        // 랜덤 프리팹 선택
        int randomIndex = Random.Range(0, prefabs.Length);
        GameObject prefabToSpawn = prefabs[randomIndex];

        // 오브젝트 생성
        Instantiate(prefabToSpawn, spawnPosition, prefabToSpawn.transform.rotation);
    }
}