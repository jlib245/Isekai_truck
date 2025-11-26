using UnityEngine;

/// <summary>
/// 용사의 타입/직업을 정의하는 컴포넌트
/// targetPrefabs에 있는 각 프리팹에 이 컴포넌트를 추가하세요
/// </summary>
public class HeroType : MonoBehaviour
{
    [Header("Hero Information")]
    public string heroName = "용사";

    [TextArea(2, 4)]
    public string description = "이세계로 보내야 할 용사";

    // 프리팹 구분용 - 각 프리팹마다 고유한 ID를 가져야 함
    public int heroID = 0;

    public string GetHeroName()
    {
        return heroName;
    }

    public string GetDescription()
    {
        return description;
    }
}
