using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        CreateInitialDataAssets();

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("No open scene is available for bootstrap setup.");
            return;
        }

        GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
        if (bootstrap == null)
        {
            GameObject bootstrapObject = new GameObject("GameBootstrap");
            bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
        }

        SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
        AssignObjectReference(serializedBootstrap, "mainGameScreen", Object.FindFirstObjectByType<MainGameScreen>());
        AssignObjectReference(serializedBootstrap, "mapPanel", Object.FindFirstObjectByType<MapPanel>());
        AssignArray(serializedBootstrap, "guildCatalog", FindAssetsByType<GuildData>(GuildsFolder));
        AssignArray(serializedBootstrap, "availableBuildings", FindAssetsByType<BuildingData>(BuildingsFolder));
        AssignArray(serializedBootstrap, "startingBuildings", new Object[]
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

    private static T LoadAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static Object[] FindAssetsByType<T>(string folder) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
        Object[] assets = new Object[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return assets;
    }

    private static void AssignObjectReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void AssignArray(SerializedObject serializedObject, string propertyName, Object[] values)
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
