using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AnimalARSetup : EditorWindow
{
    [MenuItem("Tools/Animal Rescue AR/Setup Mission UI In Current Scene")]
    public static void SetupMissionUiInCurrentScene()
    {
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();

        if (uiManager == null)
        {
            GameObject uiManagerGo = new GameObject("UIManager");
            uiManager = uiManagerGo.AddComponent<UIManager>();
        }

        uiManager.SetupMissionUiNow();

        AnimalARManager arManager = Object.FindAnyObjectByType<AnimalARManager>();
        if (arManager != null)
            arManager.uiManager = uiManager;

        EditorUtility.SetDirty(uiManager);

        if (arManager != null)
            EditorUtility.SetDirty(arManager);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog(
            "Mission UI Ready",
            "The Animal Rescue Mission UI has been created and wired in the current scene.\n\nSave the scene before pressing Play.",
            "OK"
        );
    }

    [MenuItem("Tools/Build Animal AR Project")]
    public static void BuildScene()
    {
        // 1. Create new empty scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 2. Base Hierarchy
        GameObject arCamera = new GameObject("ARCamera");
        arCamera.AddComponent<Camera>();
        arCamera.tag = "MainCamera";

        GameObject dirLight = new GameObject("Directional Light");
        Light light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);

        GameObject gameManager = new GameObject("GameManager");
        GameObject uiManagerGo = new GameObject("UIManager");
        GameObject placementCtrl = new GameObject("PlacementController");

        // 3. Canvas Setup
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // 4. UI Elements
        GameObject instTextGo = new GameObject("InstructionText");
        instTextGo.transform.SetParent(canvasGo.transform, false);
        TextMeshProUGUI instText = instTextGo.AddComponent<TextMeshProUGUI>();
        instText.text = "Step 1: Scan an animal card.";
        RectTransform instRect = instText.GetComponent<RectTransform>();
        instRect.anchorMin = new Vector2(0.5f, 1);
        instRect.anchorMax = new Vector2(0.5f, 1);
        instRect.anchoredPosition = new Vector2(0, -50);
        instRect.sizeDelta = new Vector2(800, 100);
        instText.alignment = TextAlignmentOptions.Center;

        GameObject fbTextGo = new GameObject("FeedbackText");
        fbTextGo.transform.SetParent(canvasGo.transform, false);
        TextMeshProUGUI fbText = fbTextGo.AddComponent<TextMeshProUGUI>();
        fbText.text = "";
        RectTransform fbRect = fbText.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0.5f, 1);
        fbRect.anchorMax = new Vector2(0.5f, 1);
        fbRect.anchoredPosition = new Vector2(0, -150);
        fbRect.sizeDelta = new Vector2(800, 100);
        fbText.alignment = TextAlignmentOptions.Center;
        fbText.color = Color.yellow;

        GameObject hintTextGo = new GameObject("InteractionHintText");
        hintTextGo.transform.SetParent(canvasGo.transform, false);
        TextMeshProUGUI hintText = hintTextGo.AddComponent<TextMeshProUGUI>();
        hintText.text = "Tap the animal to replay.";
        RectTransform hintRect = hintText.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0);
        hintRect.anchorMax = new Vector2(0.5f, 0);
        hintRect.anchoredPosition = new Vector2(0, 150);
        hintRect.sizeDelta = new Vector2(600, 50);
        hintText.alignment = TextAlignmentOptions.Center;
        hintTextGo.SetActive(false);

        GameObject btnGo = new GameObject("ResetButton");
        btnGo.transform.SetParent(canvasGo.transform, false);
        Image btnImg = btnGo.AddComponent<Image>();
        Button resetBtn = btnGo.AddComponent<Button>();
        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(0, 0);
        btnRect.anchoredPosition = new Vector2(100, 50);
        btnRect.sizeDelta = new Vector2(160, 50);
        
        GameObject btnTextGo = new GameObject("Text");
        btnTextGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
        btnText.text = "Reset";
        btnText.color = Color.black;
        btnText.alignment = TextAlignmentOptions.Center;

        GameObject goHomeBtnGo = CreateButton(canvasGo.transform, "GoHomeButton", "Go Home", new Vector2(-100, 50));
        Button goHomeBtn = goHomeBtnGo.GetComponent<Button>();
        goHomeBtnGo.SetActive(false);

        GameObject clearPathBtnGo = CreateButton(canvasGo.transform, "ClearPathButton", "Clear Path", new Vector2(100, 50));
        Button clearPathBtn = clearPathBtnGo.GetComponent<Button>();
        clearPathBtnGo.SetActive(false);

        // 5. Wire UX Scripts
        UIManager uiManager = uiManagerGo.AddComponent<UIManager>();
        uiManager.instructionText = instText;
        uiManager.feedbackText = fbText;
        uiManager.interactionHintText = hintTextGo;
        uiManager.goHomeButton = goHomeBtn;
        uiManager.clearPathButton = clearPathBtn;
        uiManager.resetButton = resetBtn;

        ResetButtonUI resetBtnUi = btnGo.AddComponent<ResetButtonUI>();
        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(resetBtnUi.ResetExperience);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.onClick, action);

        PlacementController placementController = placementCtrl.AddComponent<PlacementController>();

        // 6. Image Targets
        GameObject targetPenguin = new GameObject("ImageTarget_Penguin");
        AnimalTargetHandler ath = targetPenguin.AddComponent<AnimalTargetHandler>();
        ath.animalId = "penguin";

        GameObject targetArctic = new GameObject("ImageTarget_Arctic");
        HomeTargetHandler hthArctic = targetArctic.AddComponent<HomeTargetHandler>();
        hthArctic.homeId = "arctic";
        GameObject arcticVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arcticVis.name = "ArcticHomeVisual";
        arcticVis.transform.SetParent(targetArctic.transform, false);
        arcticVis.transform.localScale = Vector3.one * 0.14f;
        hthArctic.homeVisual = arcticVis;

        GameObject targetDesert = new GameObject("ImageTarget_Desert");
        HomeTargetHandler hthDesert = targetDesert.AddComponent<HomeTargetHandler>();
        hthDesert.homeId = "desert";
        GameObject desertVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desertVis.name = "DesertHomeVisual";
        desertVis.transform.SetParent(targetDesert.transform, false);
        desertVis.transform.localScale = Vector3.one * 0.14f;
        hthDesert.homeVisual = desertVis;

        // 7. Create Penguin Prefab
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        
        GameObject penguinTemp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        penguinTemp.name = "PenguinModelBase";
        MeshRenderer helperRenderer = penguinTemp.GetComponent<MeshRenderer>();
        if (helperRenderer != null) helperRenderer.enabled = false;
        BoxCollider col = penguinTemp.GetComponent<BoxCollider>();
        if(col == null) penguinTemp.AddComponent<BoxCollider>();
        
        // Setup Animator (Requires a controller but we just attach component)
        Animator anim = penguinTemp.AddComponent<Animator>();
        
        AnimalReaction ar = penguinTemp.AddComponent<AnimalReaction>();
        ar.animator = anim;

        AnimalMoveToHome am = penguinTemp.AddComponent<AnimalMoveToHome>();
        am.animator = anim;

        AnimalTapInteraction at = penguinTemp.AddComponent<AnimalTapInteraction>();
        at.animator = anim;

        string prefabPath = "Assets/Prefabs/PenguinCardPrefab.prefab";
        GameObject penguinPrefab = PrefabUtility.SaveAsPrefabAsset(penguinTemp, prefabPath);
        Object.DestroyImmediate(penguinTemp);

        // 8. Wire GameManager
        AnimalARManager arManager = gameManager.AddComponent<AnimalARManager>();
        arManager.uiManager = uiManager;
        arManager.placementController = placementController;
        
        AnimalData pd = new AnimalData();
        pd.animalId = "penguin";
        pd.displayName = "Penguin";
        pd.correctHomeId = "arctic";
        pd.wrongHomeMessage = "Too hot for a penguin. Try again.";
        pd.correctHomeMessage = "Great job! The penguin found its icy home.";
        pd.cardAnimalPrefab = penguinPrefab;

        arManager.animals = new AnimalData[] { pd };

        // 9. Save Scene
        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");

        string scenePath = "Assets/Scenes/AnimalAR.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log("Animal AR Scene and Prefab successfully generated!");
        EditorUtility.DisplayDialog("Success", "Animal AR Project Setup Complete!\n\nScene saved to Assets/Scenes/AnimalAR.unity.\n\nNext Steps:\n1. Open the Scene.\n2. Add Vuforia ImageTarget components to the placeholders.\n3. Assign an Animator Controller to the Penguin prefab.", "OK");
    }

    private static GameObject CreateButton(Transform canvasTransform, string objectName, string label, Vector2 anchoredPosition)
    {
        GameObject buttonGo = new GameObject(objectName);
        buttonGo.transform.SetParent(canvasTransform, false);
        buttonGo.AddComponent<Image>();
        buttonGo.AddComponent<Button>();

        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(170, 50);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return buttonGo;
    }
}
