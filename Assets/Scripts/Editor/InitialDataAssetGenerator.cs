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
    private const string EventsFolder = RootFolder + "/Events";
    private const string MetaFolder = RootFolder + "/Meta";
    private const string DefaultTmpFontAssetPath = "Assets/Fonts/NotoSansJP-VariableFont_wght_0 SDF.asset";

    [MenuItem("repoas/Create Initial Data Assets")]
    public static void CreateInitialDataAssets()
    {
        EnsureFolder("Assets", "ScriptableObjects");
        EnsureFolder(RootFolder, "Buildings");
        EnsureFolder(RootFolder, "Guilds");
        EnsureFolder(RootFolder, "Research");
        EnsureFolder(RootFolder, "Events");
        EnsureFolder(RootFolder, "Meta");

        CreateBuildingAssets();
        CreateGuildAssets();
        CreateResearchNodeAssets();
        CreateEventAssets();
        CreateMetaSkillAssets();

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
        AssignArray(serializedBootstrap, "researchCatalog", FindAssetsByType<ResearchNodeData>(ResearchFolder));
        AssignArray(serializedBootstrap, "availableBuildings", FindAssetsByType<BuildingData>(BuildingsFolder));
        AssignArray(serializedBootstrap, "startingBuildings", new UnityEngine.Object[]
        {
            LoadAsset<BuildingData>(BuildingsFolder + "/DungeonGate.asset")
        });
        AssignArray(serializedBootstrap, "eventCatalog", FindAssetsByType<EventData>(EventsFolder));
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
        ResourcePanel resourcePanel = UnityEngine.Object.FindFirstObjectByType<ResourcePanel>();
        PopulationAssignmentPanel populationAssignmentPanel = UnityEngine.Object.FindFirstObjectByType<PopulationAssignmentPanel>();
        ResearchPanel researchPanel = UnityEngine.Object.FindFirstObjectByType<ResearchPanel>();
        ExplorationPanel explorationPanel = UnityEngine.Object.FindFirstObjectByType<ExplorationPanel>();
        BuildPanel buildPanel = UnityEngine.Object.FindFirstObjectByType<BuildPanel>();
        GuildPanel guildPanel = UnityEngine.Object.FindFirstObjectByType<GuildPanel>();
        HappinessPanel happinessPanel = UnityEngine.Object.FindFirstObjectByType<HappinessPanel>();
        MapPanel mapPanel = UnityEngine.Object.FindFirstObjectByType<MapPanel>();
        RaidPopup raidPopup = UnityEngine.Object.FindFirstObjectByType<RaidPopup>();
        MetaScreen metaScreen = UnityEngine.Object.FindFirstObjectByType<MetaScreen>();

        if (mainScreen != null)
        {
            SerializedObject so = new SerializedObject(mainScreen);
            wired += EnsureTmpTextInChildren(so, "turnText", mainScreen.gameObject, "TurnText");
            wired += EnsureTmpTextInChildren(so, "gameStateText", mainScreen.gameObject, "GameStateText");
            AssignObjectReference(so, "resourcePanel", resourcePanel);
            AssignObjectReference(so, "populationAssignmentPanel", populationAssignmentPanel);
            AssignObjectReference(so, "researchPanel", researchPanel);
            AssignObjectReference(so, "explorationPanel", explorationPanel);
            AssignObjectReference(so, "buildPanel", buildPanel);
            AssignObjectReference(so, "guildPanel", guildPanel);
            AssignObjectReference(so, "happinessPanel", happinessPanel);
            AssignObjectReference(so, "raidPopup", raidPopup);
            AssignObjectReference(so, "metaScreen", metaScreen);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mainScreen);
        }

        if (resourcePanel != null)
        {
            SerializedObject so = new SerializedObject(resourcePanel);
            wired += EnsureTmpTextInChildren(so, "summaryText", resourcePanel.gameObject, "SummaryText");
            wired += EnsureTmpTextInChildren(so, "materialsText", resourcePanel.gameObject, "MaterialsText");
            wired += EnsureTmpTextInChildren(so, "warningText", resourcePanel.gameObject, "WarningText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resourcePanel);
        }

        if (populationAssignmentPanel != null)
        {
            SerializedObject so = new SerializedObject(populationAssignmentPanel);
            wired += AssignTmpTextInScene(so, "assignmentSummaryText", "AssignmentSummaryText");
            wired += AssignTmpTextInScene(so, "foodWorkersValueText", "FoodWorkersValueText");
            wired += AssignTmpTextInScene(so, "fundsWorkersValueText", "FundsWorkersValueText");
            wired += AssignTmpTextInScene(so, "developmentWorkersValueText", "DevelopmentWorkersValueText");
            wired += AssignSliderInChildren(so, "foodWorkersSlider", populationAssignmentPanel, "FoodWorkersSlider");
            wired += AssignSliderInChildren(so, "fundsWorkersSlider", populationAssignmentPanel, "FundsWorkersSlider");
            wired += AssignSliderInChildren(so, "developmentWorkersSlider", populationAssignmentPanel, "DevelopmentWorkersSlider");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(populationAssignmentPanel);
        }

        if (guildPanel != null)
        {
            SerializedObject so = new SerializedObject(guildPanel);
            wired += AssignTmpTextInScene(so, "guildSummaryText", "GuildSummaryText");
            wired += AssignTmpTextInScene(so, "selectedGuildText", "SelectedGuildText");
            wired += AssignTmpTextInScene(so, "memberListText", "MemberListText");
            wired += AssignTmpTextInScene(so, "selectedMemberText", "SelectedMemberText");
            wired += AssignTmpTextInScene(so, "actionResultText", "ActionResultText");
            wired += AssignButtonInChildren(so, "previousGuildButton", guildPanel, "PreviousGuildButton");
            wired += AssignButtonInChildren(so, "nextGuildButton", guildPanel, "NextGuildButton");
            wired += AssignButtonInChildren(so, "hireButton", guildPanel, "HireButton");
            wired += AssignButtonInChildren(so, "previousMemberButton", guildPanel, "PreviousMemberButton");
            wired += AssignButtonInChildren(so, "nextMemberButton", guildPanel, "NextMemberButton");
            wired += AssignButtonInChildren(so, "idleButton", guildPanel, "IdleButton");
            wired += AssignButtonInChildren(so, "defendButton", guildPanel, "DefendButton");
            wired += AssignButtonInChildren(so, "exploreButton", guildPanel, "ExploreButton");
            wired += AssignButtonInChildren(so, "researchButton", guildPanel, "ResearchButton");
            wired += AssignButtonInChildren(so, "constructButton", guildPanel, "ConstructButton");
            wired += AssignButtonInChildren(so, "developButton", guildPanel, "DevelopButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(guildPanel);
        }

        if (explorationPanel != null)
        {
            SerializedObject so = new SerializedObject(explorationPanel);
            wired += EnsureTmpTextInChildren(so, "statusText", explorationPanel.gameObject, "StatusText");
            wired += EnsureTmpTextInChildren(so, "resultText", explorationPanel.gameObject, "ResultText");
            wired += AssignButtonInChildren(so, "exploreButton", explorationPanel, "exploreButton");
            wired += AssignButtonInChildren(so, "startDungeonButton", explorationPanel, "StartDungeonButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(explorationPanel);
        }

        if (mapPanel != null)
        {
            SerializedObject so = new SerializedObject(mapPanel);
            wired += EnsureTmpTextInChildren(so, "mapText", mapPanel.gameObject, "MapText");
            wired += EnsureTmpTextInChildren(so, "legendText", mapPanel.gameObject, "LegendText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mapPanel);
        }

        if (buildPanel != null)
        {
            SerializedObject so = new SerializedObject(buildPanel);
            wired += AssignTmpTextInScene(so, "ownedBuildingsText", "OwnedBuildingsText");
            wired += AssignTmpTextInScene(so, "availableBuildingsText", "AvailableBuildingsText");
            wired += AssignTmpTextInScene(so, "buildQueueText", "BuildQueueText");
            AssignArray(so, "availableBuildings", FindAssetsByType<BuildingData>(BuildingsFolder));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(buildPanel);
        }

        if (researchPanel != null)
        {
            SerializedObject so = new SerializedObject(researchPanel);
            wired += AssignTmpTextInScene(so, "activeResearchText", "ActiveResearchText");
            wired += AssignTmpTextInScene(so, "completedResearchText", "CompletedResearchText");
            wired += AssignTmpTextInScene(so, "availableResearchText", "AvailableResearchText");
            AssignArray(so, "availableNodes", FindAssetsByType<ResearchNodeData>(ResearchFolder));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(researchPanel);
        }

        if (happinessPanel != null)
        {
            SerializedObject so = new SerializedObject(happinessPanel);
            wired += AssignTmpTextInScene(so, "happinessText", "HappinessText");
            wired += AssignTmpTextInScene(so, "detailText", "HappinessDetailText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(happinessPanel);
        }

        if (metaScreen != null)
        {
            SerializedObject so = new SerializedObject(metaScreen);
            wired += AssignTmpTextInScene(so, "scoreText", "ScoreText");
            wired += AssignTmpTextInScene(so, "metaPointText", "MetaPointText");
            wired += AssignTmpTextInScene(so, "lordStatsText", "LordStatsText");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(metaScreen);
        }

        if (raidPopup != null)
        {
            SerializedObject so = new SerializedObject(raidPopup);
            Transform rootChild = raidPopup.transform.Find("Root");
            if (rootChild == null)
            {
                GameObject rootObject = new GameObject("Root");
                rootObject.transform.SetParent(raidPopup.transform, false);
                rootObject.AddComponent<RectTransform>();
                rootChild = rootObject.transform;
            }

            AssignObjectReference(so, "root", rootChild.gameObject);
            wired++;

            wired += EnsureTmpTextInChildren(so, "titleText", rootChild.gameObject, "RaidTitleText");
            wired += EnsureTmpTextInChildren(so, "detailText", rootChild.gameObject, "RaidDetailText");
            wired += AssignButtonInChildren(so, "closeButton", raidPopup, "CloseButton");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(raidPopup);
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log($"[Wire] Done. {wired} references auto-assigned.");
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
        PopulationAssignmentPanel populationAssignmentPanel = UnityEngine.Object.FindFirstObjectByType<PopulationAssignmentPanel>();
        if (populationAssignmentPanel == null)
        {
            GameObject panelObject = new GameObject("PopulationAssignmentPanelObject");
            panelObject.transform.SetParent(canvas.transform, false);
            panelObject.AddComponent<RectTransform>();
            populationAssignmentPanel = panelObject.AddComponent<PopulationAssignmentPanel>();
        }
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
        LayoutPopulationAssignmentPanel(populationAssignmentPanel);
        LayoutResearchPanel(researchPanel);
        LayoutExplorationPanel(explorationPanel);
        LayoutBuildPanel(buildPanel);
        LayoutGuildPanel(guildPanel);
        EnsureExtendedBuildPanelControls(buildPanel);
        EnsureExtendedGuildPanelControls(guildPanel);
        EnsureGuildDevelopButton(guildPanel);
        LayoutHappinessPanel(happinessPanel);
        LayoutMapPanel(mapPanel);
        LayoutRaidPopup(raidPopup);
        LayoutMetaScreen(metaScreen);

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log("Visible UI layout applied.");
    }

    // ---- Scene search helpers --------------------------------------------

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

    private static int EnsureTmpTextInChildren(SerializedObject so, string fieldName, GameObject parent, string gameObjectName)
    {
        TMP_Text[] texts = parent.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (string.Equals(text.gameObject.name, gameObjectName, StringComparison.OrdinalIgnoreCase))
            {
                AssignObjectReference(so, fieldName, text);
                return 1;
            }
        }

        GameObject child = new GameObject(gameObjectName);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        TextMeshProUGUI created = child.AddComponent<TextMeshProUGUI>();
        created.fontSize = 16;
        created.color = Color.white;
        created.textWrappingMode = TextWrappingModes.Normal;
        AssignObjectReference(so, fieldName, created);
        EditorUtility.SetDirty(child);
        return 1;
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

    private static int AssignSliderInChildren(SerializedObject so, string fieldName, Component parent, string gameObjectName)
    {
        Slider[] sliders = parent.GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            if (string.Equals(slider.gameObject.name, gameObjectName, StringComparison.OrdinalIgnoreCase))
            {
                AssignObjectReference(so, fieldName, slider);
                return 1;
            }
        }

        Debug.LogWarning($"[Wire] Slider '{gameObjectName}' not found under {parent.gameObject.name} (field: {fieldName}).");
        return 0;
    }

    // ---- Child creation helpers ------------------------------------------

    private static void DeduplicateDirectChildren(GameObject parent)
    {
        System.Collections.Generic.HashSet<string> seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        System.Collections.Generic.List<GameObject> toDestroy = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            string name = parent.transform.GetChild(i).gameObject.name;
            if (!seen.Add(name))
            {
                toDestroy.Add(parent.transform.GetChild(i).gameObject);
            }
        }

        foreach (GameObject obj in toDestroy)
        {
            Debug.Log($"[Layout] 重複子オブジェクトを削除: {parent.name}/{obj.name}");
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    private static Transform FindDirectChildInsensitive(GameObject parent, string childName)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            if (string.Equals(child.name, childName, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }

        return null;
    }

    private static GameObject EnsureChildGameObject(GameObject parent, string childName)
    {
        Transform existing = FindDirectChildInsensitive(parent, childName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        return child;
    }

    private static TMP_Text EnsureAndAssignTmpText(SerializedObject so, string propertyName, GameObject parent, string childName, Vector2 pos, Vector2 size, int fontSize = 16, bool darkText = false)
    {
        Transform existingTransform = FindDirectChildInsensitive(parent, childName);
        TMP_Text tmpText = existingTransform != null ? existingTransform.GetComponent<TMP_Text>() : null;
        if (tmpText == null)
        {
            if (existingTransform != null)
            {
                UnityEngine.Object.DestroyImmediate(existingTransform.gameObject);
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);
            TextMeshProUGUI created = child.AddComponent<TextMeshProUGUI>();
            created.textWrappingMode = TextWrappingModes.Normal;
            tmpText = created;
        }

        ApplyDefaultTmpFont(tmpText);
        tmpText.fontSize = fontSize;
        tmpText.color = darkText ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
        SetRect(tmpText.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        AssignObjectReference(so, propertyName, tmpText);
        EditorUtility.SetDirty(tmpText.gameObject);
        return tmpText;
    }

    private static Button EnsureAndAssignButton(SerializedObject so, string propertyName, GameObject parent, string childName, string label, Vector2 pos, Vector2 size)
    {
        Transform existingTransform = FindDirectChildInsensitive(parent, childName);
        Button button = existingTransform != null ? existingTransform.GetComponent<Button>() : null;
        if (button == null)
        {
            if (existingTransform != null)
            {
                UnityEngine.Object.DestroyImmediate(existingTransform.gameObject);
            }

            GameObject btnObj = new GameObject(childName);
            btnObj.transform.SetParent(parent.transform, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
            button = btnObj.AddComponent<Button>();
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 16;
            labelText.color = Color.black;
            SetRect(labelText.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            EditorUtility.SetDirty(labelObj);
        }

        SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        AssignObjectReference(so, propertyName, button);
        EditorUtility.SetDirty(button.gameObject);
        return button;
    }

    private static Slider EnsureAndAssignSlider(SerializedObject so, string propertyName, GameObject parent, string childName, Vector2 pos, Vector2 size)
    {
        Transform existingTransform = FindDirectChildInsensitive(parent, childName);
        Slider slider = existingTransform != null ? existingTransform.GetComponent<Slider>() : null;
        if (slider == null)
        {
            GameObject sliderObj = new GameObject(childName);
            sliderObj.transform.SetParent(parent.transform, false);

            Image background = sliderObj.AddComponent<Image>();
            background.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);
            slider = sliderObj.AddComponent<Slider>();

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            SetRect(fillAreaRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 5f), new Vector2(-20f, -10f), true);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.7f, 0.35f, 0.95f);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            SetRect(fillRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            SetRect(handleAreaRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(-10f, 0f), true);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, 28f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;
        }

        SetRect(slider.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        AssignObjectReference(so, propertyName, slider);
        EditorUtility.SetDirty(slider.gameObject);
        return slider;
    }

    // ---- Layout sub-methods ----------------------------------------------

    private static void LayoutMainGameScreen(MainGameScreen mainScreen)
    {
        if (mainScreen == null)
        {
            return;
        }

        DeduplicateDirectChildren(mainScreen.gameObject);
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

        DeduplicateDirectChildren(mapPanel.gameObject);
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

        DeduplicateDirectChildren(resourcePanel.gameObject);
        SetRect(resourcePanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-460f, -20f), new Vector2(430f, 180f));

        SerializedObject so = new SerializedObject(resourcePanel);
        LayoutTextField(so, "summaryText", new Vector2(0f, 0f), new Vector2(430f, 40f), TextAnchor.MiddleLeft, 20);
        LayoutTextField(so, "materialsText", new Vector2(0f, -45f), new Vector2(430f, 100f), TextAnchor.UpperLeft, 16);
        LayoutTextField(so, "warningText", new Vector2(0f, -150f), new Vector2(430f, 60f), TextAnchor.UpperLeft, 16, Color.red);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(resourcePanel);
    }

    private static void LayoutPopulationAssignmentPanel(PopulationAssignmentPanel populationAssignmentPanel)
    {
        if (populationAssignmentPanel == null)
        {
            return;
        }

        DeduplicateDirectChildren(populationAssignmentPanel.gameObject);
        SetRect(populationAssignmentPanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-460f, -210f), new Vector2(430f, 210f));

        SerializedObject so = new SerializedObject(populationAssignmentPanel);
        EnsureAndAssignTmpText(so, "assignmentSummaryText", populationAssignmentPanel.gameObject, "AssignmentSummaryText", new Vector2(0f, 0f), new Vector2(430f, 28f), 15);
        EnsureAndAssignTmpText(so, "foodWorkersValueText", populationAssignmentPanel.gameObject, "FoodWorkersValueText", new Vector2(0f, -35f), new Vector2(430f, 24f), 14);
        EnsureAndAssignSlider(so, "foodWorkersSlider", populationAssignmentPanel.gameObject, "FoodWorkersSlider", new Vector2(0f, -61f), new Vector2(430f, 24f));
        EnsureAndAssignTmpText(so, "fundsWorkersValueText", populationAssignmentPanel.gameObject, "FundsWorkersValueText", new Vector2(0f, -93f), new Vector2(430f, 24f), 14);
        EnsureAndAssignSlider(so, "fundsWorkersSlider", populationAssignmentPanel.gameObject, "FundsWorkersSlider", new Vector2(0f, -119f), new Vector2(430f, 24f));
        EnsureAndAssignTmpText(so, "developmentWorkersValueText", populationAssignmentPanel.gameObject, "DevelopmentWorkersValueText", new Vector2(0f, -151f), new Vector2(430f, 24f), 14);
        EnsureAndAssignSlider(so, "developmentWorkersSlider", populationAssignmentPanel.gameObject, "DevelopmentWorkersSlider", new Vector2(0f, -177f), new Vector2(430f, 24f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(populationAssignmentPanel);
    }

    private static void LayoutExplorationPanel(ExplorationPanel explorationPanel)
    {
        if (explorationPanel == null)
        {
            return;
        }

        DeduplicateDirectChildren(explorationPanel.gameObject);
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

        DeduplicateDirectChildren(guildPanel.gameObject);
        SetRect(guildPanel.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-560f, -120f), new Vector2(520f, 520f));

        SerializedObject so = new SerializedObject(guildPanel);
        EnsureAndAssignTmpText(so, "guildSummaryText", guildPanel.gameObject, "GuildSummaryText", new Vector2(0f, 0f), new Vector2(520f, 120f), 17);
        EnsureAndAssignTmpText(so, "memberListText", guildPanel.gameObject, "MemberListText", new Vector2(0f, -125f), new Vector2(520f, 210f), 15);
        EnsureAndAssignTmpText(so, "selectedMemberText", guildPanel.gameObject, "SelectedMemberText", new Vector2(0f, -340f), new Vector2(520f, 32f), 16);
        EnsureAndAssignTmpText(so, "actionResultText", guildPanel.gameObject, "ActionResultText", new Vector2(0f, -378f), new Vector2(520f, 42f), 15);
        EnsureAndAssignButton(so, "previousMemberButton", guildPanel.gameObject, "PreviousMemberButton", "前", new Vector2(0f, -430f), new Vector2(60f, 36f));
        EnsureAndAssignButton(so, "nextMemberButton", guildPanel.gameObject, "NextMemberButton", "次", new Vector2(70f, -430f), new Vector2(60f, 36f));
        EnsureAndAssignButton(so, "idleButton", guildPanel.gameObject, "IdleButton", "待機", new Vector2(0f, -472f), new Vector2(72f, 36f));
        EnsureAndAssignButton(so, "defendButton", guildPanel.gameObject, "DefendButton", "防衛", new Vector2(80f, -472f), new Vector2(72f, 36f));
        EnsureAndAssignButton(so, "exploreButton", guildPanel.gameObject, "ExploreButton", "探索", new Vector2(160f, -472f), new Vector2(72f, 36f));
        EnsureAndAssignButton(so, "researchButton", guildPanel.gameObject, "ResearchButton", "研究", new Vector2(240f, -472f), new Vector2(72f, 36f));
        EnsureAndAssignButton(so, "constructButton", guildPanel.gameObject, "ConstructButton", "建設", new Vector2(320f, -472f), new Vector2(72f, 36f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(guildPanel);
    }

    private static void LayoutBuildPanel(BuildPanel buildPanel)
    {
        if (buildPanel == null)
        {
            return;
        }

        DeduplicateDirectChildren(buildPanel.gameObject);
        SetRect(buildPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-250f, 20f), new Vector2(480f, 220f));

        SerializedObject so = new SerializedObject(buildPanel);
        EnsureAndAssignTmpText(so, "ownedBuildingsText", buildPanel.gameObject, "OwnedBuildingsText", new Vector2(0f, 0f), new Vector2(480f, 70f), 15);
        EnsureAndAssignTmpText(so, "availableBuildingsText", buildPanel.gameObject, "AvailableBuildingsText", new Vector2(0f, -75f), new Vector2(480f, 100f), 14);
        EnsureAndAssignTmpText(so, "buildQueueText", buildPanel.gameObject, "BuildQueueText", new Vector2(0f, -180f), new Vector2(480f, 40f), 15);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(buildPanel);
    }

    private static void EnsureGuildDevelopButton(GuildPanel guildPanel)
    {
        if (guildPanel == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(guildPanel);
        EnsureAndAssignButton(so, "developButton", guildPanel.gameObject, "DevelopButton", "開拓", new Vector2(400f, -472f), new Vector2(72f, 36f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(guildPanel);
    }

    private static void EnsureExtendedGuildPanelControls(GuildPanel guildPanel)
    {
        if (guildPanel == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(guildPanel);
        EnsureAndAssignTmpText(so, "selectedGuildText", guildPanel.gameObject, "SelectedGuildText", new Vector2(0f, -125f), new Vector2(520f, 32f), 15);
        EnsureAndAssignButton(so, "previousGuildButton", guildPanel.gameObject, "PreviousGuildButton", "前ギルド", new Vector2(0f, -424f), new Vector2(90f, 36f));
        EnsureAndAssignButton(so, "nextGuildButton", guildPanel.gameObject, "NextGuildButton", "次ギルド", new Vector2(98f, -424f), new Vector2(90f, 36f));
        EnsureAndAssignButton(so, "hireButton", guildPanel.gameObject, "HireButton", "加入", new Vector2(196f, -424f), new Vector2(72f, 36f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(guildPanel);
    }

    private static void EnsureExtendedBuildPanelControls(BuildPanel buildPanel)
    {
        if (buildPanel == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(buildPanel);
        EnsureAndAssignTmpText(so, "selectedBuildingText", buildPanel.gameObject, "SelectedBuildingText", new Vector2(230f, -68f), new Vector2(250f, 106f), 14);
        EnsureAndAssignTmpText(so, "actionResultText", buildPanel.gameObject, "BuildActionResultText", new Vector2(0f, -222f), new Vector2(480f, 32f), 14);
        EnsureAndAssignButton(so, "previousBuildingButton", buildPanel.gameObject, "PreviousBuildingButton", "前", new Vector2(0f, -260f), new Vector2(60f, 36f));
        EnsureAndAssignButton(so, "nextBuildingButton", buildPanel.gameObject, "NextBuildingButton", "次", new Vector2(68f, -260f), new Vector2(60f, 36f));
        EnsureAndAssignButton(so, "buildOrUpgradeButton", buildPanel.gameObject, "BuildOrUpgradeButton", "建設/強化", new Vector2(136f, -260f), new Vector2(120f, 36f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(buildPanel);
    }

    private static void LayoutResearchPanel(ResearchPanel researchPanel)
    {
        if (researchPanel == null)
        {
            return;
        }

        DeduplicateDirectChildren(researchPanel.gameObject);
        SetRect(researchPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-240f, -20f), new Vector2(480f, 230f));

        SerializedObject so = new SerializedObject(researchPanel);
        EnsureAndAssignTmpText(so, "activeResearchText", researchPanel.gameObject, "ActiveResearchText", new Vector2(0f, 0f), new Vector2(480f, 80f), 15);
        EnsureAndAssignTmpText(so, "completedResearchText", researchPanel.gameObject, "CompletedResearchText", new Vector2(0f, -85f), new Vector2(480f, 50f), 14);
        EnsureAndAssignTmpText(so, "availableResearchText", researchPanel.gameObject, "AvailableResearchText", new Vector2(0f, -140f), new Vector2(480f, 90f), 14);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(researchPanel);
    }

    private static void LayoutHappinessPanel(HappinessPanel happinessPanel)
    {
        if (happinessPanel == null)
        {
            return;
        }

        DeduplicateDirectChildren(happinessPanel.gameObject);
        // ResourcePanel ends at row 200; PopulationAssignmentPanel at 210-420; HappinessPanel goes below at 430.
        SetRect(happinessPanel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-460f, -430f), new Vector2(430f, 100f));

        SerializedObject so = new SerializedObject(happinessPanel);
        EnsureAndAssignTmpText(so, "happinessText", happinessPanel.gameObject, "HappinessText", new Vector2(4f, -4f), new Vector2(422f, 36f), 20, darkText: true);
        EnsureAndAssignTmpText(so, "detailText", happinessPanel.gameObject, "HappinessDetailText", new Vector2(4f, -44f), new Vector2(422f, 56f), 15, darkText: true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(happinessPanel);
    }

    private static void LayoutRaidPopup(RaidPopup raidPopup)
    {
        if (raidPopup == null)
        {
            return;
        }

        SetRect(raidPopup.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -120f), new Vector2(400f, 220f));

        GameObject root = EnsureChildGameObject(raidPopup.gameObject, "Root");
        RectTransform rootRt = root.GetComponent<RectTransform>();
        SetRect(rootRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
        if (!Application.isPlaying)
        {
            root.SetActive(false);
        }

        SerializedObject so = new SerializedObject(raidPopup);
        AssignObjectReference(so, "root", root);
        EnsureAndAssignTmpText(so, "titleText", root, "RaidTitleText", new Vector2(0f, 0f), new Vector2(400f, 50f), 20);
        EnsureAndAssignTmpText(so, "detailText", root, "RaidDetailText", new Vector2(0f, -55f), new Vector2(400f, 80f), 16);
        EnsureAndAssignButton(so, "closeButton", root, "CloseButton", "閉じる", new Vector2(150f, -150f), new Vector2(100f, 40f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(raidPopup);
    }

    private static void LayoutMetaScreen(MetaScreen metaScreen)
    {
        if (metaScreen == null)
        {
            return;
        }

        DeduplicateDirectChildren(metaScreen.gameObject);
        SetRect(metaScreen.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-460f, 20f), new Vector2(430f, 180f));

        SerializedObject so = new SerializedObject(metaScreen);
        EnsureAndAssignTmpText(so, "scoreText", metaScreen.gameObject, "ScoreText", new Vector2(0f, 0f), new Vector2(430f, 36f), 20, darkText: true);
        EnsureAndAssignTmpText(so, "metaPointText", metaScreen.gameObject, "MetaPointText", new Vector2(0f, -40f), new Vector2(430f, 36f), 16, darkText: true);
        EnsureAndAssignTmpText(so, "lordStatsText", metaScreen.gameObject, "LordStatsText", new Vector2(0f, -80f), new Vector2(430f, 100f), 15, darkText: true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(metaScreen);
    }

    // ---- Layout field helpers --------------------------------------------

    private static void LayoutTextField(SerializedObject so, string propertyName, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize)
    {
        LayoutTextField(so, propertyName, anchoredPosition, size, anchor, fontSize, Color.white);
    }

    private static void LayoutTextField(SerializedObject so, string propertyName, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize, Color color)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == null)
        {
            return;
        }

        TMP_Text tmpText = property.objectReferenceValue as TMP_Text;
        if (tmpText != null)
        {
            ApplyDefaultTmpFont(tmpText);
            SetRect(tmpText.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            EditorUtility.SetDirty(tmpText);
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

    private static void ApplyDefaultTmpFont(TMP_Text tmpText)
    {
        if (tmpText == null)
        {
            return;
        }

        TMP_FontAsset fontAsset = GetDefaultTmpFontAsset();
        if (fontAsset == null)
        {
            return;
        }

        if (tmpText.font != fontAsset)
        {
            tmpText.font = fontAsset;
        }
    }

    private static TMP_FontAsset GetDefaultTmpFontAsset()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultTmpFontAssetPath);
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

    // ---- ScriptableObject creation ---------------------------------------

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
        UpsertGuild("CraftsmanGuild", GuildType.Craftsman, "arc_1", 3, 100, 5, 10, 2, 3, new[] { GuildAction.Construct, GuildAction.Develop });
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
            Effects(Effect(ResearchEffectType.AddFoodProductionPercent, 15)));

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
            Effects(Effect(ResearchEffectType.AddDungeonFloorSpeedBonus, 1)));

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
            Effects(Effect(ResearchEffectType.AddHouseCapacityPercent, 50)));

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

    // ---- Event assets ----------------------------------------------------

    private static void CreateEventAssets()
    {
        CreateEvent("e_harvest_good", "豊作", "今年は各地で作物が豊富に実った。",
            0.08f, 5, null,
            E(EventEffectType.AddFood, 400));

        CreateEvent("e_drought", "干ばつ", "長引く旱魃が農作物に深刻な打撃を与えた。",
            0.06f, 8, null,
            E(EventEffectType.AddFood, -300));

        CreateEvent("e_flood", "洪水", "大雨による洪水で農地と蓄えが被害を受けた。",
            0.05f, 8, null,
            E(EventEffectType.AddFood, -200),
            E(EventEffectType.AddFunds, -50));

        CreateEvent("e_merchant", "交易商人来訪", "遠方から商人が訪れ、取引で資金が増えた。",
            0.10f, 5, null,
            E(EventEffectType.AddFunds, 150));

        CreateEvent("e_migrants", "移住者の波", "高い幸福度に惹かれ、各地から人々が移り住んできた。",
            0.15f, 6, new[] { "happiness_high" },
            E(EventEffectType.AddPopulation, 20));

        CreateEvent("e_discontent", "民衆の不満", "不満が高まり、抗議活動と働き手の離脱が起きた。",
            0.20f, 9, new[] { "happiness_low" },
            E(EventEffectType.AddHappiness, -10),
            E(EventEffectType.AddFunds, -80));

        CreateEvent("e_epidemic", "疫病の流行", "原因不明の疫病が広まり、多くの住民が倒れた。",
            0.04f, 10, null,
            E(EventEffectType.AddPopulation, -30));

        CreateEvent("e_materials_found", "素材発見", "近隣の森で良質な木材が大量に発見された。",
            0.08f, 5, null,
            EM(MaterialType.Wood, MaterialGrade.F, 60));
    }

    private static void CreateEvent(string eventId, string displayName, string description,
        float probability, int priority, string[] prerequisites, params EventEffect[] effects)
    {
        string path = EventsFolder + "/" + eventId + ".asset";
        EventData existing = AssetDatabase.LoadAssetAtPath<EventData>(path);
        if (existing != null)
        {
            return;
        }

        EventData data = ScriptableObject.CreateInstance<EventData>();
        data.eventId = eventId;
        data.displayName = displayName;
        data.description = description;
        data.baseProbability = probability;
        data.priority = priority;
        data.prerequisiteConditions = prerequisites ?? Array.Empty<string>();
        data.effects = effects;
        data.choices = Array.Empty<EventChoice>();

        AssetDatabase.CreateAsset(data, path);
    }

    private static EventEffect E(EventEffectType type, int amount)
    {
        return new EventEffect { effectType = type, amount = amount };
    }

    private static EventEffect EM(MaterialType materialType, MaterialGrade grade, int amount)
    {
        return new EventEffect
        {
            effectType = EventEffectType.AddMaterial,
            materialType = materialType,
            materialGrade = grade,
            amount = amount
        };
    }

    // ---- Meta skill assets -----------------------------------------------

    private static void CreateMetaSkillAssets()
    {
        UpsertMetaSkill("meta_food", "初期食料増加", "周回開始時の食料を増やす。",
            MetaSkillEffectType.AddStartingFood, MaterialType.Stone, 3, 5, 200);
        UpsertMetaSkill("meta_funds", "初期資金増加", "周回開始時の資金を増やす。",
            MetaSkillEffectType.AddStartingFunds, MaterialType.Stone, 3, 5, 100);
        UpsertMetaSkill("meta_wood", "初期木材増加", "周回開始時のF木材を増やす。",
            MetaSkillEffectType.AddStartingMaterial, MaterialType.Wood, 2, 5, 20);
        UpsertMetaSkill("meta_stone", "初期石材増加", "周回開始時のF石材を増やす。",
            MetaSkillEffectType.AddStartingMaterial, MaterialType.Stone, 2, 5, 10);
        UpsertMetaSkill("meta_hire_cost", "ギルド雇用コスト削減", "ギルド員の雇用コストを削減する。",
            MetaSkillEffectType.ReduceHireCostPercent, MaterialType.Stone, 5, 5, 5);
        UpsertMetaSkill("meta_research_speed", "研究速度向上", "研究速度にボーナスを付与する。",
            MetaSkillEffectType.AddResearchSpeedPercent, MaterialType.Stone, 5, 5, 5);
        UpsertMetaSkill("meta_lord_stat", "領主ステータス初期値上昇", "領主の任意ステータスを1上昇させる。",
            MetaSkillEffectType.AddLordStat, MaterialType.Stone, 1, 1, 1);
    }

    private static void UpsertMetaSkill(string skillId, string displayName, string description,
        MetaSkillEffectType effectType, MaterialType materialType, int costPerLevel, int maxLevel, int effectValuePerLevel)
    {
        string path = MetaFolder + "/" + skillId + ".asset";
        MetaSkillData asset = AssetDatabase.LoadAssetAtPath<MetaSkillData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<MetaSkillData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.skillId = skillId;
        asset.displayName = displayName;
        asset.description = description;
        asset.effectType = effectType;
        asset.materialType = materialType;
        asset.costPerLevel = costPerLevel;
        asset.maxLevel = maxLevel;
        asset.effectValuePerLevel = effectValuePerLevel;
        EditorUtility.SetDirty(asset);
    }

    // ---- Folder / asset utilities ----------------------------------------

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
