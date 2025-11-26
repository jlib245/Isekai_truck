using UnityEngine;
using System.Collections.Generic;

public class MapSpawner : MonoBehaviour
{
    public GameObject[] roadPrefabs;
    public Transform playerTransform;
    public int tilesToMaintain = 6;
    public float tileLength = 20f;

    private Queue<GameObject> activeTiles = new Queue<GameObject>();
    private Dictionary<int, Queue<GameObject>> inactiveTilesByType = new Dictionary<int, Queue<GameObject>>();
    private float nextSpawnZ;

    void Start()
    {
        nextSpawnZ = -tileLength;
        for (int i = 0; i < tilesToMaintain; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // 게임이 Playing 상태일 때만 맵 생성
        if (GameManager.Instance == null || GameManager.Instance.state != GameState.Playing)
            return;

        if (playerTransform.position.z > (nextSpawnZ - tilesToMaintain * tileLength))
        {
            SpawnTile();
            DespawnTile();
        }
    }

    void SpawnTile()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0)
        {
            Debug.LogError("MapSpawner: roadPrefabs 배열이 비어있습니다!");
            return;
        }

        // 랜덤으로 프리팹 선택
        int randomIndex = Random.Range(0, roadPrefabs.Length);
        GameObject selectedPrefab = roadPrefabs[randomIndex];

        GameObject newTile;

        // 해당 타입의 비활성 타일이 있는지 확인
        if (inactiveTilesByType.ContainsKey(randomIndex) && inactiveTilesByType[randomIndex].Count > 0)
        {
            newTile = inactiveTilesByType[randomIndex].Dequeue();
            newTile.SetActive(true);
        }
        else
        {
            // 새로 생성
            newTile = Instantiate(selectedPrefab);
            newTile.transform.SetParent(transform);

            RoadTile roadTile = newTile.GetComponent<RoadTile>();
            if (roadTile != null)
                roadTile.Setup(this);

            // 타일에 타입 인덱스 저장 (나중에 재활용할 때 사용)
            TileTypeHolder typeHolder = newTile.AddComponent<TileTypeHolder>();
            typeHolder.typeIndex = randomIndex;
        }

        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        newTile.transform.position = spawnPosition;

        activeTiles.Enqueue(newTile);
        nextSpawnZ += tileLength;
    }

    void DespawnTile()
    {
        if (activeTiles.Count > tilesToMaintain + 1)
        {
            GameObject oldestTile = activeTiles.Dequeue();
            oldestTile.SetActive(false);

            // 타입별로 재활용 큐에 추가
            TileTypeHolder typeHolder = oldestTile.GetComponent<TileTypeHolder>();
            if (typeHolder != null)
            {
                int typeIndex = typeHolder.typeIndex;
                if (!inactiveTilesByType.ContainsKey(typeIndex))
                    inactiveTilesByType[typeIndex] = new Queue<GameObject>();

                inactiveTilesByType[typeIndex].Enqueue(oldestTile);
            }
        }
    }

    public void RecycleTile(GameObject tile)
    {
        if (activeTiles.Peek() == tile)
        {
            activeTiles.Dequeue();
            tile.SetActive(false);

            // 타입별로 재활용 큐에 추가
            TileTypeHolder typeHolder = tile.GetComponent<TileTypeHolder>();
            if (typeHolder != null)
            {
                int typeIndex = typeHolder.typeIndex;
                if (!inactiveTilesByType.ContainsKey(typeIndex))
                    inactiveTilesByType[typeIndex] = new Queue<GameObject>();

                inactiveTilesByType[typeIndex].Enqueue(tile);
            }
        }
    }
}

// 타일 타입을 저장하는 헬퍼 클래스
public class TileTypeHolder : MonoBehaviour
{
    public int typeIndex;
}