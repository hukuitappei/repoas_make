# repoas_make

ローグライク領地経営シミュレーション — Unity 6.3 LTS

## セットアップ手順

### 前提条件

- [Unity Hub](https://unity.com/download) インストール済み
- Unity 6.3 LTS インストール済み（Unity Hub 経由）
- Git インストール済み
- Git LFS インストール済み

### Git LFS の有効化（1マシン1回）

```powershell
git lfs install
```

### Unity プロジェクトの作成

1. Unity Hub を開く。
2. "New project" をクリック。
3. テンプレート: **3D (URP)** を選択（Unity 6.3 LTS）。
4. Project location: `C:\Projects`（`repoas_make` フォルダを指定）。
5. Project name: `repoas_make`。
6. "Create project" をクリック。
7. Hub 完了後、以下を確認:

```powershell
ls C:\Projects\repoas_make
# Assets/, Packages/, ProjectSettings/ が存在すること
```

8. `.gitignore` が正しく機能しているか確認:

```powershell
git status
# Library/ が表示されないこと
```

9. Unity 生成ファイルをコミット:

```powershell
git add Packages ProjectSettings Assets
git commit -m "feat: Unity 6.3 URP プロジェクト初期化"
```

---

## ドキュメント

| ファイル | 説明 |
|---------|------|
| [DEVELOPMENT.md](DEVELOPMENT.md) | 開発チェックリスト・フェーズ計画 |
| [docs/RULES.md](docs/RULES.md) | コーディング規約・Git 運用・共通ルール |
| [docs/SPEC.md](docs/SPEC.md) | ゲーム仕様書（Codex への実装指示書） |

---

## ブランチ構成

| ブランチ | 用途 |
|---------|------|
| `main` | リリース安定版 |
| `develop` | 開発統合ブランチ |
| `feature/xxx` | 機能実装 |
| `fix/xxx` | バグ修正 |
