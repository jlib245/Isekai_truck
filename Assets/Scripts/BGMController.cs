using UnityEngine;

/// <summary>
/// 씬별 배경음악을 자동으로 재생하는 컨트롤러
/// 각 씬에 배치하면 자동으로 해당 씬의 BGM을 재생합니다
/// </summary>
public class BGMController : MonoBehaviour
{
    public enum BGMType
    {
        Title,
        Game,
        GameOver
    }

    [Header("BGM Settings")]
    [SerializeField] private BGMType bgmType;

    void Start()
    {
        PlayBGM();
    }

    void PlayBGM()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[BGMController] AudioManager가 없습니다!");
            return;
        }

        switch (bgmType)
        {
            case BGMType.Title:
                AudioManager.Instance.PlayTitleBGM();
                Debug.Log("[BGMController] 타이틀 BGM 재생");
                break;

            case BGMType.Game:
                AudioManager.Instance.PlayGameBGM();
                Debug.Log("[BGMController] 게임 BGM 재생");
                break;

            case BGMType.GameOver:
                AudioManager.Instance.PlayGameOverBGM();
                Debug.Log("[BGMController] 게임오버 BGM 재생");
                break;
        }
    }
}
