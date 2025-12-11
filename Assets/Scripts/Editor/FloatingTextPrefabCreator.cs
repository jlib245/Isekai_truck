using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

public class FloatingTextPrefabCreator : MonoBehaviour
{
    [MenuItem("GameObject/UI/Create Floating Text Prefab")]
    static void CreateFloatingTextPrefab()
    {
        // 1. Canvas 생성 (World Space)
        GameObject canvasObj = new GameObject("FloatingTextPrefab");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        // Canvas 크기 및 스케일 설정
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 100);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // 2. CanvasGroup 추가 (페이드 아웃용)
        CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 3. Text 오브젝트 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.text = "+500G";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 50;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.yellow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 20;
        text.resizeTextMaxSize = 50;

        // Text RectTransform 설정
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // 4. Prefabs 폴더 확인 및 생성
        string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 5. Prefab으로 저장
        string prefabPath = prefabFolder + "/FloatingTextPrefab.prefab";

        // 기존 프리팹이 있으면 덮어쓰기
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Floating Text Prefab 덮어쓰기",
                "FloatingTextPrefab.prefab이 이미 존재합니다. 덮어쓰시겠습니까?",
                "예",
                "아니오"
            );

            if (!overwrite)
            {
                DestroyImmediate(canvasObj);
                return;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);

        // 6. Hierarchy에서 제거
        DestroyImmediate(canvasObj);

        // 7. 생성된 프리팹 선택
        GameObject createdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = createdPrefab;
        EditorGUIUtility.PingObject(createdPrefab);

        Debug.Log($"[FloatingTextPrefab] 생성 완료: {prefabPath}");
        EditorUtility.DisplayDialog(
            "성공!",
            "FloatingTextPrefab이 Assets/Prefabs/ 폴더에 생성되었습니다!\n\n다음 단계:\n1. FloatingTextManager GameObject를 선택\n2. Floating Text Prefab 필드에 이 프리팹을 드래그하세요.",
            "확인"
        );
    }
}
#endif
