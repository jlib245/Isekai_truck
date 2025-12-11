using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 도로 양쪽에 건물을 자동으로 배치하는 스크립트
/// MapSpawner와 연동하여 도로 타일마다 건물 생성
/// </summary>
public class BuildingSpawner : MonoBehaviour
{
    [Header("Building Prefabs")]
    [Tooltip("SimplePoly City 건물 프리팹 배열")]
    public GameObject[] buildingPrefabs;

    [Header("Ground Prefabs")]
    [Tooltip("건물 밑에 깔릴 잔디/바닥 타일 프리팹")]
    public GameObject groundTilePrefab;

    [Tooltip("바닥 타일 크기 (X축)")]
    public float groundTileWidth = 10f;

    [Tooltip("바닥 타일 크기 (Z축)")]
    public float groundTileLength = 10f;

    [Tooltip("바닥 타일 높이 (도로보다 낮게)")]
    public float groundTileHeight = -0.2f;

    [Header("Spawn Settings")]
    [Tooltip("도로로부터 건물까지 거리 (차선 폭 고려)")]
    public float buildingOffsetX = 15f;

    [Tooltip("도로 타일 길이 (MapSpawner와 동일하게)")]
    public float tileLength = 20f;

    [Tooltip("타일당 건물 개수 (양쪽)")]
    public int buildingsPerTile = 2;

    [Tooltip("건물 간격")]
    public float buildingSpacing = 10f;

    [Header("Randomization")]
    [Tooltip("건물 회전 랜덤화")]
    public bool randomRotation = true;

    [Tooltip("건물 크기 랜덤화 (0.8 ~ 1.2)")]
    public bool randomScale = false;

    [Header("Pool Settings")]
    public int poolSizePerType = 5;

    // 오브젝트 풀
    private Dictionary<int, Queue<GameObject>> buildingPoolsByType = new Dictionary<int, Queue<GameObject>>();
    private List<GameObject> activeBuildings = new List<GameObject>();

    // 바닥 타일 풀
    private Queue<GameObject> groundTilePool = new Queue<GameObject>();
    private List<GameObject> activeGroundTiles = new List<GameObject>();

    void Start()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogWarning("[BuildingSpawner] buildingPrefabs가 비어있습니다! 건물이 생성되지 않습니다.");
        }
    }

    /// <summary>
    /// 도로 타일 위치에 건물 배치
    /// </summary>
    public void SpawnBuildingsForTile(Vector3 tilePosition)
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            return;

        // 바닥 타일 먼저 생성 (왼쪽, 오른쪽)
        if (groundTilePrefab != null)
        {
            SpawnGroundTile(tilePosition, true);  // 왼쪽
            SpawnGroundTile(tilePosition, false); // 오른쪽
        }

        // 왼쪽 건물들
        for (int i = 0; i < buildingsPerTile; i++)
        {
            float zOffset = (tileLength / (buildingsPerTile + 1)) * (i + 1);
            Vector3 leftPos = tilePosition + new Vector3(-buildingOffsetX, 0, zOffset);
            SpawnBuilding(leftPos, true); // 왼쪽
        }

        // 오른쪽 건물들
        for (int i = 0; i < buildingsPerTile; i++)
        {
            float zOffset = (tileLength / (buildingsPerTile + 1)) * (i + 1);
            Vector3 rightPos = tilePosition + new Vector3(buildingOffsetX, 0, zOffset);
            SpawnBuilding(rightPos, false); // 오른쪽
        }
    }

    /// <summary>
    /// 단일 건물 생성
    /// </summary>
    void SpawnBuilding(Vector3 position, bool isLeft)
    {
        // 랜덤 건물 선택
        int randomIndex = Random.Range(0, buildingPrefabs.Length);
        GameObject buildingPrefab = buildingPrefabs[randomIndex];

        GameObject building = GetPooledBuilding(randomIndex);

        if (building == null)
        {
            // 풀에 없으면 새로 생성
            building = Instantiate(buildingPrefab, transform);

            // 타입 인덱스 저장
            BuildingTypeHolder typeHolder = building.AddComponent<BuildingTypeHolder>();
            typeHolder.typeIndex = randomIndex;
        }

        building.transform.position = position;
        building.SetActive(true);

        // 회전 설정 (도로를 향하도록)
        if (randomRotation)
        {
            float yRotation = isLeft ? 90f : -90f;
            yRotation += Random.Range(-15f, 15f); // 약간의 변화
            building.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        else
        {
            float yRotation = isLeft ? 90f : -90f;
            building.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        // 크기 랜덤화
        if (randomScale)
        {
            float scale = Random.Range(0.8f, 1.2f);
            building.transform.localScale = Vector3.one * scale;
        }

        activeBuildings.Add(building);
    }

    /// <summary>
    /// 오래된 건물 제거 (플레이어가 지나간 건물)
    /// </summary>
    public void DespawnOldBuildings(float playerZ, float despawnDistance)
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject building in activeBuildings)
        {
            if (building != null && building.transform.position.z < playerZ - despawnDistance)
            {
                toRemove.Add(building);
            }
        }

        foreach (GameObject building in toRemove)
        {
            RecycleBuilding(building);
            activeBuildings.Remove(building);
        }

        // 바닥 타일도 제거
        List<GameObject> groundTilesToRemove = new List<GameObject>();

        foreach (GameObject groundTile in activeGroundTiles)
        {
            if (groundTile != null && groundTile.transform.position.z < playerZ - despawnDistance)
            {
                groundTilesToRemove.Add(groundTile);
            }
        }

        foreach (GameObject groundTile in groundTilesToRemove)
        {
            RecycleGroundTile(groundTile);
            activeGroundTiles.Remove(groundTile);
        }
    }

    /// <summary>
    /// 풀에서 건물 가져오기
    /// </summary>
    GameObject GetPooledBuilding(int typeIndex)
    {
        if (buildingPoolsByType.ContainsKey(typeIndex) && buildingPoolsByType[typeIndex].Count > 0)
        {
            return buildingPoolsByType[typeIndex].Dequeue();
        }
        return null;
    }

    /// <summary>
    /// 건물을 풀로 반환
    /// </summary>
    void RecycleBuilding(GameObject building)
    {
        if (building == null) return;

        BuildingTypeHolder typeHolder = building.GetComponent<BuildingTypeHolder>();
        if (typeHolder != null)
        {
            int typeIndex = typeHolder.typeIndex;

            if (!buildingPoolsByType.ContainsKey(typeIndex))
                buildingPoolsByType[typeIndex] = new Queue<GameObject>();

            building.SetActive(false);
            buildingPoolsByType[typeIndex].Enqueue(building);
        }
        else
        {
            Destroy(building);
        }
    }

    /// <summary>
    /// 바닥 타일 생성
    /// </summary>
    void SpawnGroundTile(Vector3 tilePosition, bool isLeft)
    {
        if (groundTilePrefab == null)
            return;

        GameObject groundTile = GetPooledGroundTile();

        if (groundTile == null)
        {
            // 풀에 없으면 새로 생성
            groundTile = Instantiate(groundTilePrefab, transform);
        }

        // 도로 양쪽에 바닥 타일 배치 (도로보다 낮게)
        float xOffset = isLeft ? -buildingOffsetX : buildingOffsetX;
        Vector3 position = tilePosition + new Vector3(xOffset, groundTileHeight, tileLength / 2f);

        groundTile.transform.position = position;
        groundTile.transform.rotation = Quaternion.identity;

        // 크기 조정
        groundTile.transform.localScale = new Vector3(groundTileWidth, 1f, groundTileLength);

        groundTile.SetActive(true);
        activeGroundTiles.Add(groundTile);
    }

    /// <summary>
    /// 풀에서 바닥 타일 가져오기
    /// </summary>
    GameObject GetPooledGroundTile()
    {
        if (groundTilePool.Count > 0)
        {
            return groundTilePool.Dequeue();
        }
        return null;
    }

    /// <summary>
    /// 바닥 타일을 풀로 반환
    /// </summary>
    void RecycleGroundTile(GameObject groundTile)
    {
        if (groundTile == null) return;

        groundTile.SetActive(false);
        groundTilePool.Enqueue(groundTile);
    }

    /// <summary>
    /// 모든 건물 제거
    /// </summary>
    public void ClearAllBuildings()
    {
        foreach (GameObject building in activeBuildings)
        {
            if (building != null)
                RecycleBuilding(building);
        }
        activeBuildings.Clear();

        // 바닥 타일도 모두 제거
        foreach (GameObject groundTile in activeGroundTiles)
        {
            if (groundTile != null)
                RecycleGroundTile(groundTile);
        }
        activeGroundTiles.Clear();
    }
}

/// <summary>
/// 건물 타입을 저장하는 헬퍼 클래스
/// </summary>
public class BuildingTypeHolder : MonoBehaviour
{
    public int typeIndex;
}
