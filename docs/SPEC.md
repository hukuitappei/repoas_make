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
| 視点 | クォータービュー（等角投影） |
| プラットフォーム | Windows（Unity 6.3 LTS） |

### 1-1. ゲームの目的

- **勝利条件**: 60ターン終了時に領地が存続していること（人口 > 0、食料破産なし）。より高いスコアを目指す。
- **敗北条件**:
  - 人口が 0 になる
  - 資金が [TBD] ターン連続でマイナスになる
  - 領主の意志消滅（[TBD]）
- **スコア計算**: [TBD] — 人口・資金・施設数・研究数・ダンジョン踏破数の加重和。

### 1-2. メタ進行の概要

- 各周回の終了時（勝敗問わず）にメタポイントを獲得する。
- メタポイントは次周回開始前にメタスキルツリーに投資できる（永続ボーナス）。
- **引き継ぐ要素**: メタポイント、解放済みメタスキル。
- **リセットされる要素**: マップ・施設・リソース・研究状態・ギルド員。

---

## 2. リソース

| リソース | プロパティ名 | 単位 | 初期値 | 説明 |
|---------|------------|------|--------|------|
| 食料 | `Food` | 整数 | [TBD] | 毎ターン人口に応じて消費される |
| 資金 | `Funds` | 整数（通貨） | [TBD] | 施設建設・研究・ギルド雇用に使用 |
| 人口 | `Population` | 整数 | [TBD] | 毎ターン幸福度に応じて増減 |
| 素材 | `Materials` | 整数 | [TBD] | 施設建設・一部研究に使用 |

### 2-1. 毎ターンのリソース変化

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

- 食料が 0 を下回った場合: 人口減少ペナルティ（[TBD]）を適用し、食料を 0 にクランプ。
- 資金が 0 を下回った場合: 施設の一部が機能停止（[TBD] ルール）。

---

## 3. ターン進行フェーズ

```
[ターン開始]
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
  ↓ [ターン終了 → 次ターンへ / 60ターン到達でゲーム終了]
```

定数: `GameConstants.MAX_TURNS = 60`

---

## 4. マップ

- クォータービュー（等角投影）のグリッドマップ。
- グリッドサイズ: [TBD] × [TBD] タイル。
- タイルの種類:

| タイル | 建設 | 説明 |
|--------|------|------|
| 草地 | 可 | 標準的な建設可能地 |
| 岩地 | 不可 | 障害物 |
| 川 | 不可 | 食料・素材ボーナス付与（[TBD]） |
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
    public int[] maintenanceCostFunds;
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
| 娯楽 | `Tavern` | 酒場 | ギルド員士気向上（[TBD]） |

各施設の詳細コスト・産出値は Phase 2-7 のバランス文書で別途定義する。

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
    Trade,        // 交易（[TBD]）
}
```

### 6-4. ギルド種別

| ギルド | クラス名 | 専門行動 | 特記 |
|--------|---------|---------|------|
| 戦士ギルド | `WarriorGuild` | 防衛・ダンジョン突入・レイド迎撃 | 防衛力に直接加算 |
| 魔法使いギルド | `MageGuild` | 研究支援・ダンジョン探索・イベント解決 | 研究速度ボーナス |
| 職人ギルド | `CraftsmanGuild` | 施設建設・修繕・素材加工 | 建設コスト減少 |

---

## 7. 研究ツリー（ResearchTree）

- ツリー構造のノードとして実装する。
- 各ノードは `ResearchNodeData`（ScriptableObject）で定義する。
- 研究完了条件: `RequiredTurns` ターン経過 + `RequiredFunds` 資金消費。
- 前提ノードが解放済みでないと研究開始不可。

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
    public int researchCostFunds;
    public int researchDurationTurns;
    public ResearchEffect[] effects;
}
```

### 7-2. 研究カテゴリ（Phase 2 で詳細化）

| カテゴリ | 効果対象 |
|---------|---------|
| 農業 | 食料産出向上 |
| 軍事 | 戦力強化 |
| 魔法 | 探索・イベント関連 |
| 建築 | 建設コスト・上限拡張 |

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

- 毎ターン終了時に発生確率チェック（`RaidData.baseProbability` 参照）。
- 発生時: 敵戦力 vs 領地防衛力 の比較判定。

### 9-1. 防衛力計算式（暫定）

```
DefensePower = (戦士ギルド員の CombatPower 合計)
             + (城壁レベル × WALL_DEFENSE_PER_LEVEL)
             + (兵舎レベル × BARRACKS_BONUS_MULTIPLIER)
             + 研究ボーナス
```

### 9-2. 判定結果

| 結果 | 条件 | ペナルティ |
|------|------|---------|
| 完全勝利 | DefensePower >> 敵戦力 | なし |
| 辛勝 | DefensePower ≧ 敵戦力 | 軽微なリソース損失 |
| 敗北 | DefensePower < 敵戦力 | 大きなリソース損失 + 施設ダメージ |
| 壊滅 | DefensePower << 敵戦力 | 人口大幅減少 |

---

## 10. ダンジョンシステム（DungeonSystem）

- マップ上のダンジョン入口タイルから探索を開始する（`DungeonGate` 施設が必要）。
- ダンジョンは複数フロアで構成される（初期 [TBD] フロア）。
- ギルド員を探索に派遣するとその員は `IsAvailable = false` になる。
- 各フロアを突破するのに [TBD] ターン必要。
- フロアクリア時: 報酬（資金・素材・特殊アイテム [TBD]）。
- 探索失敗時: ギルド員の消耗（[TBD]）。

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
    public int Charisma    { get; private set; }  // カリスマ — イベント選択肢効果向上
    public int Intelligence { get; private set; } // 知略   — 研究速度ボーナス
    public int Valor       { get; private set; }  // 武勇   — 防衛力ボーナス
    public int Leadership  { get; private set; }  // 統率   — 幸福度ボーナス
    public int Luck        { get; private set; }  // 運     — イベント確率補正・ダンジョン報酬ボーナス
}
```

- 各ステータスの初期値: [TBD]（メタスキルで初期値変更可能）
- ステータス上限: [TBD]

---

## 13. メタ進行システム（MetaProgressionSystem）

- 周回終了時にスコアを算出し、メタポイントに変換する。
- メタポイントはゲームをまたいで永続保存される（セーブファイル or `PlayerPrefs`）。
- メタスキルは `MetaSkillData`（ScriptableObject）で定義する。

### 13-1. メタスキル例（Phase 2 で詳細化）

- 初期食料 +[TBD]
- 初期資金 +[TBD]
- 領主ステータス初期値 +1
- ギルド雇用コスト -[TBD]%
- 研究速度 +[TBD]%

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

[データ層] — ScriptableObject
  BuildingData, GuildData, ResearchNodeData,
  EventData, RaidData, MetaSkillData

[共有状態]
  GameState             — ターン数・全リソース・施設リスト等を保持
                          各 Pure C# システムは GameState を読み書きする
                          （システム間の直接参照を避けるため）
```

---

## 15. 未決定事項（TBD リスト）

実装時は `[TBD]` の箇所を定数か ScriptableObject フィールドとして定義し、数値は仮置きしてよい。

- [ ] マップサイズ（グリッド幅・高さ）
- [ ] 初期リソース量（食料・資金・人口・素材）
- [ ] 食料消費量（人口1人あたり / ターン）
- [ ] 各施設の産出量・コスト・維持費
- [ ] 各研究ノードのコスト・ターン数・効果値
- [ ] ダンジョンフロア数・探索ターン数・報酬
- [ ] ギルド員雇用コスト・レベルアップ閾値
- [ ] 領主ステータス初期値・上限
- [ ] 幸福度計算の各係数
- [ ] スコア計算の重み
- [ ] メタポイント換算レート
- [ ] 敗北条件の連続資金マイナスターン数

---

## 改訂履歴

| 日付 | 版 | 変更内容 |
|------|---|--------|
| 2026-05-06 | 0.1 | 初版作成（Phase 1） |
