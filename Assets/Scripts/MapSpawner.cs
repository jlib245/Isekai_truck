using UnityEngine;
using System.Collections.Generic;

public class MapSpawner : MonoBehaviour
{
    public GameObject roadPrefab; // 도로 타일 프리팹
    public Transform playerTransform; // 플레이어의 Transform
    public int initialTiles = 5; // 처음에 생성할 타일 수
    public int tilesToMaintain = 3; // 플레이어 앞에 유지할 최소 타일 수
    public float tileLength = 20f; // 도로 타일의 길이 (Z축)

    private Queue<GameObject> activeTiles = new Queue<GameObject>();
    private float nextSpawnZ;

    void Start()
    {
        // 초기 맵 생성
        nextSpawnZ = -tileLength; // 플레이어 위치보다 한 칸 뒤에서 생성 시작
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // 플레이어가 일정 거리 이상 전진하면 새 타일을 생성하고 오래된 타일을 제거
        if (playerTransform.position.z > (nextSpawnZ - tilesToMaintain * tileLength))
        {
            SpawnTile();
            DespawnTile();
        }
    }

    // 새 타일 생성 (오브젝트 풀링)
    void SpawnTile()
    {
        // TODO: 오브젝트 풀링을 구현하면 더 효율적입니다.
        // 지금은 간단히 Instantiate로 구현합니다.
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject newTile = Instantiate(roadPrefab, spawnPosition, Quaternion.identity);
        newTile.transform.SetParent(transform); // MapSpawner 자식으로 정리

        activeTiles.Enqueue(newTile);
        nextSpawnZ += tileLength;

        // RoadTile 스크립트에 MapSpawner 참조 전달
        RoadTile roadTile = newTile.GetComponent<RoadTile>();
        if (roadTile != null)
        {
            roadTile.Setup(this);
        }
    }

    // 가장 오래된 타일 제거
    void DespawnTile()
    {
        if (activeTiles.Count > initialTiles + 1) // 일정 수 이상이면
        {
            GameObject oldestTile = activeTiles.Dequeue();
            // TODO: 풀링을 사용한다면 SetActive(false)로 변경
            Destroy(oldestTile);
        }
    }

    // (옵션) RoadTile이 이 함수를 호출하여 재활용을 요청할 수 있습니다.
    public void RecycleTile(GameObject tile)
    {
        // 이 예제에서는 간단히 Destroy 하지만,
        // 고급 풀링에서는 여기서 타일을 큐에 다시 넣습니다.
        if (activeTiles.Peek() == tile)
        {
             activeTiles.Dequeue();
             Destroy(tile);
        }
    }
}