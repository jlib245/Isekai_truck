using UnityEngine;

/// <summary>
/// 게임 전체의 오디오를 관리하는 싱글톤 매니저
/// SFX와 BGM을 분리해서 관리하며, 볼륨 조절 가능
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip collisionClip;
    [SerializeField] private AudioClip heroHitClip;
    [SerializeField] private AudioClip rewardClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip questCompleteClip;
    [SerializeField] private AudioClip carEngineClip;
    [SerializeField] private AudioClip tireScreechClip;

    // 차량이 엔진음에 접근할 수 있도록 public property
    public AudioClip CarEngineClip => carEngineClip;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private AudioClip gameOverBGM;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.5f;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // AudioSource가 없으면 생성
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        // 볼륨 설정
        sfxSource.volume = sfxVolume;
        bgmSource.volume = bgmVolume;
    }

    #region SFX Methods

    /// <summary>
    /// SFX 재생 (일반)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    /// <summary>
    /// 충돌 사운드 재생 (장애물)
    /// </summary>
    public void PlayCollisionSound()
    {
        PlaySFX(collisionClip, 1f);
    }

    /// <summary>
    /// 용사 충돌 사운드 재생 (더 가볍고 부드러운 소리)
    /// </summary>
    public void PlayHeroHitSound()
    {
        PlaySFX(heroHitClip, 0.8f);
    }

    /// <summary>
    /// 보상 획득 사운드 재생
    /// </summary>
    public void PlayRewardSound()
    {
        PlaySFX(rewardClip, 0.8f);
    }

    /// <summary>
    /// 버튼 클릭 사운드 재생
    /// </summary>
    public void PlayButtonClickSound()
    {
        PlaySFX(buttonClickClip, 0.6f);
    }

    /// <summary>
    /// 퀘스트 완료 사운드 재생
    /// </summary>
    public void PlayQuestCompleteSound()
    {
        PlaySFX(questCompleteClip, 0.9f);
    }

    /// <summary>
    /// 차량 엔진 사운드 재생
    /// </summary>
    public void PlayCarEngineSound()
    {
        PlaySFX(carEngineClip, 1f);
    }

    /// <summary>
    /// 타이어 마찰음 재생
    /// </summary>
    public void PlayTireScreechSound()
    {
        PlaySFX(tireScreechClip, 0.6f);
    }

    #endregion

    #region BGM Methods

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return; // 이미 같은 BGM이 재생 중이면 무시
            }

            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 타이틀 BGM 재생
    /// </summary>
    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM);
    }

    /// <summary>
    /// 게임 BGM 재생
    /// </summary>
    public void PlayGameBGM()
    {
        PlayBGM(gameBGM);
    }

    /// <summary>
    /// 게임오버 BGM 재생
    /// </summary>
    public void PlayGameOverBGM()
    {
        PlayBGM(gameOverBGM);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// SFX 볼륨 가져오기
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    /// <summary>
    /// BGM 볼륨 가져오기
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    #endregion
}
