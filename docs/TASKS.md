# TASKS.md — 実装タスクリスト

> 担当凡例: 🔵 Claude Code / 🟠 Codex / 👤 ユーザー  
> 参照必須: `docs/RULES.md`, `docs/SPEC.md`, `docs/CODEX_IMPLEMENTATION.md`  
> 方針: Unityプロジェクトはユーザーが手動作成し、その後にCore/Data土台から実装する。

---

## Phase 0 — Unityプロジェクト手動作成 👤

| # | タスク | 完了条件 |
|---|--------|----------|
| 0-1 | Unity Hubでプロジェクト作成 | Unity 6.3 LTS / 2D テンプレート / パス `C:\Projects\repoas_make` |
| 0-2 | 既存フォルダとの同期 | `Assets/`, `Packages/`, `ProjectSettings/` が同一リポジトリ直下に存在する |
| 0-3 | 初回起動確認 | Unity Editorでプロジェクトが開ける |
| 0-4 | メインシーン作成 | `Assets/Scenes/Main.unity` が存在する |
| 0-5 | Git差分確認 | Unity生成ファイルがGitに認識され、不要な一時ファイルが `.gitignore` されている |

---

## Phase 1 — Core/Data土台 🔵

| # | タスク | ファイル | 完了条件 |
|---|--------|----------|----------|
| 1-1 | 全ゲーム定数 | `Assets/Scripts/Core/GameConstants.cs` | `MAX_TURNS=60`, `MAP_WIDTH=24`, `MAP_HEIGHT=24`, 探索成功率60%などを網羅 |
| 1-2 | 素材型 | `Assets/Scripts/Data/MaterialType.cs`, `MaterialGrade.cs`, `MaterialRequirement.cs` | `Stone/Wood/Metal/Foodstuff/Magic` と `F/E/D/C/B/A/S` を定義 |
| 1-3 | 共有状態 | `Assets/Scripts/Core/GameState.cs` | ターン、リソース、素材在庫、施設、ギルド、研究、探索、襲撃、幸福度、メタ情報を保持 |
| 1-4 | データSO | `Assets/Scripts/Data/*.cs` | `BuildingData`, `GuildData`, `ResearchNodeData`, `RaidData`, `MetaSkillData`, `EventData` が存在 |
| 1-5 | 効果型 | `Assets/Scripts/Data/ResearchEffect.cs`, `EventEffect.cs`, `EventChoice.cs` | 研究・イベント効果をデータで表現できる |
| 1-6 | 施設基底 | `Assets/Scripts/Buildings/BuildingBase.cs` | Codex施設クラスが継承できる抽象基底が存在 |
| 1-7 | ギルド基底 | `Assets/Scripts/Guilds/GuildBase.cs` | Codexギルドクラスが継承できる抽象基底が存在 |

---

## Phase 2 — Coreシステム 🔵

| # | タスク | ファイル | 完了条件 |
|---|--------|----------|----------|
| 2-1 | GameManager | `Assets/Scripts/Core/GameManager.cs` | 初期化、周回リセット、勝敗判定の入口がある |
| 2-2 | TurnManager | `Assets/Scripts/Core/TurnManager.cs` | プレイヤーフェーズから8つの自動処理フェーズを順に呼ぶ |
| 2-3 | ResourceManager | `Assets/Scripts/Core/ResourceManager.cs` | 食料/資金/人口/素材在庫、食料不足、資金マイナス施設停止を処理 |
| 2-4 | ResearchTree | `Assets/Scripts/Core/ResearchTree.cs` | ノード前提、同時研究数、進捗、完了時効果を扱う |
| 2-5 | EventSystem | `Assets/Scripts/Core/EventSystem.cs` | イベント抽選、優先度順処理、選択肢の適用ができる |
| 2-6 | RaidSystem | `Assets/Scripts/Core/RaidSystem.cs` | 初回襲撃、探索進捗、敵戦力、防衛判定、結果記録ができる |
| 2-7 | DungeonSystem | `Assets/Scripts/Core/DungeonSystem.cs` | 5フロア、3ターン進行、報酬、特殊アイテム、失敗消耗を扱う |
| 2-8 | GuildManager | `Assets/Scripts/Core/GuildManager.cs` | ギルド員行動割り当て、同時研究数、行動解決の入口がある |
| 2-9 | HappinessSystem | `Assets/Scripts/Core/HappinessSystem.cs` | SPECの係数で幸福度を計算し、閾値状態を出す |
| 2-10 | MetaProgressionSystem | `Assets/Scripts/Core/MetaProgressionSystem.cs` | メタポイント獲得、ステータス上昇コスト、特殊アイテム清算を処理 |
| 2-11 | ScoreCalculator | `Assets/Scripts/Core/ScoreCalculator.cs` | SPEC §1 のスコア式を実装 |
| 2-12 | MapGenerator | `Assets/Scripts/Core/MapGenerator.cs` | 24×24、草地70%、川/岩地/ダンジョン入口/初期襲撃起点を生成 |

---

## Phase 3 — Codex担当: キャラクター・ギルド 🟠

| # | タスク | ファイル | 完了条件 |
|---|--------|----------|----------|
| 3-1 | ギルド員 | `Assets/Scripts/Characters/GuildMember.cs` | Lv/経験値/CombatPower/SkillPower/行動/利用可能状態を持つ |
| 3-2 | 領主 | `Assets/Scripts/Characters/LordCharacter.cs` | 5ステータス、0〜100クランプ、上昇コスト式を持つ |
| 3-3 | 戦士ギルド | `Assets/Scripts/Guilds/WarriorGuild.cs` | 防衛・探索・レイド迎撃の専門処理がある |
| 3-4 | 魔法ギルド | `Assets/Scripts/Guilds/MageGuild.cs` | `mag_1` 解放後に研究支援・探索・イベント解決を扱える |
| 3-5 | 職人ギルド | `Assets/Scripts/Guilds/CraftsmanGuild.cs` | `arc_1` 解放後に建設支援・修繕・素材加工を扱える |

---

## Phase 4 — Codex担当: 施設 🟠

| # | タスク | ファイル | 完了条件 |
|---|--------|----------|----------|
| 4-1 | 農場 | `Assets/Scripts/Buildings/Farm.cs` | 食料産出、川隣接+10% |
| 4-2 | 製材所 | `Assets/Scripts/Buildings/Sawmill.cs` | F木材産出、川隣接+10% |
| 4-3 | 市場 | `Assets/Scripts/Buildings/Market.cs` | 資金産出 |
| 4-4 | 図書館 | `Assets/Scripts/Buildings/Library.cs` | 研究速度ボーナス |
| 4-5 | 城壁 | `Assets/Scripts/Buildings/Wall.cs` | 防衛力加算 |
| 4-6 | 兵舎 | `Assets/Scripts/Buildings/Barracks.cs` | 戦士ギルド員戦力ボーナス |
| 4-7 | 民家 | `Assets/Scripts/Buildings/House.cs` | 人口上限増加 |
| 4-8 | 宿屋 | `Assets/Scripts/Buildings/Inn.cs` | ギルド雇用コスト軽減 |
| 4-9 | ダンジョン門 | `Assets/Scripts/Buildings/DungeonGate.cs` | ダンジョン探索有効化、報酬ボーナス |
| 4-10 | 広場 | `Assets/Scripts/Buildings/Plaza.cs` | 幸福度ボーナス |
| 4-11 | 酒場 | `Assets/Scripts/Buildings/Tavern.cs` | ギルド員士気ボーナス |

---

## Phase 5 — Codex担当: UI 🟠

| # | タスク | ファイル | 完了条件 |
|---|--------|----------|----------|
| 5-1 | メイン画面 | `Assets/Scripts/UI/MainGameScreen.cs` | ターン、年/月、ターン終了ボタンを表示 |
| 5-2 | リソース表示 | `Assets/Scripts/UI/ResourcePanel.cs` | 食料/資金/人口/素材総量/等級別素材を表示 |
| 5-3 | 研究UI | `Assets/Scripts/UI/ResearchPanel.cs` | 研究ノード状態、コスト、進捗、同時研究上限を表示 |
| 5-4 | マップUI | `Assets/Scripts/UI/MapPanel.cs` | Tilemap表示とタイル選択ができる |
| 5-5 | 探索UI | `Assets/Scripts/UI/ExplorationPanel.cs` | 襲撃起点探索進捗と成功/失敗を表示 |
| 5-6 | 施設建設UI | `Assets/Scripts/UI/BuildPanel.cs` | 建設可能施設、コスト、不足資源、配置可否を表示 |
| 5-7 | ギルドUI | `Assets/Scripts/UI/GuildPanel.cs` | ギルド員、行動割り当て、解放状態を表示 |
| 5-8 | 襲撃UI | `Assets/Scripts/UI/RaidPopup.cs` | 敵戦力、防衛力、結果を表示 |
| 5-9 | 幸福度UI | `Assets/Scripts/UI/HappinessPanel.cs` | 幸福度、閾値、危機状態を表示 |
| 5-10 | 周回終了UI | `Assets/Scripts/UI/MetaScreen.cs` | スコア、メタポイント、特殊アイテム清算、割り振りを表示 |

---

## Phase 6 — ScriptableObject初期データ 🔵/🟠

| # | タスク | 担当 | 完了条件 |
|---|--------|------|----------|
| 6-1 | 施設データ | 🔵/🟠 | `Assets/ScriptableObjects/Buildings/` に11施設分の `.asset` がある |
| 6-2 | 研究データ | 🔵 | `Assets/ScriptableObjects/Research/` に16研究ノード分の `.asset` がある |
| 6-3 | メタスキルデータ | 🔵 | `Assets/ScriptableObjects/Meta/` に初期メタスキルがある |
| 6-4 | イベント・襲撃データ | 🔵 | 初回襲撃、基本イベント、RaidData が `.asset` 化されている |

---

## Phase 7 — 検証・レビュー 🔵

| # | タスク | 完了条件 |
|---|--------|----------|
| 7-1 | コンパイル確認 | Unity Console にコンパイルエラーがない |
| 7-2 | Core単体確認 | Resource/Research/Raid/Dungeon/Happiness/Meta の基本ケースを確認 |
| 7-3 | Codex担当レビュー | `docs/CODEX_IMPLEMENTATION.md` の完成判定を満たす |
| 7-4 | 10ターン試験 | 初期状態から10ターン進行し、襲撃・研究・資源が破綻しない |
| 7-5 | 60ターン試験 | 1周完走し、スコアとメタポイントが表示される |

---

## 着手順序

1. 👤 Unityプロジェクトを手動作成する。
2. 🔵 Phase 1 Core/Data土台を作る。
3. 🔵 Phase 2 Coreシステムを作る。
4. 🟠 Phase 3〜5 を Codex が実装する。
5. 🔵/🟠 Phase 6 ScriptableObject初期データを作る。
6. 🔵 Phase 7 でレビュー・検証する。

---

## Codex への渡し方

Codexへ実装依頼するときは、以下を必ず渡す。

- `docs/RULES.md`
- `docs/SPEC.md`
- `docs/CODEX_IMPLEMENTATION.md`
- `docs/TASKS.md`

最初の依頼は Phase 3 または Phase 4 から始める。Phase 1〜2 が未完了の場合、Codexには実装ではなくレビュー・事前確認だけを依頼する。
