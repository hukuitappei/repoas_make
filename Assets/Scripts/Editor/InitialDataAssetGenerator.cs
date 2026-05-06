using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InitialDataAssetGenerator
{
    private const string RootFolder = "Assets/ScriptableObjects";
    private const string BuildingsFolder = RootFolder + "/Buildings";
    private const string GuildsFolder = RootFolder + "/Guilds";
    private const string ResearchFolder = RootFolder + "/Research";

    [MenuItem("repoas/Create Initial Data Assets")]
    public static void CreateInitialDataAssets()
    {
        EnsureFolder("Assets", "ScriptableObjects");
        EnsureFolder(RootFolder, "Buildings");
        EnsureFolder(RootFolder, "Guilds");
        EnsureFolder(RootFolder, "Research");

        CreateBuildingAssets();
        CreateGuildAssets();
        CreateResearchNodeAssets();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        DefaultAsset rootFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(RootFolder);
        if (rootFolder != null)
        {
            Selection.activeObject = rootFolder;
            EditorGUIUtility.PingObject(rootFolder);
        }

        Debug.Log("Initial data assets created under " + RootFolder);
    }

    [MenuItem("repoas/Setup GameBootstrap In Open Scene")]
    public static void SetupGameBootstrapInOpenScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before running Setup GameBootstrap In Open Scene.");
            return;
        }

        CreateInitialDataAssets();

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("No open scene is available for bootstrap setup.");
            return;
        }

        GameBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<GameBootstrap>();
        if (bootstrap == null)
        {
            GameObject bootstrapObject = new GameObject("GameBootstrap");
            bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
        }

        DebugGameHud debugHud = UnityEngine.Object.FindFirstObjectByType<DebugGameHud>();
        if (debugHud == null)
        {
            GameObject hudObject = new GameObject("DebugGameHud");
            debugHud = hudObject.AddComponent<DebugGameHud>();
        }

        SerializedObject serializedHud = new SerializedObject(debugHud);
        AssignObjectReference(serializedHud, "bootstrap", bootstrap);
        serializedHud.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(debugHud);

        SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
        AssignObjectReference(serializedBootstrap, "mainGameScreen", UnityEngine.Object.FindFirstObjectByType<MainGameScreen>());
        AssignObjectReference(serializedBootstrap, "mapPanel", UnityEngine.Object.FindFirstObjectByType<MapPanel>());
        AssignArray(serializedBootstrap, "guildCatalog", FindAssetsByType<GuildData>(GuildsFolder));
        AssignArray(serializedBootstrap, "availableBuildings", FindAssetsByType<BuildingData>(BuildingsFolder));
        AssignArray(serializedBootstrap, "startingBuildings", new UnityEngine.Object[]
        {
            LoadAsset<BuildingData>(BuildingsFolder + "/DungeonGate.asset")
        });
        serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(bootstrap);
        EditorSceneManager.MarkSceneDirty(activeScene);
        Selection.activeObject = bootstrap.gameObject;
        EditorGUIUtility.PingObject(bootstrap.gameObject);

        Debug.Log("GameBootstrap setup completed for the open scene.");
    }

    [MenuItem("repoas/Wire Panel References In Open Scene")]
    public static void WirePanelReferencesInOpenScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before running Wire Panel References In Open Scene.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("[Wire] No open scene found.");
            return;
        }

        int wired = 0;

        MainGameScreen mainScreen = UnityEngine.Object.FindFirstObjectByType<MainGameScreen>();
        if (mainScreen != null)
        {
            SerializedObject so = new SerializedObject(mainScreen);
            wired += AssignTmpTextInScene(so, "turnText", "TurnText");
            wired += AssignTmpTextInScene(so, "gameStateText", "GameStateText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mainScreen);
        }

        ResourcePanel resourcePanel = UnityEngine.Object.FindFirstObjectByType<ResourcePanel>();
        if (resourcePanel != null)
        {
            SerializedObject so = new SerializedObject(resourcePanel);
            wired += AssignTmpTextInScene(so, "summaryText", "SummaryText");
            wired += AssignTmpTextInScene(so, "materialsText", "MaterialsText");
            wired += AssignTmpTextInScene(so, "warningText", "WarningText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resourcePanel);
        }

        GuildPanel guildPanel = UnityEngine.Object.FindFirstObjectByType<GuildPanel>();
        if (guildPanel != null)
        {
            SerializedObject so = new SerializedObject(guildPanel);
            wired += AssignTmpTextInScene(so, "guildSummaryText", "GuildSummaryText");
            wired += AssignTmpTextInScene(so, "memberListText", "MemberListText");
            wired += AssignTmpTextInScene(so, "selectedMemberText", "SelectedMemberText");
            wired += AssignTmpTextInScene(so, "actionResultText", "ActionResultText");
            wired += AssignButtonInChildren(so, "previousMemberButton", guildPanel, "PreviousMemberButton");
            wired += AssignButtonInChildren(so, "nextMemberButton", guildPanel, "NextMemberButton");
            wired += AssignButtonInChildren(so, "idleButton", guildPanel, "IdleButton");
            wired += AssignButtonInChildren(so, "defendButton", guildPanel, "DefendButton");
            wired += AssignButtonInChildren(so, "exploreButton", guildPanel, "ExploreButton");
            wired += AssignButtonInChildren(so, "researchButton", guildPanel, "ResearchButton");
            wired += AssignButtonInChildren(so, "constructButton", guildPanel, "ConstructButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(guildPanel);
        }

        ExplorationPanel explorationPanel = UnityEngine.Object.FindFirstObjectByType<ExplorationPanel>();
        if (explorationPanel != null)
        {
            SerializedObject so = new SerializedObject(explorationPanel);
            wired += AssignTmpTextInScene(so, "statusText", "StatusText");
            wired += AssignTmpTextInScene(so, "resultText", "ResultText");
            wired += AssignButtonInChildren(so, "exploreButton", explorationPanel, "exploreButton");
            wired += AssignButtonInChildren(so, "startDungeonButton", explorationPanel, "StartDungeonButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(explorationPanel);
        }

        MapPanel mapPanel = UnityEngine.Object.FindFirstObjectByType<MapPanel>();
        if (mapPanel != null)
        {
            SerializedObject so = new SerializedObject(mapPanel);
            wired += AssignTmpTextInScene(so, "mapText", "MapText");
            wired += AssignTmpTextInScene(so, "legendText", "LegendText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mapPanel);
        }

        BuildPanel buildPanel = UnityEngine.Object.FindFirstObjectByType<BuildPanel>();
        if (buildPanel != null)
        {
            SerializedObject so = new SerializedObject(buildPanel);
            AssignArray(so, "availableBuildings", FindAssetsByType<BuildingData>(BuildingsFolder));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(buildPanel);
            Debug.LogWarning("[Wire] BuildPanel: ownedBuildingsText / availableBuildingsText — create two TMP Text child objects and assign manually.");
        }

        ResearchPanel researchPanel = UnityEngine.Object.FindFirstObjectByType<ResearchPanel>();
        if (researchPanel != null)
        {
            SerializedObject so = new SerializedObject(researchPanel);
            AssignArray(so, "availableNodes", FindAssetsByType<ResearchNodeData>(ResearchFolder));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(researchPanel);
            Debug.LogWarning("[Wire] ResearchPanel: activeResearchText / completedResearchText / availableResearchText — create TMP Text child objects and assign manually.");
        }

        HappinessPanel happinessPanel = UnityEngine.Object.FindFirstObjectByType<HappinessPanel>();
        if (happinessPanel != null)
        {
            Debug.LogWarning("[Wire] HappinessPanel: happinessText / detailText — create TMP Text child objects and assign manually.");
        }

        MetaScreen metaScreen = UnityEngine.Object.FindFirstObjectByType<MetaScreen>();
        if (metaScreen != null)
        {
            Debug.LogWarning("[Wire] MetaScreen: scoreText / metaPointText / lordStatsText — create TMP Text child objects and assign manually.");
        }

        RaidPopup raidPopup = UnityEngine.Object.FindFirstObjectByType<RaidPopup>();
        if (raidPopup != null)
        {
            SerializedObject so = new SerializedObject(raidPopup);
            wired += AssignButtonInChildren(so, "closeButton", raidPopup, "CloseButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(raidPopup);
            Debug.LogWarning("[Wire] RaidPopup: root (GameObject) / titleText / detailText — create child objects and assign manually.");
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log($"[Wire] Done. {wired} references auto-assigned. Check Console for manual-assignment warnings.");
    }

    [MenuItem("repoas/Layout Visible UI In Open Scene")]
    public static void LayoutVisibleUiInOpenScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before running Layout Visible UI In Open Scene.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("No open scene found.");
            return;
        }

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found.");
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystemExists();

        MainGameScreen mainScreen = UnityEngine.Object.FindFirstObjectByType<MainGameScreen>();
        ResourcePanel resourcePanel = UnityEngine.Object.FindFirstObjectByType<ResourcePanel>();
        ResearchPanel researchPanel = UnityEngine.Object.FindFirstObjectByType<ResearchPanel>();
        ExplorationPanel explorationPanel = UnityEngine.Object.FindFirstObjectByType<ExplorationPanel>();
        BuildPanel buildPanel = UnityEngine.Object.FindFirstObjectByType<BuildPanel>();
        GuildPanel guildPanel = UnityEngine.Object.FindFirstObjectByType<GuildPanel>();
        HappinessPanel happinessPanel = UnityEngine.Object.FindFirstObjectByType<HappinessPanel>();
        MapPanel mapPanel = UnityEngine.Object.FindFirstObjectByType<MapPanel>();
        RaidPopup raidPopup = UnityEngine.Object.FindFirstObjectByType<RaidPopup>();
        MetaScreen metaScreen = UnityEngine.Object.FindFirstObjectByType<MetaScreen>();

        LayoutMainGameScreen(mainScreen);
        LayoutResourcePanel(resourcePanel);
        LayoutResearchPanel(researchPanel);
        LayoutExplorationPanel(explorationPanel);
        LayoutBuildPanel(buildPanel);
        LayoutGuildPanel(guildPanel);
        LayoutHappinessPanel(happinessPanel);
        LayoutMapPanel(mapPanel);
        LayoutRaidPopup(raidPopup);
        LayoutMetaScreen(metaScreen);

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log("Visible UI layout applied.");
    }

    private static int AssignTmpTextInScene(SerializedObject so, string fieldName, string gameObjectName)
    {
        TMP_Text[] all = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text t in all)
        {
            if (string.Equals(t.gameObject.name, gameObjectName, StringComparison.OrdinalIgnoreCase))
            {
                AssignObjectReference(so, fieldName, t);
                return 1;
            }
        }

        Debug.LogWarning($"[Wire] TMP_Text '{gameObjectName}' not found in scene (field: {fieldName}).");
        return 0;
    }

    private static int AssignButtonInChildren(SerializedObject so, string fieldName, Component parent, string gameObjectName)
    {
        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (string.Equals(btn.gameObject.name, gameObjectName, StringComparison.OrdinalIgnoreCase))
            {
                AssignObjectReference(so, fieldName, btn);
                return 1;
            }
        }

        Debug.LogWarning($"[Wire] Button '{gameObjectName}' not found under {parent.gameObject.name} (field: {fieldName}).");
        return 0;
    }

    private static void LayoutMainGameScreen(MainGameScreen mainScreen)
    {
        if (mainScreen == null)
        {
            return;
        }

        SetRect(mainScreen.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 0f));

        SerializedObject so = new SerializedObject(mainScreen);
        LayoutTextField(so, "turnText", new Vector2(10f, -10f), new Vector2(420f, 40f), TextAnchor.MiddleLeft, 24);
        LayoutTextField(so, "gameStateText", new Vector2(10f, -55f), new Vector2(700f, 36f), TextAnchor.MiddleLeft, 20);
        LayoutButtonField(so, "endTurnButton", "ターン終了", new Vector2(-170f, -20f), new Vector2(160f, 44f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(mainScreen);
    }

    private static void LayoutMapPanel(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        SetRect(mapPanel.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 40f), new Vector2(440f, 420f));

        SerializedObject so = new SerializedObject(mapPanel);
        LayoutTextField(so, "mapText", new Vector2(0f, 0f), new Vector2(440f, 360f), TextAnchor.UpperLeft, 18);
        LayoutTextField(so, "legendText", new Vector2(0f, -365f), new Vector2(440f, 48f), TextAnchor.UpperLeft, 16);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(mapPanel);
    }

    private static void LayoutResourcePanel(ResourcePanel resourcePanel)
    {
        if (resourcePanel == null)
        {
            return;
        }

        SetRect(resourcePanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-460f, -20f), new Vector2(430f, 180f));

        SerializedObject so = new SerializedObject(resourcePanel);
        LayoutTextField(so, "summaryText", new Vector2(0f, 0f), new Vector2(430f, 40f), TextAnchor.MiddleLeft, 20);
        LayoutTextField(so, "materialsText", new Vector2(0f, -45f), new Vector2(430f, 100f), TextAnchor.UpperLeft, 16);
        LayoutTextField(so, "warningText", new Vector2(0f, -150f), new Vector2(430f, 60f), TextAnchor.UpperLeft, 16, Color.red);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(resourcePanel);
    }

    private static void LayoutExplorationPanel(ExplorationPanel explorationPanel)
    {
        if (explorationPanel == null)
        {
            return;
        }

        SetRect(explorationPanel.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(440f, 170f));

        SerializedObject so = new SerializedObject(explorationPanel);
        LayoutTextField(so, "statusText", new Vector2(0f, 0f), new Vector2(440f, 64f), TextAnchor.UpperLeft, 18);
        LayoutTextField(so, "resultText", new Vector2(0f, -70f), new Vector2(440f, 46f), TextAnchor.UpperLeft, 16);
        LayoutButtonField(so, "exploreButton", "襲撃元を探索", new Vector2(0f, -120f), new Vector2(160f, 40f));
        LayoutButtonField(so, "startDungeonButton", "ダンジョン開始", new Vector2(175f, -120f), new Vector2(160f, 40f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(explorationPanel);
    }

    private static void LayoutGuildPanel(GuildPanel guildPanel)
    {
        if (guildPanel == null)
        {
            return;
        }

        SetRect(guildPanel.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-560f, -120f), new Vector2(520f, 520f));

        SerializedObject so = new SerializedObject(guildPanel);
        LayoutTextField(so, "guildSummaryText", new Vector2(0f, 0f), new Vector2(520f, 120f), TextAnchor.UpperLeft, 17);
        LayoutTextField(so, "memberListText", new Vector2(0f, -125f), new Vector2(520f, 210f), TextAnchor.UpperLeft, 15);
        LayoutTextField(so, "selectedMemberText", new Vector2(0f, -340f), new Vector2(520f, 32f), TextAnchor.MiddleLeft, 16);
        LayoutTextField(so, "actionResultText", new Vector2(0f, -378f), new Vector2(520f, 42f), TextAnchor.UpperLeft, 15);
        LayoutButtonField(so, "previousMemberButton", "前", new Vector2(0f, -430f), new Vector2(60f, 36f));
        LayoutButtonField(so, "nextMemberButton", "次", new Vector2(70f, -430f), new Vector2(60f, 36f));
        LayoutButtonField(so, "idleButton", "待機", new Vector2(0f, -472f), new Vector2(72f, 36f));
        LayoutButtonField(so, "defendButton", "防衛", new Vector2(80f, -472f), new Vector2(72f, 36f));
        LayoutButtonField(so, "exploreButton", "探索", new Vector2(160f, -472f), new Vector2(72f, 36f));
        LayoutButtonField(so, "researchButton", "研究", new Vector2(240f, -472f), new Vector2(72f, 36f));
        LayoutButtonField(so, "constructButton", "建設", new Vector2(320f, -472f), new Vector2(72f, 36f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(guildPanel);
    }

    private static void LayoutBuildPanel(BuildPanel buildPanel)
    {
        if (buildPanel == null)
        {
            return;
        }

        SetRect(buildPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-250f, 20f), new Vector2(480f, 220f));
    }

    private static void LayoutResearchPanel(ResearchPanel researchPanel)
    {
        if (researchPanel == null)
        {
            return;
        }

        SetRect(researchPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-240f, -20f), new Vector2(480f, 230f));
    }

    private static void LayoutHappinessPanel(HappinessPanel happinessPanel)
    {
        if (happinessPanel == null)
        {
            return;
        }

        SetRect(happinessPanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-460f, -210f), new Vector2(430f, 100f));
    }

    private static void LayoutRaidPopup(RaidPopup raidPopup)
    {
        if (raidPopup == null)
        {
            return;
        }

        SetRect(raidPopup.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -120f), new Vector2(400f, 220f));
    }

    private static void LayoutMetaScreen(MetaScreen metaScreen)
    {
        if (metaScreen == null)
        {
            return;
        }

        SetRect(metaScreen.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-460f, 20f), new Vector2(430f, 180f));
    }

    private static void LayoutTextField(SerializedObject so, string propertyName, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize)
    {
        LayoutTextField(so, propertyName, anchoredPosition, size, anchor, fontSize, Color.black);
    }

    private static void LayoutTextField(SerializedObject so, string propertyName, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize, Color color)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == null)
        {
            return;
        }

        Text text = property.objectReferenceValue as Text;
        if (text == null)
        {
            return;
        }

        SetRect(text.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        text.alignment = anchor;
        text.fontSize = fontSize;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        EditorUtility.SetDirty(text);
    }

    private static void LayoutButtonField(SerializedObject so, string propertyName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == null)
        {
            return;
        }

        Button button = property.objectReferenceValue as Button;
        if (button == null)
        {
            return;
        }

        SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);

        Text labelText = button.GetComponentInChildren<Text>(true);
        if (labelText != null)
        {
            labelText.text = label;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 16;
            labelText.color = Color.black;
            SetRect(labelText.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, true);
            EditorUtility.SetDirty(labelText);
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
            EditorUtility.SetDirty(image);
        }

        EditorUtility.SetDirty(button);
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        SetRect(rectTransform, anchorMin, anchorMax, anchoredPosition, sizeDelta, false);
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, bool stretch)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = stretch ? Vector2.zero : sizeDelta;
        if (!stretch)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);
        }
    }

    private static void EnsureEventSystemExists()
    {
        if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void CreateBuildingAssets()
    {
        UpsertBuilding("Farm", BuildingEffectType.FoodProduction, 3,
            new[] { 80, 140, 220 }, new[] { 5, 8, 12 }, new[] { 80, 140, 220 },
            Requirements(Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Foodstuff, 10)),
            Requirements(Requirement(MaterialType.Wood, 35), Requirement(MaterialType.Stone, 10), Requirement(MaterialType.Foodstuff, 15)),
            Requirements(Requirement(MaterialType.Wood, 55), Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Foodstuff, 25)));

        UpsertBuilding("Sawmill", BuildingEffectType.WoodProduction, 3,
            new[] { 90, 160, 250 }, new[] { 6, 10, 15 }, new[] { 20, 35, 55 },
            Requirements(Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Stone, 15)),
            Requirements(Requirement(MaterialType.Wood, 35), Requirement(MaterialType.Stone, 25), Requirement(MaterialType.Metal, 10)),
            Requirements(Requirement(MaterialType.Wood, 55), Requirement(MaterialType.Stone, 40), Requirement(MaterialType.Metal, 20)));

        UpsertBuilding("Market", BuildingEffectType.FundsProduction, 3,
            new[] { 100, 180, 280 }, new[] { 0, 0, 0 }, new[] { 30, 55, 90 },
            Requirements(Requirement(MaterialType.Wood, 15), Requirement(MaterialType.Stone, 10)),
            Requirements(Requirement(MaterialType.Wood, 30), Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Metal, 10)),
            Requirements(Requirement(MaterialType.Wood, 45), Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Metal, 20)));

        UpsertBuilding("Library", BuildingEffectType.ResearchSpeedPercent, 3,
            new[] { 120, 220, 340 }, new[] { 8, 14, 22 }, new[] { 5, 10, 18 },
            Requirements(Requirement(MaterialType.Wood, 25), Requirement(MaterialType.Stone, 15), Requirement(MaterialType.Magic, 5)),
            Requirements(Requirement(MaterialType.Wood, 40), Requirement(MaterialType.Stone, 30), Requirement(MaterialType.Magic, 10)),
            Requirements(Requirement(MaterialType.Wood, 60), Requirement(MaterialType.Stone, 45), Requirement(MaterialType.Magic, 20)));

        UpsertBuilding("Wall", BuildingEffectType.DefensePower, 3,
            new[] { 100, 190, 310 }, new[] { 5, 9, 15 }, new[] { 40, 80, 140 },
            Requirements(Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Wood, 10)),
            Requirements(Requirement(MaterialType.Stone, 60), Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Metal, 10)),
            Requirements(Requirement(MaterialType.Stone, 90), Requirement(MaterialType.Wood, 30), Requirement(MaterialType.Metal, 25)));

        UpsertBuilding("Barracks", BuildingEffectType.WarriorCombatBonus, 3,
            new[] { 130, 240, 380 }, new[] { 10, 18, 28 }, new[] { 5, 10, 18 },
            Requirements(Requirement(MaterialType.Wood, 25), Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Metal, 15)),
            Requirements(Requirement(MaterialType.Wood, 40), Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Metal, 30)),
            Requirements(Requirement(MaterialType.Wood, 60), Requirement(MaterialType.Stone, 55), Requirement(MaterialType.Metal, 50)));

        UpsertBuilding("House", BuildingEffectType.PopulationCapacity, 3,
            new[] { 70, 130, 220 }, new[] { 3, 5, 8 }, new[] { 100, 220, 380 },
            Requirements(Requirement(MaterialType.Wood, 25), Requirement(MaterialType.Stone, 5)),
            Requirements(Requirement(MaterialType.Wood, 45), Requirement(MaterialType.Stone, 15)),
            Requirements(Requirement(MaterialType.Wood, 70), Requirement(MaterialType.Stone, 30)));

        UpsertBuilding("Inn", BuildingEffectType.HireCostReductionPercent, 3,
            new[] { 110, 210, 330 }, new[] { 7, 12, 20 }, new[] { 5, 10, 18 },
            Requirements(Requirement(MaterialType.Wood, 30), Requirement(MaterialType.Foodstuff, 15)),
            Requirements(Requirement(MaterialType.Wood, 50), Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Foodstuff, 25)),
            Requirements(Requirement(MaterialType.Wood, 75), Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Foodstuff, 40)));

        UpsertBuilding("DungeonGate", BuildingEffectType.DungeonRewardPercent, 3,
            new[] { 150, 280, 450 }, new[] { 8, 14, 24 }, new[] { 0, 10, 20 },
            Requirements(Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Magic, 10)),
            Requirements(Requirement(MaterialType.Stone, 60), Requirement(MaterialType.Wood, 35), Requirement(MaterialType.Magic, 20)),
            Requirements(Requirement(MaterialType.Stone, 90), Requirement(MaterialType.Wood, 55), Requirement(MaterialType.Magic, 35)));

        UpsertBuilding("Plaza", BuildingEffectType.HappinessBonus, 3,
            new[] { 90, 170, 270 }, new[] { 5, 9, 15 }, new[] { 5, 10, 18 },
            Requirements(Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Wood, 15)),
            Requirements(Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Wood, 25), Requirement(MaterialType.Foodstuff, 10)),
            Requirements(Requirement(MaterialType.Stone, 55), Requirement(MaterialType.Wood, 40), Requirement(MaterialType.Foodstuff, 20)));

        UpsertBuilding("Tavern", BuildingEffectType.MoraleBonus, 3,
            new[] { 120, 230, 360 }, new[] { 8, 14, 22 }, new[] { 5, 10, 18 },
            Requirements(Requirement(MaterialType.Wood, 35), Requirement(MaterialType.Foodstuff, 20)),
            Requirements(Requirement(MaterialType.Wood, 55), Requirement(MaterialType.Stone, 20), Requirement(MaterialType.Foodstuff, 35)),
            Requirements(Requirement(MaterialType.Wood, 80), Requirement(MaterialType.Stone, 35), Requirement(MaterialType.Foodstuff, 55)));
    }

    private static void CreateGuildAssets()
    {
        UpsertGuild("WarriorGuild", GuildType.Warrior, string.Empty, 5, 80, 10, 5, 3, 1, new[] { GuildAction.Defend, GuildAction.Explore });
        UpsertGuild("MageGuild", GuildType.Mage, "mag_1", 3, 120, 3, 12, 1, 4, new[] { GuildAction.Research, GuildAction.Explore });
        UpsertGuild("CraftsmanGuild", GuildType.Craftsman, "arc_1", 3, 100, 5, 10, 2, 3, new[] { GuildAction.Construct });
    }

    // ---- Research nodes --------------------------------------------------

    private static void CreateResearchNodeAssets()
    {
        // 重要ノード0 — 共通コスト: 50G / F木材40 / F石材25 / 食料50 / 人員1 / 2ターン
        MaterialRequirement[] node0Mat = Requirements(Requirement(MaterialType.Wood, 40), Requirement(MaterialType.Stone, 25));

        UpsertResearchNode("agri_1", "周辺開拓",
            "農地周辺を開拓し、食料生産基盤を整える。",
            "agri_0", ResearchImportantGroupTier.Node0,
            50, 2, 1, 50, node0Mat, null,
            Effects(Effect(ResearchEffectType.UnlockExploration),
                    Effect(ResearchEffectType.AddBaseFoodProduction, 50),
                    Effect(ResearchEffectType.AddFoodProductionPercent, 3)));

        UpsertResearchNode("mil_1", "ギルドとの戦闘協力",
            "戦士ギルドと正式な協力体制を築き、迎撃能力を確保する。",
            "mil_0", ResearchImportantGroupTier.Node0,
            50, 2, 1, 50, node0Mat, null,
            Effects(Effect(ResearchEffectType.UnlockCombatSupport)));

        UpsertResearchNode("mag_1", "魔法ギルドの設立",
            "魔法使いギルドを設立し、魔法研究の道を開く。",
            "mag_0", ResearchImportantGroupTier.Node0,
            50, 2, 1, 50, node0Mat, null,
            Effects(Effect(ResearchEffectType.UnlockGuild, targetId: "Mage")));

        UpsertResearchNode("arc_1", "職人ギルドの設立",
            "職人ギルドを設立し、建設技術の発展を促す。",
            "arc_0", ResearchImportantGroupTier.Node0,
            50, 2, 1, 50, node0Mat, null,
            Effects(Effect(ResearchEffectType.UnlockGuild, targetId: "Craftsman")));

        // 農業カテゴリ
        UpsertResearchNode("agri_2", "灌漑技術",
            "灌漑により農場の収穫量を大幅に増やす。",
            "agri_initial", ResearchImportantGroupTier.Initial,
            180, 4, 1, 0,
            Requirements(Requirement(MaterialType.Wood, 30), Requirement(MaterialType.Stone, 20)),
            new[] { "agri_1" },
            Effects(Effect(ResearchEffectType.AddFoodProductionPercent, 40)));

        UpsertResearchNode("agri_3", "保存技術",
            "食料の長期保存技術を習得し、備蓄上限を拡大する。",
            "agri_initial", ResearchImportantGroupTier.Initial,
            160, 4, 1, 0,
            Requirements(Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Foodstuff, 30)),
            new[] { "agri_1" },
            Effects());

        UpsertResearchNode("agri_4", "大規模農業",
            "農場をさらに拡張可能にする大規模農業技術を確立する。",
            "agri_upper", ResearchImportantGroupTier.Upper,
            360, 7, 2, 0,
            Requirements(Requirement(MaterialType.Wood, 30, MaterialGrade.E), Requirement(MaterialType.Stone, 20, MaterialGrade.E), Requirement(MaterialType.Foodstuff, 50)),
            new[] { "agri_2", "agri_3" },
            Effects(Effect(ResearchEffectType.AddBuildingMaxLevel, 1, targetId: "Farm")));

        // 軍事カテゴリ
        UpsertResearchNode("mil_2", "城壁強化",
            "城壁の構造を改良し、防衛力を大幅に向上させる。",
            "mil_initial", ResearchImportantGroupTier.Initial,
            220, 5, 1, 0,
            Requirements(Requirement(MaterialType.Stone, 40), Requirement(MaterialType.Metal, 20)),
            new[] { "mil_1" },
            Effects(Effect(ResearchEffectType.AddDefensePercent, 20)));

        UpsertResearchNode("mil_3", "戦術理論",
            "組織的な戦術理論を習得し、防衛力全体を底上げする。",
            "mil_initial", ResearchImportantGroupTier.Initial,
            200, 4, 1, 0,
            Requirements(Requirement(MaterialType.Wood, 20), Requirement(MaterialType.Metal, 25)),
            new[] { "mil_1" },
            Effects(Effect(ResearchEffectType.AddDefensePercent, 20)));

        UpsertResearchNode("mil_4", "精鋭部隊",
            "精鋭訓練により戦士ギルドの受け入れ人数を増やす。",
            "mil_upper", ResearchImportantGroupTier.Upper,
            380, 7, 2, 0,
            Requirements(Requirement(MaterialType.Metal, 35, MaterialGrade.E), Requirement(MaterialType.Foodstuff, 20, MaterialGrade.E)),
            new[] { "mil_2", "mil_3" },
            Effects(Effect(ResearchEffectType.AddGuildMaxMembers, 2, targetId: "Warrior")));

        // 魔法カテゴリ
        UpsertResearchNode("mag_2", "探索強化",
            "魔法的手段でダンジョン探索の効率を高める。",
            "mag_initial", ResearchImportantGroupTier.Initial,
            240, 5, 1, 0,
            Requirements(Requirement(MaterialType.Magic, 25), Requirement(MaterialType.Wood, 15)),
            new[] { "mag_1" },
            Effects());

        UpsertResearchNode("mag_3", "予知術",
            "予知魔法により領主の運を高め、有利なイベントを引き寄せる。",
            "mag_initial", ResearchImportantGroupTier.Initial,
            220, 4, 1, 0,
            Requirements(Requirement(MaterialType.Magic, 20), Requirement(MaterialType.Foodstuff, 20)),
            new[] { "mag_1" },
            Effects(Effect(ResearchEffectType.AddLordStat, 5, targetId: "Luck")));

        UpsertResearchNode("mag_4", "上位魔法",
            "高度な魔法理論を修得し、すべての研究速度を高める。",
            "mag_upper", ResearchImportantGroupTier.Upper,
            480, 8, 2, 0,
            Requirements(Requirement(MaterialType.Magic, 40, MaterialGrade.E), Requirement(MaterialType.Metal, 20, MaterialGrade.E)),
            new[] { "mag_2", "mag_3" },
            Effects(Effect(ResearchEffectType.AddResearchSpeedPercent, 20)));

        // 建築カテゴリ
        UpsertResearchNode("arc_2", "高度建築",
            "建築技術を向上させ、すべての施設をさらに強化可能にする。",
            "arc_initial", ResearchImportantGroupTier.Initial,
            300, 6, 1, 0,
            Requirements(Requirement(MaterialType.Wood, 40), Requirement(MaterialType.Stone, 40), Requirement(MaterialType.Metal, 20)),
            new[] { "arc_1" },
            Effects(Effect(ResearchEffectType.AddBuildingMaxLevel, 1)));

        UpsertResearchNode("arc_3", "都市計画",
            "計画的な都市設計により民家の人口収容力を高める。",
            "arc_initial", ResearchImportantGroupTier.Initial,
            220, 5, 1, 0,
            Requirements(Requirement(MaterialType.Wood, 35), Requirement(MaterialType.Stone, 25)),
            new[] { "arc_1" },
            Effects());

        UpsertResearchNode("arc_4", "名匠の技",
            "職人の熟練技術で素材生産量を大幅に増加させる。",
            "arc_upper", ResearchImportantGroupTier.Upper,
            420, 7, 2, 0,
            Requirements(Requirement(MaterialType.Wood, 30, MaterialGrade.E), Requirement(MaterialType.Stone, 30, MaterialGrade.E), Requirement(MaterialType.Metal, 25, MaterialGrade.E)),
            new[] { "arc_2", "arc_3" },
            Effects(Effect(ResearchEffectType.AddMaterialProductionPercent, 30)));
    }

    private static void UpsertResearchNode(string nodeId, string displayName, string description, string importantResearchGroupId, ResearchImportantGroupTier importantGroupTier, int costFunds, int durationTurns, int requiredWorkers, int requiredFood, MaterialRequirement[] materialRequirements, string[] prerequisiteNodeIds, ResearchEffect[] effects)
    {
        string path = ResearchFolder + "/" + nodeId + ".asset";
        ResearchNodeData asset = AssetDatabase.LoadAssetAtPath<ResearchNodeData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ResearchNodeData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.nodeId = nodeId;
        asset.displayName = displayName;
        asset.description = description;
        asset.importantResearchGroupId = importantResearchGroupId;
        asset.importantGroupTier = importantGroupTier;
        asset.researchCostFunds = costFunds;
        asset.researchDurationTurns = durationTurns;
        asset.requiredWorkers = requiredWorkers;
        asset.requiredFood = requiredFood;
        asset.materialRequirements = materialRequirements;
        asset.prerequisiteNodeIds = prerequisiteNodeIds ?? new string[0];
        asset.effects = effects;
        EditorUtility.SetDirty(asset);
    }

    private static ResearchEffect Effect(ResearchEffectType type, int intValue = 0, string targetId = "", float floatValue = 0f)
    {
        ResearchEffect e = new ResearchEffect();
        e.effectType = type;
        e.intValue = intValue;
        e.targetId = targetId;
        e.floatValue = floatValue;
        return e;
    }

    private static ResearchEffect[] Effects(params ResearchEffect[] effects)
    {
        return effects;
    }

    // ---- Building / Guild upsert helpers ---------------------------------

    private static void UpsertBuilding(string assetName, BuildingEffectType effectType, int maxLevel, int[] buildCostFunds, int[] maintenanceCostFunds, int[] effectValues, MaterialRequirement[] level1Requirements, MaterialRequirement[] level2Requirements, MaterialRequirement[] level3Requirements)
    {
        string path = BuildingsFolder + "/" + assetName + ".asset";
        BuildingData asset = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<BuildingData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.buildingName = assetName;
        asset.effectType = effectType;
        asset.maxLevel = maxLevel;
        asset.buildCostFunds = buildCostFunds;
        asset.maintenanceCostFunds = maintenanceCostFunds;
        asset.effectValues = effectValues;
        asset.level1MaterialRequirements = level1Requirements;
        asset.level2MaterialRequirements = level2Requirements;
        asset.level3MaterialRequirements = level3Requirements;
        asset.buildCostMaterials = new int[0];
        EditorUtility.SetDirty(asset);
    }

    private static void UpsertGuild(string assetName, GuildType guildType, string unlockResearchNodeId, int maxMembers, int hireCostFunds, int baseCombatPower, int baseSkillPower, int combatPowerGrowth, int skillPowerGrowth, GuildAction[] specializedActions)
    {
        string path = GuildsFolder + "/" + assetName + ".asset";
        GuildData asset = AssetDatabase.LoadAssetAtPath<GuildData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<GuildData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.guildName = assetName;
        asset.guildType = guildType;
        asset.unlockResearchNodeId = unlockResearchNodeId;
        asset.maxMembers = maxMembers;
        asset.hireCostFunds = hireCostFunds;
        asset.baseCombatPower = baseCombatPower;
        asset.baseSkillPower = baseSkillPower;
        asset.combatPowerGrowth = combatPowerGrowth;
        asset.skillPowerGrowth = skillPowerGrowth;
        asset.specializedActions = specializedActions;
        EditorUtility.SetDirty(asset);
    }

    private static MaterialRequirement Requirement(MaterialType type, int amount, MaterialGrade minimumGrade = MaterialGrade.F)
    {
        MaterialRequirement requirement = new MaterialRequirement();
        requirement.Type = type;
        requirement.MinimumGrade = minimumGrade;
        requirement.Amount = amount;
        return requirement;
    }

    private static MaterialRequirement[] Requirements(params MaterialRequirement[] requirements)
    {
        return requirements;
    }

    private static void EnsureFolder(string parentFolder, string childFolder)
    {
        string fullPath = parentFolder + "/" + childFolder;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static UnityEngine.Object[] FindAssetsByType<T>(string folder) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
        UnityEngine.Object[] assets = new UnityEngine.Object[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return assets;
    }

    private static void AssignObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void AssignArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
