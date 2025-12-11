using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    public Text moneyText;          // 이번 게임 수익
    public Text totalMoneyText;     // 누적 총 금액
    public Text heroCountText;
    public Text obstacleCountText;
    public Text correctHeroText;    // 퀘스트 대상 용사 수
    public Text wrongHeroText;      // 잘못 친 용사 수
    public Button restartButton;
    public Button quitButton;

    [Header("Display Settings")]
    public Transform displayParent;
    public Vector3 displayAreaCenter = new Vector3(0, 0, 0);
    public float objectSpacing = 2.5f;
    public int objectsPerRow = 6;

    [Header("Drop Animation")]
    public float dropHeight = 20f;
    public float dropDelay = 0.1f; // 각 오브젝트마다 떨어지는 간격
    public float randomPositionRange = 3f; // 떨어지는 위치의 랜덤 범위
    public float randomTorqueStrength = 5f; // 회전력의 강도

    [Header("Count Animation")]
    public float countDuration = 1.5f; // 카운트 애니메이션 시간
    public float countStartDelay = 0.5f; // 시작 전 딜레이

    [Header("Camera Settings")]
    public Camera gameOverCamera;
    public Vector3 cameraPosition = new Vector3(0, 15, -10);
    public Vector3 cameraRotation = new Vector3(45, 0, 0);

    [Header("Result Image")]
    public Image resultImage;
    public Sprite goddessSprite;    // 플러스일 때 보상금 주는 여신님
    public Sprite billSprite;       // 마이너스일 때 청구서

    void Start()
    {
        StartCoroutine(InitializeGameOver());
    }

    IEnumerator InitializeGameOver()
    {
        // GameManager가 준비될 때까지 대기
        while (GameManager.Instance == null)
        {
            Debug.Log("GameOverController: GameManager를 기다리는 중...");
            yield return null;
        }

        SetupCamera();
        DisplayCollectedObjects();
        UpdateUI();
        UpdateResultImage();
        SetupButtons();
    }

    void SetupCamera()
    {
        if (gameOverCamera == null)
            gameOverCamera = Camera.main;

        if (gameOverCamera != null)
        {
            gameOverCamera.transform.position = cameraPosition;
            gameOverCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        }
    }

    void DisplayCollectedObjects()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameOverController: GameManager가 없습니다!");
            return;
        }

        // 용사와 장애물을 하나의 리스트로 합침
        List<GameObject> allObjects = new List<GameObject>();
        allObjects.AddRange(GameManager.Instance.GetCollectedHeroes());
        allObjects.AddRange(GameManager.Instance.GetCollectedObstacles());

        // 리스트를 섞음 (랜덤하게)
        ShuffleList(allObjects);

        // 각 오브젝트를 배치하고 떨어뜨림
        for (int i = 0; i < allObjects.Count; i++)
        {
            GameObject obj = allObjects[i];
            if (obj == null) continue;

            // 부모 설정
            if (displayParent != null)
                obj.transform.SetParent(displayParent);

            // 물리 활성화
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
            }

            // 물리 설정
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 1f;
            rb.drag = 0.5f; // 공기 저항
            rb.angularDrag = 0.5f; // 회전 저항

            // Collider 확인 및 추가
            if (obj.GetComponent<Collider>() == null)
            {
                BoxCollider collider = obj.AddComponent<BoxCollider>();
            }

            // Animator 다시 활성화
            Animator animator = obj.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
            }

            // 떨어지는 애니메이션 시작
            StartCoroutine(DropObject(obj, i * dropDelay));
        }

        Debug.Log($"GameOverController: 총 {allObjects.Count}개의 오브젝트를 배치했습니다.");
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    IEnumerator DropObject(GameObject obj, float delay)
    {
        // 초기 딜레이
        yield return new WaitForSeconds(delay);

        // 랜덤한 시작 위치 (위쪽, X와 Z는 랜덤)
        float randomX = Random.Range(-randomPositionRange, randomPositionRange);
        float randomZ = Random.Range(-randomPositionRange, randomPositionRange);
        Vector3 startPosition = displayAreaCenter + new Vector3(randomX, dropHeight, randomZ);

        obj.transform.position = startPosition;
        obj.transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );

        // Rigidbody에 랜덤한 회전력 추가
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 초기 속도와 회전력 설정
            rb.velocity = Vector3.zero;
            rb.angularVelocity = new Vector3(
                Random.Range(-randomTorqueStrength, randomTorqueStrength),
                Random.Range(-randomTorqueStrength, randomTorqueStrength),
                Random.Range(-randomTorqueStrength, randomTorqueStrength)
            );
        }
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // 초기값 0으로 설정
        if (moneyText != null) moneyText.text = "This Run: 0G";
        if (totalMoneyText != null) totalMoneyText.text = "Total Gold: 0G";
        if (heroCountText != null) heroCountText.text = "Heroes: 0";
        if (obstacleCountText != null) obstacleCountText.text = "Obstacles: 0";
        if (correctHeroText != null) correctHeroText.text = "Correct Heroes: 0";
        if (wrongHeroText != null) wrongHeroText.text = "Wrong Heroes: 0";

        // 카운트 애니메이션 시작
        StartCoroutine(AnimateAllCounts());
    }

    IEnumerator AnimateAllCounts()
    {
        yield return new WaitForSeconds(countStartDelay);

        // 모든 카운트 애니메이션을 동시에 시작
        if (moneyText != null)
            StartCoroutine(AnimateCount(moneyText, "This Run: ", GameManager.Instance.money, "G"));
        if (totalMoneyText != null)
            StartCoroutine(AnimateCount(totalMoneyText, "Total Gold: ", GameManager.Instance.totalMoney, "G"));
        if (heroCountText != null)
            StartCoroutine(AnimateCount(heroCountText, "Heroes: ", GameManager.Instance.GetCollectedHeroes().Count, ""));
        if (obstacleCountText != null)
            StartCoroutine(AnimateCount(obstacleCountText, "Obstacles: ", GameManager.Instance.obstacleHitCount, ""));
        if (correctHeroText != null)
            StartCoroutine(AnimateCount(correctHeroText, "Correct Heroes: ", GameManager.Instance.correctHeroCount, ""));
        if (wrongHeroText != null)
            StartCoroutine(AnimateCount(wrongHeroText, "Wrong Heroes: ", GameManager.Instance.wrongHeroCount, ""));
    }

    IEnumerator AnimateCount(Text textComponent, string prefix, int targetValue, string suffix)
    {
        float elapsed = 0f;
        int currentValue = 0;

        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / countDuration;

            // EaseOut 효과 (처음엔 빠르고 끝에 느려짐)
            progress = 1f - Mathf.Pow(1f - progress, 3f);

            currentValue = Mathf.RoundToInt(Mathf.Lerp(0, targetValue, progress));
            textComponent.text = prefix + currentValue + suffix;

            yield return null;
        }

        // 최종값 확실히 설정
        textComponent.text = prefix + targetValue + suffix;
    }

    void UpdateResultImage()
    {
        if (resultImage == null) return;
        if (GameManager.Instance == null) return;

        // 이번 게임 수익에 따라 이미지 변경
        if (GameManager.Instance.money >= 0)
        {
            // 플러스: 여신님 이미지
            if (goddessSprite != null)
            {
                resultImage.sprite = goddessSprite;
                resultImage.gameObject.SetActive(true);
            }
        }
        else
        {
            // 마이너스: 청구서 이미지
            if (billSprite != null)
            {
                resultImage.sprite = billSprite;
                resultImage.gameObject.SetActive(true);
            }
        }
    }

    void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnRestartClicked()
    {
        // 수집된 용사/장애물들 모두 파괴
        if (GameManager.Instance != null)
        {
            foreach (GameObject hero in GameManager.Instance.GetCollectedHeroes())
            {
                if (hero != null) Destroy(hero);
            }
            foreach (GameObject obstacle in GameManager.Instance.GetCollectedObstacles())
            {
                if (obstacle != null) Destroy(obstacle);
            }

            // GameManager 파괴
            Destroy(GameManager.Instance.gameObject);
        }

        // TitleScene으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }

    void OnQuitClicked()
    {
        // 타이틀 씬이 있으면 그쪽으로, 없으면 게임 종료
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
