using UnityEngine;

public class RoadTile : MonoBehaviour
{
    private MapSpawner mapSpawner;

    public void Setup(MapSpawner spawner)
    {
        mapSpawner = spawner;
    }

    // 참고: 이 기본 버전에서는 MapSpawner가 직접 타일을 관리합니다.
    // 만약 타일이 스스로 파괴 시점을 결정하게 하려면 (예: 플레이어와 너무 멀어지면)
    // 이 스크립트에서 Update 로직을 추가할 수 있습니다.

    /*
    void Update()
    {
        // 예시: 플레이어보다 3칸 이상 뒤에 있으면 자신을 파괴(재활용) 요청
        if (mapSpawner != null && mapSpawner.playerTransform.position.z > transform.position.z + (mapSpawner.tileLength * 3))
        {
            mapSpawner.RecycleTile(gameObject);
        }
    }
    */
}