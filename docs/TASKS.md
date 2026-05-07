# TASKS.md

> 実装タスクの現状管理。  
> 詳細仕様は `docs/SPEC.md`、Codex 担当の実装詳細は `docs/CODEX_IMPLEMENTATION.md` を参照する。

---

## Current Progress

### Done

- Core / Data の基盤実装
- Resource / Research / Raid / Dungeon / Happiness / Meta の主要 Core 実装
- Codex 担当の Characters 実装
  - `GuildMember`
  - `LordCharacter`
- Codex 担当の Guilds 実装
  - `WarriorGuild`
  - `MageGuild`
  - `CraftsmanGuild`
- Codex 担当の Buildings 実装
  - `Farm`
  - `Sawmill`
  - `Market`
  - `Library`
  - `Wall`
  - `Barracks`
  - `House`
  - `Inn`
  - `DungeonGate`
  - `Plaza`
  - `Tavern`
- Codex 担当の UI 実装
  - `MainGameScreen`
  - `ResourcePanel`
  - `PopulationAssignmentPanel`
  - `ResearchPanel`
  - `MapPanel`
  - `ExplorationPanel`
  - `BuildPanel`
  - `GuildPanel`
  - `RaidPopup`
  - `HappinessPanel`
  - `MetaScreen`
- UI から Core への主要操作入口
  - 建設 / 強化
  - ギルド加入
  - メタポイント消費

### Remaining

- Unity Editor 上での確認
  - [ ] コンパイル確認
  - [ ] Scene / serialized field 配線確認
- UI の未達表現
  - [ ] Tilemap ベースの視覚表示
  - [ ] 建設位置の可視化
  - [ ] 研究ノードの視覚的な状態区別
- 仕上げ
  - [ ] 60 ターン通しでの動作確認
  - [ ] ドキュメント間の進捗表記統一

---

## Responsibility Split

### Codex

- `Assets/Scripts/Buildings/`
- `Assets/Scripts/Guilds/`
- `Assets/Scripts/Characters/`
- `Assets/Scripts/UI/`
- 上記に必要な最小限の Core 接続

### Outside Codex Main Scope

- Core / Data の最終責務整理
- Unity ProjectSettings / Scene の最終調整
- Git LFS 設定
- セーブ / ロードの永続化本体

---

## Next Recommended Steps

1. Unity Editor でコンパイル確認を行う
2. `BuildPanel` と `GuildPanel` の追加参照が Scene 上で割り当たっているか確認する
3. UI の視覚表現の未達項目を詰める
4. 問題なければ進捗を再度 docs に反映する
