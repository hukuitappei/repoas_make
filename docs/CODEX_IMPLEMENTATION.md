# CODEX_IMPLEMENTATION.md

> Codex 担当実装の進捗管理用ドキュメント。  
> 参照元: `docs/SPEC.md`, `docs/RULES.md`, `docs/TASKS.md`

---

## Scope

Codex の担当範囲は以下とする。

- `Assets/Scripts/Buildings/`
- `Assets/Scripts/Guilds/`
- `Assets/Scripts/Characters/`
- `Assets/Scripts/UI/`
- 上記を利用するために必要な最小限の Core 接続

Core / Data 本体、Unity プロジェクト設定、Scene 全体の最終調整は原則として Codex 担当外とする。

---

## Current Status

### Completed

- Characters
  - [x] `GuildMember`
  - [x] `LordCharacter`
- Guilds
  - [x] `WarriorGuild`
  - [x] `MageGuild`
  - [x] `CraftsmanGuild`
- Buildings
  - [x] `Farm`
  - [x] `Sawmill`
  - [x] `Market`
  - [x] `Library`
  - [x] `Wall`
  - [x] `Barracks`
  - [x] `House`
  - [x] `Inn`
  - [x] `DungeonGate`
  - [x] `Plaza`
  - [x] `Tavern`
- UI
  - [x] `MainGameScreen`
  - [x] `ResourcePanel`
  - [x] `PopulationAssignmentPanel`
  - [x] `ResearchPanel`
  - [x] `MapPanel`
  - [x] `ExplorationPanel`
  - [x] `BuildPanel`
  - [x] `GuildPanel`
  - [x] `RaidPopup`
  - [x] `HappinessPanel`
  - [x] `MetaScreen`
- Codex-side Core integration
  - [x] `GameManager.TryHireGuildMember`
  - [x] `GameManager.TryBuildOrUpgradeBuilding`
  - [x] `GameManager.GetGuildHireCost`
  - [x] `GameState.SpentMetaPoints`

### Remaining

- Verification
  - [ ] Unity Editor 上でのコンパイル確認
  - [ ] Scene / serialized field の参照確認
- UI spec gaps
  - [ ] Tilemap ベースの視覚表示
  - [ ] 建設位置の可視化
  - [ ] 研究ノードの視覚的な状態区別
- Documentation
  - [ ] `docs/TASKS.md` の進捗表記更新

---

## Detailed Checklist

### Characters

- [x] `GuildMember` が `Level`, `Experience`, `CombatPower`, `SkillPower`, `CurrentAction`, `IsAvailable` を持つ
- [x] 雇用時に `floor(Popularity / 10)` の補正を適用する
- [x] `RequiredExperience = Level * 100` を利用する
- [x] 行動結果に応じた経験値加算を持つ
- [x] 一時的な戦闘ペナルティとダンジョン探索状態を持つ

- [x] `LordCharacter` が `Popularity`, `Negotiation`, `Luck`, `Support`, `Thinking` を持つ
- [x] 全ステータスを `0..100` にクランプする
- [x] メタポイント消費コスト `floor(CurrentStat / 5) + 1` を利用する
- [x] Core 側から参照できる public API を持つ

### Guilds

- [x] `WarriorGuild` を実装
- [x] `MageGuild` を実装
- [x] `CraftsmanGuild` を実装
- [x] `AssignAction` で所属員と解放状態を検証する
- [x] `ResolveActions` で `GameState` へ結果を反映する
- [x] `CalculateCombatPower` で防衛参加可能メンバーを集計する

### Buildings

- [x] 全施設クラスが `BuildingBase` を継承する
- [x] コスト、維持費、効果値を `BuildingData` から読む
- [x] `CanUpgrade(GameState state)` を実装する
- [x] `Upgrade()` はレベル上昇のみを担当する
- [x] `OnTurnStart` / `OnTurnEnd` のどちらかで効果を適用する

- [x] `Farm` 食料生産
- [x] `Sawmill` 木材生産
- [x] `Market` 資金生産
- [x] `Library` 研究速度補正
- [x] `Wall` 防衛力補正
- [x] `Barracks` 戦闘系補正
- [x] `House` 人口上限補正
- [x] `Inn` 雇用コスト軽減
- [x] `DungeonGate` ダンジョン解放 / 報酬補正
- [x] `Plaza` 幸福度補正
- [x] `Tavern` 士気補正

### UI

- [x] メイン画面から主要パネルを bind / refresh できる
- [x] リソース表示 UI
- [x] 人口割当 UI
- [x] 研究 UI
- [x] 探索 UI
- [x] 施設建設 UI
- [x] ギルド管理 UI
- [x] 襲撃結果 UI
- [x] 幸福度 UI
- [x] メタポイント UI

### Core Integration Added By Codex

- [x] UI から建設 / 強化の入口を呼べる
- [x] UI からギルド加入の入口を呼べる
- [x] UI からメタポイント消費結果を状態として保持できる

---

## Open Risks

- Unity Editor 上で未確認のため、serialized field の未割当が残っている可能性がある
- UI は操作入口までは実装済みだが、見た目の最終仕様は未達部分がある
- `docs/TASKS.md` には旧来の進捗表記が残っている

---

## Implementation Notes

### 2026-05-07

- Added Core API
  - `GameManager.TryHireGuildMember`
  - `GameManager.TryBuildOrUpgradeBuilding`
  - `GameManager.GetGuildHireCost`
  - `GameState.SpentMetaPoints`
- Updated UI
  - `GuildPanel` にギルド選択と加入処理を追加
  - `BuildPanel` に建設対象選択と建設 / 強化処理を追加
  - `MetaScreen` のメタポイント消費を `GameState` に保持するよう変更
  - `MainGameScreen` から `BuildPanel` を `GameManager` 経由で bind するよう変更
- Dependencies checked
  - 建設生成は `BuildingEffectType` と既存 building class の対応に従う
  - 加入コストは `GuildData.hireCostFunds` と `GameState.HireCostReductionPercent` を使用
  - メタポイント残量は `MetaProgressionSystem.CalculateEarnedMetaPoints(state) - state.SpentMetaPoints`
- Review points
  - Unity Editor 上でのボタン配線確認が必要
  - Scene 上の serialized field 割当確認が必要
