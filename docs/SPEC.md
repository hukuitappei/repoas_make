# SPEC.md — repoas_make ゲーム仕様書

> このドキュメントは Codex への実装指示書を兼ねる公式ゲーム仕様書です。
> 実装前に `docs/RULES.md` も必ず参照すること。
> `[TBD]` はバランス未決定の数値プレースホルダー。
> 実装時は定数または ScriptableObject フィールドとして定義し、数値リテラルをコードに直接書かないこと。

---

## 1. ゲーム概要

| 項目 | 値 |
|------|----|
| ジャンル | ローグライク領地経営シミュレーション |
| 進行形式 | ターン制（1ターン = 1ヶ月） |
| 1周のターン数 | 60ターン（5年） |
| 視点 | 2D トップダウン（真上視点） |
| プラットフォーム | Windows（Unity 6.3 LTS） |

### 1-1. ゲームの目的

- **勝利条件**: 60ターン終了時に領地が存続していること（人口 > 0、食料破産なし）。より高いスコアを目指す。
- **敗北条件**:
  - 人口が 0 になる
  - 資金が **3ターン連続**でマイナスになる（`GameConstants.FUNDS_DEFICIT_DEFEAT_TURNS = 3`）
  - 領主の意志消滅: 幸福度 < 20 の状態が **3ターン連続**で続いた場合に発動（`GameConstants.HAPPINESS_CRISIS_DEFEAT_TURNS = 3`）
- **スコア計算**: 人口・資金・施設数・研究数・ダンジョン踏破数・襲撃結果の加重和。

```
Score = Population
      + floor(Funds / 5)
      + (BuildingCount × 50)
      + (CompletedResearchCount × 80)
      + (CompletedImportantResearchGroupCount × 150)
      + (ClearedDungeonFloorCount × 120)
      + (RaidWinCount × 100)
      + (PerfectRaidWinCount × 150)
      - (RaidLossCount × 150)
```

### 1-2. メタ進行の概要

- 各周回の終了時（勝敗問わず）にメタポイントを獲得する。
- メタポイントは次周回開始前に、初期リソース・研究ノード事前解放・領主ステータス・メタスキルツリーへ投資できる。
- **引き継ぐ要素**: メタポイント、解放済みメタスキル。
- **リセットされる要素**: マップ・施設・リソース・研究状態・ギルド員。

---

## 2. リソース

| リソース | プロパティ名 | 単位 | 初期値 | 説明 |
|---------|------------|------|--------|------|
| 食料 | `Food` | 整数 | 1000 | 毎ターン人口に応じて消費される |
| 資金 | `Funds` | 整数（通貨） | 500 | 施設建設・研究・ギルド雇用に使用 |
| 人口 | `Population` | 整数 | 300 | 毎ターン幸福度に応じて増減 |
| 素材 | `Materials` | 整数 / 種別別在庫 | 300 | 施設建設・一部研究に使用 |

### 2-1. 素材種別と等級

- 素材は内部的には「素材種別 × 等級」の在庫として管理する。
- `Materials` はUI上の集計表示または旧API互換用の総量として扱い、実際の消費条件は可能な限り素材種別と等級で指定する。
- 素材種別は `Stone`（石材）、`Wood`（木材）、`Metal`（金属材）、`Foodstuff`（食材）、`Magic`（魔法素材）の5種。
- 素材等級は低い順に `F` / `E` / `D` / `C` / `B` / `A` / `S` の7段階。
- 研究・施設・イベントは、必要に応じて特定種別・特定等級以上の素材を要求できる。
- 初期素材はすべて F 等級とし、総量 300 を以下の内訳で保持する。

| 等級 | 石材 | 木材 | 金属材 | 食材 | 魔法素材 |
|------|------|------|--------|------|----------|
| F | 80 | 100 | 40 | 60 | 20 |

```csharp
public enum MaterialType
{
    Stone,
    Wood,
    Metal,
    Foodstuff,
    Magic,
}

public enum MaterialGrade
{
    F,
    E,
    D,
    C,
    B,
    A,
    S,
}

[System.Serializable]
public struct MaterialRequirement
{
    public MaterialType Type;
    public MaterialGrade MinimumGrade;
    public int Amount;
}
```

### 2-2. 素材入手・消費バランス

- 通常施設から得られる素材は原則 F 等級とする。
- E 等級以上の素材は、ダンジョン報酬、イベント報酬、上位研究の効果で入手する。
- 素材消費時に `MinimumGrade` より高い等級の素材を代替消費してよい。ただし自動消費は低等級から優先する。
- 施設建設・重要ノード0・序盤研究は F 等級中心、中盤研究は E〜C、終盤研究は B〜S を要求する。
- 素材生産施設の基本産出は `Sawmill` が木材、研究・イベント報酬が石材/金属材/食材/魔法素材の主入手源になる。

### 2-3. 毎ターンのリソース変化

```
食料     += 農場などの生産施設からの産出
         - (人口 × FOOD_CONSUMPTION_PER_POP)

資金     += 収入施設からの収入
         - 施設維持費合計
         - ギルド維持費合計

人口     += floor(幸福度ボーナス)
         - 疫病・災害などのイベント減少

素材     += 素材生産施設からの産出
         - 建設消費
```

- 食料が 0 を下回った場合: `floor(Population × 0.01)` を人口から減少（最低 -1）し、食料を 0 にクランプ。
- 資金が 0 を下回った場合: 維持費の高い施設から順に機能停止。資金が 0 以上に回復した時点で自動復旧。
- `FOOD_CONSUMPTION_PER_POP = 1` とする。初期人口 300 の場合、食料消費は 300 / ターン。

---

## 3. ターン進行フェーズ

```
[プレイヤーフェーズ]（順序自由、リソース制限のみが制約）
  ├─ 建設 / 施設アップグレード
  ├─ 研究選択・ギルド員の Research 行動への割り当て
  └─ その他ギルド行動指示（各ギルド員に行動を割り当て）
       ↓ 「ターン終了」ボタン（いつでも変更可能、確定後に以下を自動処理）

[自動処理フェーズ]
  ↓ 1. イベントフェーズ（EventSystem）
        ランダムイベント判定・固定イベント発火
  ↓ 2. リソース計算フェーズ（ResourceManager）
        食料・資金・人口・素材を更新
  ↓ 3. ギルド行動フェーズ（GuildManager）
        各ギルド員の行動を解決
  ↓ 4. 幸福度計算フェーズ（HappinessSystem）
        幸福度を再計算・閾値イベント判定
  ↓ 5. 襲撃判定フェーズ（RaidSystem）
        発生確率チェック・戦力計算・結果適用
  ↓ 6. ダンジョン探索フェーズ（DungeonSystem）
        探索中のギルド員の進捗更新・フロアクリア判定
  ↓ 7. 研究フェーズ（ResearchTree）
        進行中の研究を 1 ターン進める・完了判定
  ↓ 8. UI 更新
  ↓ [次のプレイヤーフェーズへ / 60ターン到達でゲーム終了]
```

定数: `GameConstants.MAX_TURNS = 60`

---

## 4. マップ

- 2D トップダウン視点のグリッドマップ。Unity の Tilemap コンポーネントを使用する。
- グリッドサイズ: 24 × 24 タイル（`GameConstants.MAP_WIDTH = 24`, `GameConstants.MAP_HEIGHT = 24`）。
- **周回ごとにランダム生成**: ダンジョン入口・川・岩地の配置が変わる。草地（建設可能地）の比率は 70% に保つ。
- タイルの種類:

| タイル | 建設 | 説明 |
|--------|------|------|
| 草地 | 可 | 標準的な建設可能地 |
| 岩地 | 不可 | 障害物 |
| 川 | 不可 | 隣接する農場の食料産出 +10%、隣接する製材所の木材産出 +10% |
| ダンジョン入口 | 不可 | `DungeonSystem` と連携 |

---

## 5. 施設（Buildings）

### 5-1. 施設基底クラス `BuildingBase`

```csharp
// Assets/Scripts/Buildings/BuildingBase.cs
public abstract class BuildingBase
{
    public string Name { get; protected set; }
    public int Level { get; protected set; }
    public int MaxLevel { get; protected set; }
    public BuildingData Data { get; protected set; }  // ScriptableObject

    public abstract void OnTurnStart(GameState state);
    public abstract void OnTurnEnd(GameState state);
    public abstract bool CanUpgrade(GameState state);
    public abstract void Upgrade();
}
```

### 5-2. `BuildingData`（ScriptableObject）の必須フィールド

```csharp
// Assets/Scripts/Data/BuildingData.cs
[CreateAssetMenu(fileName = "BuildingData", menuName = "repoas/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int[] buildCostFunds;      // レベル 1〜MaxLevel のコスト配列
    public int[] buildCostMaterials;
    public MaterialRequirement[][] buildMaterialRequirements;
    public int[] maintenanceCostFunds;
    public int[] effectValues;
    public int maxLevel;
}
```

### 5-3. 施設一覧

| カテゴリ | 施設名（英） | 施設名（日） | 主な効果 |
|---------|------------|------------|--------|
| 生産 | `Farm` | 農場 | 毎ターン食料を産出 |
| 生産 | `Sawmill` | 製材所 | 毎ターン素材を産出 |
| 生産 | `Market` | 市場 | 毎ターン資金を産出 |
| 研究 | `Library` | 図書館 | 研究速度ボーナス |
| 防衛 | `Wall` | 城壁 | 防衛力加算 |
| 防衛 | `Barracks` | 兵舎 | 戦士ギルド員の戦力ボーナス |
| 居住 | `House` | 民家 | 人口上限引き上げ |
| 居住 | `Inn` | 宿屋 | ギルド員雇用コスト減 |
| 攻略 | `DungeonGate` | ダンジョン門 | ダンジョン探索有効化 |
| 娯楽 | `Plaza` | 広場 | 幸福度ボーナス |
| 娯楽 | `Tavern` | 酒場 | ギルド員士気向上 |

### 5-4. 施設バランス初期値

- 全施設の初期 `maxLevel` は 3 とする。
- コスト表の素材はすべて F 等級以上を要求する。
- `buildCostMaterials` は旧API互換用の総素材量とし、実際の消費は `buildMaterialRequirements` を優先する。

| 施設 | Lv | 建設/強化資金 | 素材要求 | 維持費/ターン | 効果 |
|------|----|--------------|----------|--------------|------|
| `Farm` | 1 | 80 | F木材20 / F食材10 | 5 | 食料 +80 / ターン |
| `Farm` | 2 | 140 | F木材35 / F石材10 / F食材15 | 8 | 食料 +140 / ターン |
| `Farm` | 3 | 220 | F木材55 / F石材20 / F食材25 | 12 | 食料 +220 / ターン |
| `Sawmill` | 1 | 90 | F木材20 / F石材15 | 6 | F木材 +20 / ターン |
| `Sawmill` | 2 | 160 | F木材35 / F石材25 / F金属材10 | 10 | F木材 +35 / ターン |
| `Sawmill` | 3 | 250 | F木材55 / F石材40 / F金属材20 | 15 | F木材 +55 / ターン |
| `Market` | 1 | 100 | F木材15 / F石材10 | 0 | 資金 +30 / ターン |
| `Market` | 2 | 180 | F木材30 / F石材20 / F金属材10 | 0 | 資金 +55 / ターン |
| `Market` | 3 | 280 | F木材45 / F石材35 / F金属材20 | 0 | 資金 +90 / ターン |
| `Library` | 1 | 120 | F木材25 / F石材15 / F魔法素材5 | 8 | 研究速度 +5% |
| `Library` | 2 | 220 | F木材40 / F石材30 / F魔法素材10 | 14 | 研究速度 +10% |
| `Library` | 3 | 340 | F木材60 / F石材45 / F魔法素材20 | 22 | 研究速度 +18% |
| `Wall` | 1 | 100 | F石材35 / F木材10 | 5 | 防衛力 +40 |
| `Wall` | 2 | 190 | F石材60 / F木材20 / F金属材10 | 9 | 防衛力 +80 |
| `Wall` | 3 | 310 | F石材90 / F木材30 / F金属材25 | 15 | 防衛力 +140 |
| `Barracks` | 1 | 130 | F木材25 / F石材20 / F金属材15 | 10 | 戦士ギルド員戦力 +5 |
| `Barracks` | 2 | 240 | F木材40 / F石材35 / F金属材30 | 18 | 戦士ギルド員戦力 +10 |
| `Barracks` | 3 | 380 | F木材60 / F石材55 / F金属材50 | 28 | 戦士ギルド員戦力 +18 |
| `House` | 1 | 70 | F木材25 / F石材5 | 3 | 人口上限 +100 |
| `House` | 2 | 130 | F木材45 / F石材15 | 5 | 人口上限 +220 |
| `House` | 3 | 220 | F木材70 / F石材30 | 8 | 人口上限 +380 |
| `Inn` | 1 | 110 | F木材30 / F食材15 | 7 | ギルド雇用コスト -5% |
| `Inn` | 2 | 210 | F木材50 / F石材20 / F食材25 | 12 | ギルド雇用コスト -10% |
| `Inn` | 3 | 330 | F木材75 / F石材35 / F食材40 | 20 | ギルド雇用コスト -18% |
| `DungeonGate` | 1 | 150 | F石材35 / F木材20 / F魔法素材10 | 8 | ダンジョン探索有効化 |
| `DungeonGate` | 2 | 280 | F石材60 / F木材35 / F魔法素材20 | 14 | 探索報酬 +10% |
| `DungeonGate` | 3 | 450 | F石材90 / F木材55 / F魔法素材35 | 24 | 探索報酬 +20% |
| `Plaza` | 1 | 90 | F石材20 / F木材15 | 5 | 幸福度 +5 |
| `Plaza` | 2 | 170 | F石材35 / F木材25 / F食材10 | 9 | 幸福度 +10 |
| `Plaza` | 3 | 270 | F石材55 / F木材40 / F食材20 | 15 | 幸福度 +18 |
| `Tavern` | 1 | 120 | F木材35 / F食材20 | 8 | ギルド員士気 +5 |
| `Tavern` | 2 | 230 | F木材55 / F石材20 / F食材35 | 14 | ギルド員士気 +10 |
| `Tavern` | 3 | 360 | F木材80 / F石材35 / F食材55 | 22 | ギルド員士気 +18 |

---

## 6. ギルド（Guilds）

### 6-1. ギルド基底クラス `GuildBase`

```csharp
// Assets/Scripts/Guilds/GuildBase.cs
public abstract class GuildBase
{
    public string GuildName { get; protected set; }
    public List<GuildMember> Members { get; protected set; }
    public int MaxMembers { get; protected set; }
    public GuildData Data { get; protected set; }  // ScriptableObject

    public abstract void AssignAction(GuildMember member, GuildAction action);
    public abstract void ResolveActions(GameState state);
    public abstract int CalculateCombatPower();
}
```

### 6-2. ギルド員クラス `GuildMember`

```csharp
// Assets/Scripts/Characters/GuildMember.cs
public class GuildMember
{
    public string Name { get; set; }
    public int Level { get; private set; }
    public int CombatPower { get; private set; }
    public int SkillPower { get; private set; }     // ギルド固有スキル値
    public GuildAction CurrentAction { get; set; }
    public bool IsAvailable { get; set; }
}
```

### 6-3. ギルド行動列挙型

```csharp
public enum GuildAction
{
    Idle,
    Defend,       // 防衛
    Explore,      // ダンジョン探索
    Research,     // 研究支援
    Construct,    // 建設支援
}
```

### 6-4. ギルド種別

| ギルド | クラス名 | 専門行動 | 特記 |
|--------|---------|---------|------|
| 戦士ギルド | `WarriorGuild` | 防衛・ダンジョン突入・レイド迎撃 | 防衛力に直接加算 |
| 魔法使いギルド | `MageGuild` | 研究支援・ダンジョン探索・イベント解決 | 研究速度ボーナス |
| 職人ギルド | `CraftsmanGuild` | 施設建設・修繕・素材加工 | 建設コスト減少 |

### 6-5. ギルド員の初期数

- 周回開始時のギルド員総数: **5人**（`GameConstants.INITIAL_GUILD_MEMBER_COUNT = 5`）、全員が戦士ギルド所属。
- 魔法使いギルドは `mag_1`（魔法ギルドの設立）、職人ギルドは `arc_1`（職人ギルドの設立）で解放する。解放後に雇用・加入が可能になる。
- 全ギルド員は任意の行動（`GuildAction`）に割り当て可能。専門ギルドに所属する員は専門行動でボーナスを得る。

### 6-6. ギルド員の雇用・成長初期値

- 全ギルド員の初期レベルは 1、最大レベルは 10 とする。
- 雇用時の基礎能力は、戦士 `CombatPower 10 / SkillPower 5`、魔法使い `CombatPower 3 / SkillPower 12`、職人 `CombatPower 5 / SkillPower 10` とする。
- 領主の人望は雇用時能力に加算する。補正式は `floor(Popularity / 10)` を `CombatPower` と `SkillPower` に加算する。
- 雇用コストは、戦士 80G、魔法使い 120G、職人 100G とする。
- `Inn` のギルド雇用コスト軽減は雇用時に適用し、軽減後コストは小数切り捨てとする。
- レベルアップに必要な経験値は `RequiredExp = Level * 100` とする。
- レベルアップ時、戦士は `CombatPower +3 / SkillPower +1`、魔法使いは `CombatPower +1 / SkillPower +4`、職人は `CombatPower +2 / SkillPower +3`。
- 経験値獲得量は行動解決ごとに、専門行動成功時 +30、専門行動失敗時 +15、非専門行動成功時 +15、非専門行動失敗時 +5 とする。

---

## 7. 研究ノードグラフ（ResearchTree）

- 研究は単純な樹形図ではなく、複数前提を持てる研究ノードグラフとして実装する。
- 実装クラス名は既存方針に合わせて `ResearchTree` のままとする。
- 各ノードは `ResearchNodeData`（ScriptableObject）で定義する。
- 研究完了条件: `RequiredTurns` ターン経過 + `RequiredFunds` 資金消費。
- 前提ノードが解放済みでないと研究開始不可。
- 素材等級が上がるほど、1段階上の研究群・重要研究に進むイメージとする。

#### 研究の同時進行上限

- **同時進行できる研究数 = Research 行動に割り当てたギルド員数**（人員が実質的な上限）。
- 図書館などの施設・研究はあくまでボーナス・補正要素（所要ターン短縮など）であり、スロット数には影響しない。
- 初期ギルド員は全体で **5人**（周回開始時）。雇用で増員可能。

### 7-1. `ResearchNodeData`（ScriptableObject）の必須フィールド

```csharp
// Assets/Scripts/Data/ResearchNodeData.cs
[CreateAssetMenu(fileName = "ResearchNodeData", menuName = "repoas/ResearchNodeData")]
public class ResearchNodeData : ScriptableObject
{
    public string nodeId;
    public string displayName;
    public string description;
    public string[] prerequisiteNodeIds;
    public string importantResearchGroupId; // 空の場合は重要研究グループに属さない
    public int researchCostFunds;
    public int researchDurationTurns;
    public MaterialRequirement[] materialRequirements;
    public ResearchEffect[] effects;
}
```

### 7-2. 重要研究グループ

- 重要研究は単一ノードではなく、複数ノードで構成される研究グループとして扱う。
- ただし、各カテゴリの導入にあたる「重要ノード0」は単一ノードで重要研究グループ扱いにする。
- 初期段階の重要研究は、2〜3ノードの完了で1つの重要研究グループを達成したものとする。
- 以降の重要研究は、3〜5ノードの完了で1つの重要研究グループを達成したものとする。
- 重要研究グループはメタポイント獲得の集計単位になる。
- 素材等級と研究段階は連動させ、上位等級の素材ほど上位段階の研究グループで要求する。

#### 重要ノード0

| カテゴリ | ノードID | 表示名 | 解放内容 |
|---------|---------|--------|---------|
| 農業 | `agri_1` | 周辺開拓 | 探索システム解放、基礎食料生産 +50 / ターン、食料総生産 +3% |
| 軍事 | `mil_1` | ギルドとの戦闘協力 | 資金20を消費して戦闘力50を一時確保、資源の定期確保システムの解放 |
| 魔法 | `mag_1` | 魔法ギルドの設立 | 魔法カテゴリの研究ノードグラフと研究資源（魔法ギルド員）の解放 |
| 建築 | `arc_1` | 職人ギルドの設立 | 建築カテゴリの研究ノードグラフと研究・建設資源（職人ギルド員）の解放 |

- 重要ノード0は、必要資源を確保できていれば 1〜2 ターンで解放できる導入研究とする。
- 重要ノード0の共通コストは、資金 50、F等級木材 40、F等級石材 25、食料 50、必要人員 1、所要ターン 2 とする。
- 初期リソースでは木材がボトルネックになり、重要ノード0は最大2系統まで解放できる。メタポイントによる初期素材増加で4系統解放を現実的な目標にする。
- `mil_1` の一時戦闘力は戦士ギルド員上限まで確保できる。換算は戦士ギルド員 1 人 = 戦闘力 10。
- `mag_1` 解放直後の魔法ギルド員雇用上限は 3 人。
- `arc_1` 解放直後の職人ギルド員雇用上限は 3 人。

### 7-3. 研究ノード一覧

前提なしのノード（`agri_1` / `mil_1` / `mag_1` / `arc_1`）は常に研究開始可能。

#### 農業カテゴリ

| ノードID | 表示名 | 効果 | コスト | 所要ターン | 前提 |
|---------|--------|------|--------|-----------|------|
| `agri_1` | 周辺開拓 | 探索システム解放、基礎食料生産 +50 / ターン、食料総生産 +3% | 50G / F木材40 / F石材25 / 食料50 / 人員1 | 2 | なし |
| `agri_2` | 灌漑技術 | 農場の食料産出 さらに +40% | 180G / F木材30 / F石材20 / 人員1 | 4 | agri_1 |
| `agri_3` | 保存技術 | 食料の上限 +50% | 160G / F木材20 / F食材30 / 人員1 | 4 | agri_1 |
| `agri_4` | 大規模農業 | 農場の最大レベル +1 | 360G / E木材30 / E石材20 / F食材50 / 人員2 | 7 | agri_2 + agri_3 |

#### 軍事カテゴリ

| ノードID | 表示名 | 効果 | コスト | 所要ターン | 前提 |
|---------|--------|------|--------|-----------|------|
| `mil_1` | ギルドとの戦闘協力 | 資金20消費で戦闘力50を一時確保 + 資源の定期確保システム解放 | 50G / F木材40 / F石材25 / 食料50 / 人員1 | 2 | なし |
| `mil_2` | 城壁強化 | `WALL_DEFENSE_PER_LEVEL` × 1.5 | 220G / F石材40 / F金属材20 / 人員1 | 5 | mil_1 |
| `mil_3` | 戦術理論 | 防衛力全体 +20% | 200G / F木材20 / F金属材25 / 人員1 | 4 | mil_1 |
| `mil_4` | 精鋭部隊 | 戦士ギルドの最大員数 +2 | 380G / E金属材35 / E食材20 / 人員2 | 7 | mil_2 + mil_3 |

#### 魔法カテゴリ

| ノードID | 表示名 | 効果 | コスト | 所要ターン | 前提 |
|---------|--------|------|--------|-----------|------|
| `mag_1` | 魔法ギルドの設立 | 魔法研究ノードグラフ + 魔法ギルド員解放（初期上限3人） | 50G / F木材40 / F石材25 / 食料50 / 人員1 | 2 | なし |
| `mag_2` | 探索強化 | ダンジョン探索速度 +1（フロア突破が 1 ターン早くなる） | 240G / F魔法素材25 / F木材15 / 人員1 | 5 | mag_1 |
| `mag_3` | 予知術 | 領主の運 +5（イベント確率補正に適用） | 220G / F魔法素材20 / F食材20 / 人員1 | 4 | mag_1 |
| `mag_4` | 上位魔法 | 全研究の所要ターン -20% | 480G / E魔法素材40 / E金属材20 / 人員2 | 8 | mag_2 + mag_3 |

#### 建築カテゴリ

| ノードID | 表示名 | 効果 | コスト | 所要ターン | 前提 |
|---------|--------|------|--------|-----------|------|
| `arc_1` | 職人ギルドの設立 | 建築研究ノードグラフ + 職人ギルド員解放（初期上限3人） + 建設支援解放 | 50G / F木材40 / F石材25 / 食料50 / 人員1 | 2 | なし |
| `arc_2` | 高度建築 | 全施設の最大レベル +1 | 300G / F木材40 / F石材40 / F金属材20 / 人員1 | 6 | arc_1 |
| `arc_3` | 都市計画 | 民家の人口上限 +50% | 220G / F木材35 / F石材25 / 人員1 | 5 | arc_1 |
| `arc_4` | 名匠の技 | 素材生産量 +30% | 420G / E木材30 / E石材30 / E金属材25 / 人員2 | 7 | arc_2 + arc_3 |

---

## 8. イベントシステム（EventSystem）

- 毎ターン開始時に発生確率チェックを行う。
- イベントは `EventData`（ScriptableObject）として定義する。
- 優先度（`priority`）の高いイベントが先に処理される。
- 1ターンに複数イベントが発生する場合: 優先度降順で処理。

### 8-1. `EventData`（ScriptableObject）の必須フィールド

```csharp
// Assets/Scripts/Data/EventData.cs
[CreateAssetMenu(fileName = "EventData", menuName = "repoas/EventData")]
public class EventData : ScriptableObject
{
    public string eventId;
    public string displayName;
    public string description;
    public float baseProbability;      // 0.0〜1.0
    public int priority;
    public EventEffect[] effects;
    public string[] prerequisiteConditions;
    public EventChoice[] choices;      // 空配列の場合は自動解決
}
```

### 8-2. イベントカテゴリ例

| カテゴリ | 例 | 主な効果 |
|---------|-----|--------|
| 自然災害 | 洪水・干ばつ | 食料減少 |
| 吉事 | 豊作・交易商人 | リソース増加 |
| 幸福度閾値 | 暴動・移住者 | 幸福度連動 |
| 固定イベント | 特定ターン発生 | シナリオ進行 |

---

## 9. 襲撃システム（RaidSystem）

- 初回襲撃は 6 か月目を基準に発生する。
- 6 か月目以降、初回襲撃までの発生確率は指数関数的に増加し、初回のみ 4 ターン経過で確定発生する。
- 初期襲撃起点は、町からマンハッタン距離 5 マス以内に生成する。
- 初期襲撃起点が未探索の場合、初回襲撃の敵戦力は `100` とする。
- 初期襲撃起点を探索完了済みの場合、初回襲撃の敵戦力は `75` に減少する。
- 戦力基準は、成人男性の非戦闘員を `1` とする。
- 都市の初期戦力は **30**（人口300の10%相当、`GameConstants.INITIAL_CITY_DEFENSE = 30`）。
- 発生時: 敵戦力 vs 領地防衛力 の比較判定。

### 9-1. 初期襲撃起点の探索

- 探索は整数パーセントの進捗率で管理する。
- 探索実行ごとに成功 / 失敗判定を行う。
- 探索失敗時は進捗 `+34%`、探索成功時は進捗 `+50%` とする。
- 進捗が `100%` 以上になった時点で探索完了とする。
- 探索成功率は **60%**（`GameConstants.EXPLORATION_SUCCESS_RATE = 0.6f`）。

### 9-2. 防衛力計算式（暫定）

```
DefensePower = (戦士ギルド員の CombatPower 合計)
             + (城壁レベル × WALL_DEFENSE_PER_LEVEL)
             + (兵舎レベル × BARRACKS_BONUS_MULTIPLIER)
             + 研究ボーナス
```

### 9-3. 判定結果

| 結果 | 条件 | ペナルティ |
|------|------|---------|
| 完全勝利 | DefensePower >> 敵戦力 | なし |
| 辛勝 | DefensePower ≧ 敵戦力 | 軽微なリソース損失 |
| 敗北 | DefensePower < 敵戦力 | 大きなリソース損失 + 施設ダメージ |
| 壊滅 | DefensePower << 敵戦力 | 人口大幅減少 |

---

## 10. ダンジョンシステム（DungeonSystem）

- マップ上のダンジョン入口タイルから探索を開始する（`DungeonGate` 施設が必要）。
- ダンジョンは **5フロア**構成（`GameConstants.DUNGEON_FLOOR_COUNT = 5`）。
- ギルド員を探索に派遣するとその員は `IsAvailable = false` になる。
- 各フロアの突破に **3ターン**必要（`GameConstants.DUNGEON_TURNS_PER_FLOOR = 3`）。
- フロアクリア時: 報酬（資金・素材・特殊アイテム）。特殊アイテムはレア度を持ち、**周回終了時にレア度に応じたメタポイントに清算される**（加算方式）。
- 特殊アイテムのレア度は素材等級と同じ `F/E/D/C/B/A/S` の 7 段階。換算レートは下表の通り（バランス調整の可能性あり）。

| レア度 | 換算メタポイント |
|--------|--------------|
| F | 1 pt |
| E | 2 pt |
| D | 3 pt |
| C | 4 pt |
| B | 5 pt |
| A | 7 pt |
| S | 10 pt |
- 探索失敗時: 派遣ギルド員の `CombatPower` が **-30%**（2ターン後に自動回復）。死亡なし。

### 10-1. フロア別報酬初期値

| フロア | 資金 | 素材報酬 | 特殊アイテム抽選 |
|--------|------|----------|----------------|
| 1 | 60G | F石材20 / F木材20 / F食材10 | F 40% / E 10% |
| 2 | 90G | F石材25 / F木材25 / F金属材15 / F魔法素材5 | F 35% / E 15% / D 5% |
| 3 | 130G | E石材20 / E木材20 / F金属材25 / F魔法素材10 | E 25% / D 15% / C 5% |
| 4 | 180G | E石材25 / E金属材25 / E魔法素材15 | D 25% / C 15% / B 5% |
| 5 | 250G | C石材20 / C金属材20 / C魔法素材20 | C 25% / B 15% / A 5% / S 1% |

- 特殊アイテム抽選はフロアクリアごとに1回行う。表の確率に該当しなかった場合、特殊アイテムなし。
- `DungeonGate` の探索報酬ボーナスは資金と素材量に適用し、特殊アイテム確率には適用しない。

---

## 11. 幸福度システム（HappinessSystem）

- 幸福度は 0〜100 の整数値。
- 毎ターン再計算される。

### 11-1. 幸福度計算式（暫定）

```
Happiness = BASE_HAPPINESS
          + 食料充足ボーナス（食料 / 人口 の比率に応じた値）
          + 娯楽施設ボーナス（広場・酒場のレベル合計 × 係数）
          - 過密ペナルティ（人口 / 住居上限 の比率に応じた値）
          - 資金不足ペナルティ（資金がマイナスの場合）
          + イベント補正値
```

初期係数:

```csharp
BASE_HAPPINESS = 50
FOOD_RATIO_GOOD_THRESHOLD = 3.0f     // 食料 / 人口 が 3 以上なら最大ボーナス
FOOD_RATIO_BAD_THRESHOLD = 1.0f      // 食料 / 人口 が 1 未満ならペナルティ
FOOD_SUFFICIENCY_MAX_BONUS = 15
FOOD_SHORTAGE_MAX_PENALTY = -25
ENTERTAINMENT_BONUS_PER_LEVEL = 3    // Plaza / Tavern のレベル合計1ごと
OVERCROWDING_START_RATIO = 0.9f      // 人口 / 住居上限
OVERCROWDING_MAX_PENALTY = -20
FUNDS_NEGATIVE_PENALTY = -10
```

- 食料充足ボーナスは `食料 / 人口` が 1.0〜3.0 の範囲で -25〜+15 に線形補間する。
- 過密ペナルティは `人口 / 住居上限` が 0.9 を超えた場合に発生し、1.2 以上で最大 -20 とする。
- 娯楽施設ボーナスは最大 +25 でクランプする。
- 最終幸福度は 0〜100 にクランプする。

### 11-2. 幸福度閾値イベント

| 閾値 | 効果 |
|-----|------|
| >= 80 | 移住者ボーナス（人口増加加速） |
| 40〜79 | 通常状態 |
| 20〜39 | 人口増加停止 |
| < 20 | 暴動イベント発生確率上昇 |

---

## 12. 領主（Lord）

```csharp
// Assets/Scripts/Characters/LordCharacter.cs
public class LordCharacter
{
    public int Popularity  { get; private set; } // 人望   — 雇用するギルド員の初期ステータス上昇
    public int Negotiation { get; private set; } // 交渉力 — 施設・研究の資金/素材コスト軽減
    public int Luck        { get; private set; } // 運     — 入手素材量増加、人口増加速度を微増
    public int Support     { get; private set; } // 支援力 — 研究速度補正
    public int Thinking    { get; private set; } // 思考力 — 研究条件・研究深度による追加コストを緩和
}
```

- 各ステータスは 0〜100 の整数値。
- 周回開始時の基礎値は全ステータス `0`。
- 周回開始前にメタポイントを消費して、任意の領主ステータスを `+1` できる。
- ステータス上昇コストは現在値に応じて増加する。
- コスト式: `CostToIncrease = floor(CurrentStat / 5) + 1`
- 例: 0〜4 は 1pt、5〜9 は 2pt、10〜14 は 3pt、15〜19 は 4pt。
- ステータス値は補正適用後も原則 0〜100 にクランプする。

---

## 13. メタ進行システム（MetaProgressionSystem）

- 周回終了時にメタポイントを算出する。
- メタポイントはゲームをまたいで永続保存される（セーブファイル or `PlayerPrefs`）。
- 敗北時でも最低 `5pt` を獲得する。
- メタポイント獲得の骨子は、重要研究解放数と襲撃イベントの達成状況とする。
- 初回の強制襲撃イベント発生で `5pt` を獲得し、これにより最低 `5pt` を確保する。
- 初回の強制襲撃イベントをクリアした場合、追加で `5pt` を獲得する。
- 初回以降の襲撃イベントによる獲得メタポイントは、勝利段階に応じて算出する。
- 重要研究の解放数に応じて追加メタポイントを獲得する。
- ダンジョン探索で入手した特殊アイテムは、周回終了時にレア度（F〜S）に応じたメタポイントに清算される（加算方式）。換算レート: F=1, E=2, D=3, C=4, B=5, A=7, S=10（暫定、バランス調整の可能性あり）。
- メタスキルは `MetaSkillData`（ScriptableObject）で定義する。

### 13-1. メタポイント獲得初期値

| 条件 | 獲得メタポイント |
|------|----------------|
| 初回強制襲撃イベント発生 | 5pt |
| 初回強制襲撃イベントクリア | 5pt |
| 2回目以降の襲撃を完全勝利 | 3pt |
| 2回目以降の襲撃を辛勝 | 2pt |
| 2回目以降の襲撃に敗北 | 0pt |
| 重要ノード0を1つ解放 | 1pt |
| 初期段階の重要研究グループを1つ達成 | 2pt |
| 以降の重要研究グループを1つ達成 | 4pt |

- 重要ノード0は `agri_1` / `mil_1` / `mag_1` / `arc_1` の4つ。
- 初期段階の重要研究グループは、農業 `agri_2 + agri_3`、軍事 `mil_2 + mil_3`、魔法 `mag_2 + mag_3`、建築 `arc_2 + arc_3`。
- 以降の重要研究グループは、農業 `agri_4`、軍事 `mil_4`、魔法 `mag_4`、建築 `arc_4` を含む上位グループとして扱う。現行ノード数が少ないため、初期実装では各上位ノード単体で達成扱いにする。

### 13-2. メタスキル例

- 初期食料 +200（コスト 3pt / 最大5段階）
- 初期資金 +100（コスト 3pt / 最大5段階）
- 初期F木材 +20（コスト 2pt / 最大5段階）
- 初期F石材 +10（コスト 2pt / 最大5段階）
- 領主ステータス初期値 +1（コストは `CostToIncrease = floor(CurrentStat / 5) + 1`）
- ギルド雇用コスト -5%（コスト 5pt / 最大5段階 / 最大 -25%）
- 研究速度 +5%（コスト 5pt / 最大5段階 / 最大 +25%）

---

## 14. アーキテクチャ概要

```
[MonoBehaviour 層] — Unity ライフサイクルのみ担当
  GameManagerMono       → GameManager（純粋クラス）
  TurnManagerMono       → TurnManager（純粋クラス）
  ResourceManagerMono   → ResourceManager（純粋クラス）
  （以下同様）

[Pure C# 層] — ゲームロジック
  GameManager           — ゲーム全体状態・システム間のブローカー
  TurnManager           — ターン進行・フェーズ管理
  ResourceManager       — リソース（食料・資金・人口・素材）の読み書き
  ResearchTree          — ノードグラフ・解放ロジック
  EventSystem           — イベント登録・確率判定・発火
  RaidSystem            — 敵戦力生成・防衛力計算・判定
  DungeonSystem         — フロア管理・探索進捗
  GuildManager          — ギルド・ギルド員管理
  HappinessSystem       — 幸福度計算
  MetaProgressionSystem — ポイント管理・引き継ぎ

[データ層] — ScriptableObject / 値型データ
  BuildingData, GuildData, ResearchNodeData,
  EventData, RaidData, MetaSkillData,
  MaterialRequirement

[共有状態]
  GameState             — ターン数・全リソース・施設リスト等を保持
                          各 Pure C# システムは GameState を読み書きする
                          （システム間の直接参照を避けるため）
```

---

## 15. 初期実装後の調整項目

初期実装を止める未決定事項はなし。以下はノード追加・バランス調整時に再検討する。

- [ ] 上位研究グループ追加時の3〜5ノード構成
- [ ] 中盤以降の素材等級別消費量の追加調整

---

## 改訂履歴

| 日付 | 版 | 変更内容 |
|------|---|--------|
| 2026-05-06 | 0.1 | 初版作成（Phase 1） |
| 2026-05-06 | 0.2 | §3 プレイヤーフェーズ追加、§4 マップランダム生成方針追記、§7-2 研究ノード 16 件定義 |
| 2026-05-06 | 0.3 | §1・§4 視点をクォータービューから 2D トップダウンに変更 |
| 2026-05-06 | 0.4 | §6-5 初期ギルド員5人を追記、§7 研究同時進行は人員数が上限・施設はボーナスに修正 |
| 2026-05-06 | 0.5 | §6-5 初期は戦士ギルドのみ・他ギルドはゲーム内解放に修正 |
| 2026-05-06 | 0.6 | §9 初回襲撃・襲撃起点探索・探索進捗仕様を反映 |
| 2026-05-06 | 0.7 | §12 領主ステータス差し替え、§13 メタポイント獲得・割り振り方針を反映 |
| 2026-05-06 | 0.8 | §2 素材種別・等級、§7 研究ノードグラフ・重要研究グループ方針を反映 |
| 2026-05-06 | 0.9 | §7 重要ノード0として周辺開拓・戦闘協力・魔法ギルド設立・職人ギルド設立を定義 |
| 2026-05-06 | 1.0 | §2 初期リソース・食料消費、§7 重要ノード0の初期効果を反映 |
| 2026-05-06 | 1.1 | §1 意志消滅→幸福度連続低下に変更、§6 Trade 削除、§10 特殊アイテム→周回内恒久強化に確定 |
| 2026-05-06 | 1.2 | §10・§13 特殊アイテムをレア度別メタポイント清算方式に変更 |
| 2026-05-06 | 1.3 | §7 重要ノード0の共通コストと初期最大2系統解放方針を反映 |
| 2026-05-06 | 1.4 | §10 特殊アイテムのレア度 F〜S・換算レート（F=1〜S=10）を確定 |
| 2026-05-06 | 1.5 | §5 施設Lv1〜3の初期コスト・維持費・効果値を定義 |
| 2026-05-06 | 1.6 | §6 ギルド員の雇用コスト・基礎能力・レベルアップ閾値を定義 |
| 2026-05-06 | 1.7 | §1 敗北条件3ターン確定、§2 食料/資金不足ペナルティ確定、§9 初期戦力30/探索成功率60%、§10 5フロア/3ターン/失敗消耗確定 |
| 2026-05-06 | 1.8 | §4 マップサイズ、§7 研究コスト、§10 報酬、§11 幸福度係数、§13 メタポイント条件を定義 |
| 2026-05-06 | 1.9 | §13 メタスキル初期値を定義、初期実装を止めるTBDなしに整理 |

---

## 16. 最新確認メモ

- 重要ノード0の共通コストは、資金 50、F等級木材 40、F等級石材 25、食料 50、必要人員 1、所要ターン 2。
- 初期リソースでは木材がボトルネックになり、重要ノード0は最大2系統まで解放できる。
- メタポイントによる初期素材増加で、重要ノード0の4系統解放を現実的な周回目標にする。
