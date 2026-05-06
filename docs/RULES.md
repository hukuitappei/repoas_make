# RULES.md — repoas_make 共通開発ルール

> このドキュメントは Claude Code・Codex・ユーザーの全員が従う共通規約です。
> コードを書く前に必ず読むこと。

---

## 1. 命名規則

| 対象 | 規則 | 例 |
|------|------|----|
| クラス名 | PascalCase | `GameManager`, `BuildingBase` |
| メソッド名 | PascalCase | `AdvanceTurn()`, `CalculateHappiness()` |
| プライベートフィールド | _camelCase | `_currentTurn`, `_foodAmount` |
| パブリックプロパティ | PascalCase | `CurrentTurn`, `FoodAmount` |
| 定数 | UPPER_SNAKE_CASE | `MAX_TURNS`, `BASE_FOOD_PRODUCTION` |
| ローカル変数 | camelCase | `totalFood`, `raidPower` |
| インターフェース | I + PascalCase | `IResearchable`, `IBuilding` |
| 列挙型 | PascalCase（型名・値ともに） | `ResourceType.Food` |

---

## 2. MonoBehaviour ルール

**MonoBehaviour は薄く保つ。**

MonoBehaviour に書いてよいのは以下のみ：
- `Start()`, `Update()`, `OnDestroy()` などのライフサイクル呼び出し
- UI イベントのデリゲート登録・解除

**ゲームロジックは純粋 C# クラス（MonoBehaviour を継承しないクラス）に書く。**

```csharp
// 良い例
public class TurnManagerMono : MonoBehaviour
{
    private TurnManager _turnManager;

    private void Start()
    {
        _turnManager = GameManager.Instance.TurnManager;
    }

    public void OnEndTurnButtonClicked()
    {
        _turnManager.AdvanceTurn();
    }
}

// 悪い例 — ロジックが MonoBehaviour に漏れている
public class TurnManagerMono : MonoBehaviour
{
    private int _currentTurn;

    public void OnEndTurnButtonClicked()
    {
        _currentTurn++;
        if (_currentTurn >= 60) { /* ゲーム終了ロジック */ }
    }
}
```

---

## 3. ScriptableObject ルール

- マスターデータ（施設コスト・研究コスト・初期リソース量など）は必ず ScriptableObject に定義する。
- **マジックナンバー禁止**。数値リテラルをコードに直接書かない。
- ScriptableObject のアセット（`.asset` ファイル）は `Assets/ScriptableObjects/` 以下に配置する。
- ScriptableObject クラスの定義（`.cs` ファイル）は `Assets/Scripts/Data/` に置く。

---

## 4. 定数ルール

クラスや ScriptableObject に切り出せない定数は `GameConstants.cs` または `XxxConstants.cs` に集約する。定数クラスは `static` クラスにする。

```csharp
public static class GameConstants
{
    public const int MAX_TURNS = 60;
    public const int TURNS_PER_YEAR = 12;
    public const int STARTING_POPULATION = 50;
}
```

---

## 5. ファイル・クラス対応ルール

- 1ファイル = 1クラス（ネスト型は例外）。
- ファイル名はクラス名と完全一致させる（例: `TurnManager.cs`）。

---

## 6. フォルダ構成

```
Assets/
├── Scripts/
│   ├── Core/         ← コアシステム全般（Claude Code 担当）
│   │                    GameManager, TurnManager, ResourceManager,
│   │                    ResearchTree, EventSystem, RaidSystem,
│   │                    DungeonSystem, GuildManager, HappinessSystem,
│   │                    MetaProgressionSystem
│   ├── Buildings/    ← 施設クラス（Codex 担当）
│   ├── Guilds/       ← ギルドクラス（Codex 担当）
│   ├── Characters/   ← 領主・ギルド員クラス（Codex 担当）
│   ├── UI/           ← UI コントローラー（Codex 担当）
│   └── Data/         ← ScriptableObject 定義クラス
├── Sprites/          ← [カテゴリ]_[名前]_[variant].png
├── Audio/            ← audio_[bgm/se]_[名前].[wav/ogg]
├── Prefabs/          ← Prefab アセット
└── ScriptableObjects/  ← ScriptableObject アセット（.asset ファイル）
```

---

## 7. 素材命名規則

```
[カテゴリ]_[名前]_[variant].[拡張子]
```

| カテゴリ | 例 |
|---------|-----|
| `facility` | `facility_farm_lv1.png`, `facility_barracks_lv2.png` |
| `guild` | `guild_warrior_member.png`, `guild_mage_icon.png` |
| `ui` | `ui_button_confirm.png`, `ui_panel_resources.png` |
| `tile` | `tile_grass_01.png`, `tile_dungeon_entrance.png` |
| `lord` | `lord_portrait_default.png` |
| `audio_bgm` | `audio_bgm_main.wav`, `audio_bgm_combat.ogg` |
| `audio_se` | `audio_se_button_click.wav`, `audio_se_raid_alert.wav` |

---

## 8. Git 運用ルール

### 8-1. ブランチ戦略

| ブランチ | 用途 |
|---------|------|
| `main` | リリース安定版のみ |
| `develop` | 開発統合ブランチ |
| `feature/xxx` | 機能単位の実装ブランチ |
| `fix/xxx` | バグ修正ブランチ |

### 8-2. コミット規約

粒度: 1機能・1修正 = 1コミット。

コミットメッセージ形式：
```
<type>: <内容（日本語可）>
```

| type | 用途 |
|------|------|
| `feat` | 新機能 |
| `fix` | バグ修正 |
| `docs` | ドキュメントのみ |
| `refact` | リファクタリング |
| `test` | テスト追加・修正 |
| `chore` | ビルド・設定変更 |

例: `feat: TurnManager の月進行ロジック実装`

### 8-3. バイナリ素材の管理

- 画像・音声・3Dモデルは全て Git LFS で管理（`.gitattributes` 参照）。
- `git lfs install` を事前に実行すること（1マシン1回）。
- `git lfs status` でトラッキング状態を確認してからコミットすること。

### 8-4. リリースタグ

| タグ | 条件 |
|-----|------|
| `v0.1` | コアゲームループ完成（ターン進行・リソース管理・基本イベント） |
| `v0.2` | UI 完成・1周 60 ターンプレイ可能 |
| `v0.3` | バランス調整済み・周回引き継ぎ動作確認済み |

---

## 9. Claude Code / Codex 連携プロトコル

1. Claude Code が仕様書（Markdown）を `docs/` に作成する。
2. ユーザーが Codex に仕様書を渡して実装を依頼する。
3. Codex が実装・コード提出する（`feature/xxx` ブランチ）。
4. Claude Code がコードレビューを実施し、修正指示を出す。
5. 問題なければ `develop` にマージする。

---

## 10. コードレビュー基準

Codex が提出したコードを Claude Code がレビューする際の確認事項：

- [ ] MonoBehaviour が薄いか（ロジックが純粋クラスに分離されているか）
- [ ] マジックナンバーがないか
- [ ] 命名規則に従っているか
- [ ] ScriptableObject を通じてマスターデータが外部化されているか
- [ ] 未使用の `using` 文がないか
- [ ] `null` チェックが適切か
- [ ] `public` フィールドが不必要に露出していないか（プロパティを使っているか）
- [ ] 例外処理・エラーハンドリングが適切か
- [ ] 1ファイル1クラス規則が守られているか
