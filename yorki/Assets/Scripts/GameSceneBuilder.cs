using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameSceneBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Yorki/Setup Game Scene")]
    public static void SetupScene()
    {
        // --- GameManager 오브젝트 ---
        GameObject gmGO = GameObject.Find("GameManager");
        if (gmGO == null)
        {
            gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Debug.Log("[GameSceneBuilder] GameManager 생성 완료.");
        }
        else
        {
            Debug.Log("[GameSceneBuilder] GameManager 이미 존재합니다. 스킵.");
        }

        // ReactionUI를 GameManager에 붙임 (없을 때만)
        if (gmGO.GetComponent<ReactionUI>() == null)
        {
            gmGO.AddComponent<ReactionUI>();
            Debug.Log("[GameSceneBuilder] ReactionUI -> GameManager에 추가 완료.");
        }

        // --- DrawingUI 캔버스 찾기 ---
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[GameSceneBuilder] Canvas를 찾을 수 없습니다. Build Drawing Scene을 먼저 실행하세요.");
            return;
        }
        Transform drawingUI = canvas.transform;

        // --- DialogueBox 생성 ---
        if (drawingUI.Find("DialogueBox") == null)
        {
            CreateDialogueBox(drawingUI);
            Debug.Log("[GameSceneBuilder] DialogueBox 생성 완료.");
        }
        else
        {
            Debug.Log("[GameSceneBuilder] DialogueBox 이미 존재합니다. 스킵.");
        }

        // --- GameManager 레퍼런스 연결 ---
        GameManager gmRef = gmGO.GetComponent<GameManager>();
        if (gmRef != null)
        {
            gmRef.drawingCanvas = Object.FindAnyObjectByType<DrawingCanvas>();
            gmRef.customerImage = drawingUI.Find("CustomerImage")?.GetComponent<RawImage>();
            gmRef.reactionUI    = gmGO.GetComponent<ReactionUI>();
        }

        // --- CustomerData 에셋 자동 생성 및 연결 ---
        const string customerAssetPath = "Assets/Resources/Customers/SampleCustomer.asset";
        CustomerData customerData = AssetDatabase.LoadAssetAtPath<CustomerData>(customerAssetPath);
        if (customerData == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Customers"))
                AssetDatabase.CreateFolder("Assets/Resources", "Customers");

            customerData = ScriptableObject.CreateInstance<CustomerData>();
            customerData.customerName = "샘플 손님";
            customerData.type         = CustomerType.Normal;
            customerData.basePay      = 20;

            customerData.zoneWeights = new float[64];
            for (int i = 0; i < 64; i++) customerData.zoneWeights[i] = 0.5f;
            int[] centerIndices = { 18,19,20,21, 26,27,28,29, 34,35,36,37, 42,43,44,45 };
            foreach (int idx in centerIndices) customerData.zoneWeights[idx] = 2.0f;

            Texture2D refTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/토미에 샘플.png");
            if (refTex != null)
                customerData.referenceImage = refTex;
            else
                Debug.LogWarning("[GameSceneBuilder] Assets/Sprites/토미에 샘플.png 를 찾지 못했습니다.");

            AssetDatabase.CreateAsset(customerData, customerAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameSceneBuilder] SampleCustomer.asset 생성 완료.");
        }
        else
        {
            Debug.Log("[GameSceneBuilder] SampleCustomer.asset 이미 존재합니다. 스킵.");
        }

        if (gmRef != null)
            gmRef.currentCustomer = customerData;

        Debug.Log("[GameSceneBuilder] 씬 셋업 완료!");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    static void CreateDialogueBox(Transform parent)
    {
        // 배경 패널 — 화면 하단 고정
        GameObject box = new GameObject("DialogueBox");
        box.transform.SetParent(parent, false);
        Image boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.06f, 0.06f, 0.06f, 0.88f);
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin        = new Vector2(0f, 0f);
        boxRect.anchorMax        = new Vector2(1f, 0f);
        boxRect.pivot            = new Vector2(0.5f, 0f);
        boxRect.sizeDelta        = new Vector2(0f, 130f);
        boxRect.anchoredPosition = new Vector2(0f, 0f);

        // 이름 텍스트 (좌상단)
        GameObject nameGO = new GameObject("NameText");
        nameGO.transform.SetParent(box.transform, false);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize  = 18;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color     = new Color(1f, 0.88f, 0.45f);
        nameText.text      = "";
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin        = new Vector2(0f, 1f);
        nameRect.anchorMax        = new Vector2(0f, 1f);
        nameRect.pivot            = new Vector2(0f, 1f);
        nameRect.sizeDelta        = new Vector2(220f, 28f);
        nameRect.anchoredPosition = new Vector2(24f, -8f);

        // 본문 텍스트
        GameObject bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(box.transform, false);
        Text bodyText = bodyGO.AddComponent<Text>();
        bodyText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize  = 22;
        bodyText.color     = Color.white;
        bodyText.text      = "";
        bodyText.alignment = TextAnchor.UpperLeft;
        RectTransform bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(24f, 10f);
        bodyRect.offsetMax = new Vector2(-24f, -38f);

        // DialogueUI 컴포넌트 연결
        DialogueUI dialogueUI  = box.AddComponent<DialogueUI>();
        dialogueUI.nameText    = nameText;
        dialogueUI.bodyText    = bodyText;
    }
#endif
}
