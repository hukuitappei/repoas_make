# CODEX_IMPLEMENTATION.md — Codex 実装チェックリスト

> Codex はこの文書、`docs/SPEC.md`、`docs/RULES.md` を必ず読んでから実装する。
> この文書は Codex 担当範囲を固定し、完成判定を明確にするためのチェックリストである。

---

## 1. 実装ゴール

Codex の初期実装ゴールは、Claude Code が用意する Core / Data 層に接続できる形で、以下を完成させること。

- [ ] 施設クラス群を実装し、施設ごとの効果を `GameState` に適用できる。
- [ ] ギルド・ギルド員・領主クラスを実装し、行動割り当てと能力計算ができる。
- [ ] UI コントローラーを実装し、主要ゲーム状態を表示・操作できる。
- [ ] ScriptableObject データを前提にし、マジックナンバーをコードへ直書きしない。
- [ ] MonoBehaviour は UI イベント接続と描画更新に限定し、ゲームロジックを持たない。

---

## 2. 担当範囲

### 2-1. Codex が実装する

- [ ] `Assets/Scripts/Buildings/`
  - [ ] `Farm`
  - [ ] `Sawmill`
  - [ ] `Market`
  - [ ] `Library`
  - [ ] `Wall`
  - [ ] `Barracks`
  - [ ] `House`
  - [ ] `Inn`
  - [ ] `DungeonGate`
  - [ ] `Plaza`
  - [ ] `Tavern`
- [ ] `Assets/Scripts/Guilds/`
  - [ ] `WarriorGuild`
  - [ ] `MageGuild`
  - [ ] `CraftsmanGuild`
- [ ] `Assets/Scripts/Characters/`
  - [ ] `GuildMember`
  - [ ] `LordCharacter`
- [ ] `Assets/Scripts/UI/`
  - [ ] メイン画面 UI コントローラー
  - [ ] リソース表示 UI
  - [ ] 研究ノード UI
  - [ ] マップ・探索 UI
  - [ ] 施設建設 UI
  - [ ] ギルド管理 UI
  - [ ] 襲撃結果 UI
  - [ ] 幸福度 UI
  - [ ] 周回終了・メタポイント UI

### 2-2. Codex が実装しない

- [ ] `Assets/Scripts/Core/` のコアシステム本体
- [ ] ターン進行の最終責務
- [ ] セーブ/ロードの永続化本体
- [ ] ランダムマップ生成本体
- [ ] Unity プロジェクト初期化、ProjectSettings、Scene 作成
- [ ] Git LFS 設定

Codex が Core 側の変更を必要と判断した場合は、直接大きく変更せず、必要なインターフェース・メソッド・データを実装メモに書く。

---

## 3. Core / Data 層への依存

Claude Code が先に用意する前提の型:

- [ ] `GameState`
- [ ] `GameConstants`
- [ ] `MaterialType`
- [ ] `MaterialGrade`
- [ ] `MaterialRequirement`
- [ ] `BuildingBase`
- [ ] `GuildBase`
- [ ] `BuildingData`
- [ ] `GuildData`
- [ ] `ResearchNodeData`
- [ ] `ResearchEffect`
- [ ] `ResourceManager`
- [ ] `ResearchTree`
- [ ] `GuildManager`
- [ ] `RaidSystem`
- [ ] `DungeonSystem`
- [ ] `HappinessSystem`
- [ ] `MetaProgressionSystem`

Codex 実装は、上記が未実装でもコンパイル可能な仮スタブを勝手に増やさない。必要なら `TODO` コメントではなく、実装メモで Claude Code 側に要求する。

---

## 4. 施設実装チェックリスト

### 4-1. 共通条件

- [ ] 各施設クラスは `BuildingBase` を継承する。
- [ ] 施設名・最大レベル・コスト・維持費・効果値は `BuildingData` から読む。
- [ ] レベル範囲は `1 <= Level <= MaxLevel` を守る。
- [ ] `CanUpgrade(GameState state)` は資金・素材・最大レベルを確認する。
- [ ] `Upgrade()` はレベル上昇のみを担当し、リソース消費は Core 側または明示されたサービスに委譲する。
- [ ] `OnTurnStart` / `OnTurnEnd` のどちらで効果を適用するかは `SPEC.md` のターン順に合わせる。
- [ ] 効果値が割合の場合は、整数丸めルールを明示する。

### 4-2. 施設別完成条件

- [ ] `Farm`: ターンごとに食料を加算する。川隣接ボーナス +10% を受けられる。
- [ ] `Sawmill`: ターンごとに F 木材を加算する。川隣接ボーナス +10% を受けられる。
- [ ] `Market`: ターンごとに資金を加算する。
- [ ] `Library`: 研究速度ボーナスを提供する。研究スロット数は増やさない。
- [ ] `Wall`: 防衛力を加算する。
- [ ] `Barracks`: 戦士ギルド員の戦力ボーナスを提供する。
- [ ] `House`: 人口上限を増加させる。
- [ ] `Inn`: ギルド雇用コストを軽減する。
- [ ] `DungeonGate`: ダンジョン探索を有効化し、Lv2以降は探索報酬を増加させる。
- [ ] `Plaza`: 幸福度ボーナスを提供する。
- [ ] `Tavern`: ギルド員士気ボーナスを提供する。

---

## 5. ギルド・キャラクター実装チェックリスト

### 5-1. `GuildMember`

- [ ] 初期レベルは 1、最大レベルは 10。
- [ ] `CombatPower`、`SkillPower`、`CurrentAction`、`IsAvailable` を持つ。
- [ ] 雇用時に領主の人望補正 `floor(Popularity / 10)` を `CombatPower` と `SkillPower` に加算する。
- [ ] レベルアップ必要経験値は `RequiredExp = Level * 100`。
- [ ] 行動結果に応じて経験値を加算できる。
- [ ] レベルアップ時の上昇値は `SPEC.md` のギルド種別ごとの値に従う。

### 5-2. ギルド

- [ ] `WarriorGuild` は防衛・探索・レイド迎撃で専門ボーナスを持つ。
- [ ] `MageGuild` は研究支援・探索・イベント解決で専門ボーナスを持つ。
- [ ] `CraftsmanGuild` は建設支援・修繕・素材加工で専門ボーナスを持つ。
- [ ] `AssignAction` は所属員・利用可能状態・解放済み行動を検証する。
- [ ] `ResolveActions` は行動結果を `GameState` に反映するための結果オブジェクトまたは Core API を使う。
- [ ] `CalculateCombatPower` は防衛に参加可能なギルド員だけを集計する。

### 5-3. `LordCharacter`

- [ ] ステータスは `Popularity` / `Negotiation` / `Luck` / `Support` / `Thinking`。
- [ ] 全ステータスは 0〜100 にクランプする。
- [ ] メタポイントによる上昇コスト式 `floor(CurrentStat / 5) + 1` を利用できる。
- [ ] 交渉力・運・支援力・思考力の効果は、Core 側の計算に渡せる形にする。

---

## 6. UI 実装チェックリスト

### 6-1. メイン画面

- [ ] 現在ターン、年、月を表示する。
- [ ] ターン終了ボタンを配置する。
- [ ] ボタン押下は Core のターン進行 API を呼ぶだけにする。

### 6-2. リソース表示

- [ ] 食料、資金、人口、素材総量を表示する。
- [ ] 素材詳細として、石材・木材・金属材・食材・魔法素材を等級別に確認できる。
- [ ] 食料不足、資金マイナス、幸福度危機を警告表示する。

### 6-3. 研究 UI

- [ ] 研究ノードグラフをカテゴリ別に表示する。
- [ ] 未解放、研究可能、研究中、完了を視覚的に区別する。
- [ ] 前提ノード、コスト、必要人員、所要ターン、効果を表示する。
- [ ] 同時研究上限が Research 行動に割り当てたギルド員数であることを表示する。

### 6-4. マップ・探索 UI

- [ ] 24×24 のトップダウン Tilemap を表示できる。
- [ ] 草地、岩地、川、ダンジョン入口を区別できる。
- [ ] 襲撃起点探索の進捗率を表示する。
- [ ] 探索成功/失敗と進捗増加量を表示する。

### 6-5. 施設建設 UI

- [ ] 建設可能施設一覧を表示する。
- [ ] 施設レベル、建設/強化コスト、素材要求、維持費、効果を表示する。
- [ ] 資源不足時は建設/強化できない。
- [ ] 建設位置が建設可能タイルかを表示する。

### 6-6. ギルド管理 UI

- [ ] ギルド別の所属員数、上限、解放状態を表示する。
- [ ] 各ギルド員のレベル、経験値、戦力、スキル力、現在行動を表示する。
- [ ] 行動割り当てを変更できる。
- [ ] 魔法ギルド・職人ギルドは未解放時にロック表示する。

### 6-7. 襲撃・幸福度・周回終了 UI

- [ ] 襲撃発生時に敵戦力、都市防衛力、判定結果を表示する。
- [ ] 幸福度と閾値状態を表示する。
- [ ] 周回終了時にスコア、獲得メタポイント、特殊アイテム清算を表示する。
- [ ] メタポイント割り振り UI で領主ステータス上昇コストを表示する。

---

## 7. 完成判定

Codex 担当分は以下を満たしたら完成とする。

- [ ] Unity コンパイルエラーがない。
- [ ] `docs/RULES.md` の命名規則に従っている。
- [ ] 1ファイル1クラスを守っている。
- [ ] MonoBehaviour にゲームロジックがない。
- [ ] 施設11種がすべて存在し、`BuildingData` 経由で動作する。
- [ ] ギルド3種、`GuildMember`、`LordCharacter` が存在する。
- [ ] UIから主要状態を確認でき、ターン終了・研究選択・行動割り当て・施設建設の入口がある。
- [ ] 数値は `SPEC.md`、`GameConstants`、ScriptableObject のいずれかから取得している。
- [ ] Codex担当外の Core 不足点がある場合、実装メモに明記している。

---

## 8. 実装順序

1. [ ] Claude Code が Unity プロジェクト、Core 型、Data 型を用意する。
2. [ ] Codex が `GuildMember` と `LordCharacter` を実装する。
3. [ ] Codex がギルド3種を実装する。
4. [ ] Codex が施設11種を実装する。
5. [ ] Codex が UI の表示専用部分を実装する。
6. [ ] Codex が UI 操作を Core API に接続する。
7. [ ] Claude Code がレビューし、Core 連携の不足を修正する。

---

## 9. 実装メモ欄

Codex は実装中に Core 側の不足を見つけたら、以下の形式で追記する。

```md
### YYYY-MM-DD Codex 実装メモ

- 必要なCore API:
- 理由:
- 暫定回避:
- レビュー依頼:
```

### 2026-05-07 Codex 実装メモ

- 必要なCore API:
  - `GameManager.TryHireGuildMember`
  - `GameManager.TryBuildOrUpgradeBuilding`
  - `GameManager.GetGuildHireCost`
  - `GameState.SpentMetaPoints`
- 変更点:
  - `GuildPanel` にギルド選択と加入処理を追加
  - `BuildPanel` に建設対象選択と建設/強化処理を追加
  - `MetaScreen` のメタポイント消費を `GameState` に保持するよう変更
  - `MainGameScreen` から `BuildPanel` を `GameManager` 経由で bind するよう変更
- 確認した依存:
  - 建設生成は `BuildingEffectType` と既存 building class の対応に従う
  - 加入コストは `GuildData.hireCostFunds` と `GameState.HireCostReductionPercent` を使用
  - メタポイント残量は `MetaProgressionSystem.CalculateEarnedMetaPoints(state) - state.SpentMetaPoints`
- レビュー観点:
  - UI の serialized field 追加に伴い Scene/Generator 側の割当確認が必要
  - Unity Editor 上でボタン配線と表示更新を要確認
