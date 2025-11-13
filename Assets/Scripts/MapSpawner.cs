using UnityEngine;
using System.Collections.Generic;

public class MapSpawner : MonoBehaviour
{
    public GameObject roadPrefab;
    public Transform playerTransform;
    public int initialTiles = 5;
    public int tilesToMaintain = 3;
    public float tileLength = 20f;

    private Queue<GameObject> activeTiles = new Queue<GameObject>();
    private Queue<GameObject> inactiveTiles = new Queue<GameObject>();
    private float nextSpawnZ;

    void Start()
    {
        nextSpawnZ = -tileLength;
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        if (playerTransform.position.z > (nextSpawnZ - tilesToMaintain * tileLength))
        {
            SpawnTile();
            DespawnTile();
        }
    }

    void SpawnTile()
    {
        GameObject newTile;

        if (inactiveTiles.Count > 0)
        {
            newTile = inactiveTiles.Dequeue();
            newTile.SetActive(true);
        }
        else
        {
            newTile = Instantiate(roadPrefab);
            newTile.transform.SetParent(transform);

            RoadTile roadTile = newTile.GetComponent<RoadTile>();
            if (roadTile != null)
                roadTile.Setup(this);
        }

        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        newTile.transform.position = spawnPosition;

        activeTiles.Enqueue(newTile);
        nextSpawnZ += tileLength;
    }

    void DespawnTile()
    {
        if (activeTiles.Count > initialTiles + 1)
        {
            GameObject oldestTile = activeTiles.Dequeue();
            oldestTile.SetActive(false);
            inactiveTiles.Enqueue(oldestTile);
        }
    }

    public void RecycleTile(GameObject tile)
    {
        if (activeTiles.Peek() == tile)
        {
            activeTiles.Dequeue();
            tile.SetActive(false);
            inactiveTiles.Enqueue(tile);
        }
    }
}