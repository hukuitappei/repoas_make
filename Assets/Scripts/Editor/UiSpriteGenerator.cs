using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UiSpriteGenerator
{
    private const string SpritesRoot = "Assets/Sprites";
    private const string OutputFolder = "Assets/Sprites/UI";
    private const int Ppu = 100;

    // ---- Menu items --------------------------------------------------------

    [MenuItem("repoas/Generate UI Sprites")]
    public static void GenerateUiSprites()
    {
        EnsureFolder("Assets", "Sprites");
        EnsureFolder(SpritesRoot, "UI");

        // Panel backgrounds (96x96, 9-slice 14px border)
        SavePanel("panel_dark",   96, 96, C(18,  18,  32,  228), C(70, 100, 160, 255), 13, 2, 14);
        SavePanel("panel_light",  96, 96, C(238, 240, 250, 235), C(95, 110, 155, 255), 13, 2, 14);
        SavePanel("panel_header", 96, 28, C(24,  36,  68,  240), C(80, 115, 185, 255), 10, 2, 10);

        // Buttons (96x40, 9-slice 10px border)
        SavePanel("btn_normal",  96, 40, C(198, 203, 218, 245), C(65,  78, 120, 255), 10, 2, 10);
        SavePanel("btn_pressed", 96, 40, C(148, 155, 178, 245), C(45,  58, 100, 255), 10, 2, 10);
        SavePanel("btn_primary", 96, 40, C(45,  98,  175, 245), C(25,  65, 135, 255), 10, 2, 10);

        // Resource icons 48x48
        SaveCircleIcon("icon_food",       48, C( 25, 135,  25, 255), C(155, 235,  70, 255));
        SaveDiamondIcon("icon_funds",     48, C(185, 138,  10, 255), C(255, 228,  90, 255));
        SaveHexIcon("icon_population",    48, C( 25,  75, 195, 255), C(115, 175, 255, 255));
        SaveStarIcon("icon_happiness",    48, C(218, 175,  10, 255), C(255, 238, 120, 255));
        SavePlankIcon("icon_wood",        48, C( 98,  52,  14, 255), C(192, 128,  50, 255));
        SaveCircleIcon("icon_stone",      48, C(102, 102,  98, 255), C(192, 192, 185, 255));
        SaveTriangleIcon("icon_metal",    48, C(128, 138, 148, 255), C(212, 218, 228, 255));
        SaveDiamondIcon("icon_magic",     48, C( 98,  18, 195, 255), C(185, 105, 255, 255));

        AssetDatabase.Refresh();
        Debug.Log("[UiSpriteGenerator] 14 sprites generated in " + OutputFolder);
    }

    [MenuItem("repoas/Apply UI Sprites To Open Scene")]
    public static void ApplyUiSpritesToOpenScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before applying UI sprites.");
            return;
        }

        Sprite panelDark   = LoadSprite("panel_dark");
        Sprite panelLight  = LoadSprite("panel_light");
        Sprite panelHeader = LoadSprite("panel_header");
        Sprite btnNormal   = LoadSprite("btn_normal");
        Sprite btnPressed  = LoadSprite("btn_pressed");
        Sprite btnPrimary  = LoadSprite("btn_primary");

        if (panelDark == null)
        {
            Debug.LogWarning("[ApplySprites] Sprites not found. Run 'repoas/Generate UI Sprites' first.");
            return;
        }

        int applied = 0;

        // Apply panel backgrounds
        applied += ApplyPanelSprite<ResourcePanel>(panelDark);
        applied += ApplyPanelSprite<GuildPanel>(panelDark);
        applied += ApplyPanelSprite<BuildPanel>(panelDark);
        applied += ApplyPanelSprite<ResearchPanel>(panelDark);
        applied += ApplyPanelSprite<HappinessPanel>(panelLight);
        applied += ApplyPanelSprite<ExplorationPanel>(panelDark);
        applied += ApplyPanelSprite<MapPanel>(panelDark);
        applied += ApplyPanelSprite<MetaScreen>(panelLight);
        applied += ApplyPanelSprite<RaidPopup>(panelDark);

        // Apply button sprites to all buttons in scene
        Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button btn in buttons)
        {
            Image img = btn.GetComponent<Image>();
            if (img == null)
            {
                continue;
            }

            bool isPrimary = btn.gameObject.name.Contains("EndTurn")
                || btn.gameObject.name.Contains("Primary");
            img.sprite = isPrimary ? btnPrimary : btnNormal;
            img.type = Image.Type.Sliced;

            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = btn.spriteState;
            ss.pressedSprite = btnPressed;
            ss.highlightedSprite = btnNormal;
            btn.spriteState = ss;

            EditorUtility.SetDirty(btn);
            applied++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[ApplySprites] Done. {applied} components updated.");
    }

    // ---- Panel / rounded-rect sprite ------------------------------------

    private static void SavePanel(string name, int w, int h, Color32 fill, Color32 border, int cornerR, int borderT, int slice)
    {
        Color32[] px = Transparent(w * h);
        FillRR(px, w, h, 0, 0, w, h, cornerR, border);
        int ir = Mathf.Max(0, cornerR - borderT);
        FillRR(px, w, h, borderT, borderT, w - borderT * 2, h - borderT * 2, ir, fill);
        AddTopHighlight(px, w, h, borderT);
        SaveSprite(name, w, h, px, slice, slice, slice, slice);
    }

    private static void FillRR(Color32[] px, int tw, int th, int rx, int ry, int rw, int rh, int r, Color32 col)
    {
        for (int y = ry; y < ry + rh && y < th; y++)
        {
            for (int x = rx; x < rx + rw && x < tw; x++)
            {
                if (x >= 0 && y >= 0 && InsideRR(x - rx, y - ry, rw, rh, r))
                {
                    px[y * tw + x] = col;
                }
            }
        }
    }

    private static bool InsideRR(int x, int y, int w, int h, int r)
    {
        if (r <= 0) return x >= 0 && x < w && y >= 0 && y < h;
        if (x < r && y < r)           return D2(x, y, r - 1, r - 1) <= r * r;
        if (x >= w - r && y < r)      return D2(x, y, w - r, r - 1) <= r * r;
        if (x < r && y >= h - r)      return D2(x, y, r - 1, h - r) <= r * r;
        if (x >= w - r && y >= h - r) return D2(x, y, w - r, h - r) <= r * r;
        return x >= 0 && x < w && y >= 0 && y < h;
    }

    private static void AddTopHighlight(Color32[] px, int w, int h, int margin)
    {
        Color32 hl = C(255, 255, 255, 28);
        for (int y = h - margin - 1; y >= h - margin - 3 && y >= 0; y--)
        {
            for (int x = margin; x < w - margin; x++)
            {
                if (px[y * w + x].a > 0)
                {
                    px[y * w + x] = Blend(px[y * w + x], hl);
                }
            }
        }
    }

    // ---- Icon shapes -------------------------------------------------------

    private static void SaveCircleIcon(string name, int s, Color32 outer, Color32 inner)
    {
        Color32[] px = Transparent(s * s);
        float cx = (s - 1) * 0.5f;
        float cy = (s - 1) * 0.5f;
        float r = s * 0.5f - 2f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r)
                {
                    px[y * s + x] = Lerp32(inner, outer, d / r * 0.78f);
                }
            }
        }

        AddIconHighlight(px, s, cx, cy, r);
        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    private static void SaveDiamondIcon(string name, int s, Color32 outer, Color32 inner)
    {
        Color32[] px = Transparent(s * s);
        float cx = (s - 1) * 0.5f;
        float cy = (s - 1) * 0.5f;
        float r = s * 0.5f - 2.5f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float md = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                if (md <= r)
                {
                    px[y * s + x] = Lerp32(inner, outer, md / r * 0.78f);
                }
            }
        }

        AddIconHighlight(px, s, cx, cy, r);
        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    private static void SaveHexIcon(string name, int s, Color32 outer, Color32 inner)
    {
        Color32[] px = Transparent(s * s);
        float cx = (s - 1) * 0.5f;
        float cy = (s - 1) * 0.5f;
        float r = s * 0.5f - 2.5f;
        const float Sq32 = 0.8660254f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float fx = x - cx;
                float fy = y - cy;
                // Flat-top hexagon with circumradius r
                float inr = r * Sq32;
                bool inside = Mathf.Abs(fy) <= inr
                    && Mathf.Abs(fy * 0.5f + fx * Sq32) <= inr
                    && Mathf.Abs(fy * 0.5f - fx * Sq32) <= inr;
                if (inside)
                {
                    float d = Mathf.Sqrt(fx * fx + fy * fy);
                    px[y * s + x] = Lerp32(inner, outer, d / r * 0.78f);
                }
            }
        }

        AddIconHighlight(px, s, cx, cy, r);
        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    private static void SaveStarIcon(string name, int s, Color32 outer, Color32 inner)
    {
        Color32[] px = Transparent(s * s);
        float cx = (s - 1) * 0.5f;
        float cy = (s - 1) * 0.5f;
        float outerR = s * 0.5f - 2.5f;
        float innerR = outerR * 0.42f;
        const float SectorAngle = 2f * Mathf.PI / 4f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float fx = x - cx;
                float fy = y - cy;
                float angle = Mathf.Atan2(fy, fx);
                float dist = Mathf.Sqrt(fx * fx + fy * fy);
                float norm = ((angle % SectorAngle) + SectorAngle) % SectorAngle;
                float half = SectorAngle * 0.5f;
                float t = norm <= half ? norm / half : (SectorAngle - norm) / half;
                float starR = Mathf.Lerp(outerR, innerR, t);
                if (dist <= starR)
                {
                    px[y * s + x] = Lerp32(inner, outer, dist / outerR * 0.78f);
                }
            }
        }

        AddIconHighlight(px, s, cx, cy, outerR);
        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    private static void SavePlankIcon(string name, int s, Color32 dark, Color32 light)
    {
        Color32[] px = Transparent(s * s);
        const int Margin = 4;
        const int Count = 3;
        int totalH = s - Margin * 2;
        int ph = (totalH - (Count - 1) * 3) / Count;

        for (int p = 0; p < Count; p++)
        {
            int py = Margin + p * (ph + 3);
            for (int y = py; y < py + ph && y < s; y++)
            {
                float rowT = (float)(y - py) / ph;
                for (int x = Margin; x < s - Margin; x++)
                {
                    float colT = Mathf.Abs((x - s * 0.5f) / (s * 0.5f - Margin));
                    px[y * s + x] = Lerp32(light, dark, rowT * 0.45f + colT * 0.15f);
                }
            }

            if (py < s)
            {
                for (int x = Margin; x < s - Margin; x++)
                {
                    px[py * s + x] = Lerp32(new Color32(255, 255, 255, 200), light, 0.35f);
                }
            }
        }

        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    private static void SaveTriangleIcon(string name, int s, Color32 outer, Color32 inner)
    {
        Color32[] px = Transparent(s * s);
        float cx = (s - 1) * 0.5f;
        const int Margin = 4;
        float tipY = s - 1 - Margin;
        float baseY = Margin;
        float halfBase = cx - Margin;
        float height = tipY - baseY;

        for (int y = 0; y < s; y++)
        {
            float fy = y - baseY;
            if (fy < 0f || fy > height) continue;
            float prog = fy / height;
            float hw = halfBase * (1f - prog);

            for (int x = 0; x < s; x++)
            {
                float fx = Mathf.Abs(x - cx);
                if (fx <= hw)
                {
                    px[y * s + x] = Lerp32(inner, outer, fx / halfBase * 0.55f + prog * 0.25f);
                }
            }
        }

        float hlCx = cx - halfBase * 0.2f;
        float hlCy = baseY + height * 0.7f;
        AddIconHighlight(px, s, hlCx, hlCy, halfBase * 0.55f);
        SaveSprite(name, s, s, px, 0, 0, 0, 0);
    }

    // ---- Highlight overlay ------------------------------------------------

    private static void AddIconHighlight(Color32[] px, int s, float cx, float cy, float r)
    {
        float hlCx = cx - r * 0.22f;
        float hlCy = cy + r * 0.32f;
        float hlR = r * 0.38f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                if (px[y * s + x].a == 0) continue;
                float dx = x - hlCx;
                float dy = y - hlCy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < hlR)
                {
                    float t = (1f - d / hlR) * 0.42f;
                    px[y * s + x] = BlendWhite(px[y * s + x], t);
                }
            }
        }
    }

    // ---- Sprite save ------------------------------------------------------

    private static void SaveSprite(string name, int w, int h, Color32[] px, int bl, int bb, int br, int bt)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(px);
        tex.Apply();

        string path = OutputFolder + "/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;

        imp.textureType = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = Ppu;
        imp.filterMode = FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;
        imp.spriteImportMode = SpriteImportMode.Single;

        if (bl > 0 || bb > 0 || br > 0 || bt > 0)
        {
            imp.spriteBorder = new Vector4(bl, bb, br, bt);
        }

        imp.SaveAndReimport();
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(OutputFolder + "/" + name + ".png");
    }

    private static int ApplyPanelSprite<T>(Sprite sprite) where T : MonoBehaviour
    {
        T panel = UnityEngine.Object.FindFirstObjectByType<T>();
        if (panel == null) return 0;

        Image img = panel.GetComponent<Image>();
        if (img == null)
        {
            img = panel.gameObject.AddComponent<Image>();
        }

        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        EditorUtility.SetDirty(img);
        return 1;
    }

    // ---- Pixel utilities --------------------------------------------------

    private static Color32[] Transparent(int count)
    {
        Color32[] px = new Color32[count];
        Color32 clear = new Color32(0, 0, 0, 0);
        for (int i = 0; i < count; i++) px[i] = clear;
        return px;
    }

    private static Color32 Lerp32(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)Mathf.RoundToInt(a.r + (b.r - a.r) * t),
            (byte)Mathf.RoundToInt(a.g + (b.g - a.g) * t),
            (byte)Mathf.RoundToInt(a.b + (b.b - a.b) * t),
            (byte)Mathf.RoundToInt(a.a + (b.a - a.a) * t));
    }

    private static Color32 Blend(Color32 src, Color32 over)
    {
        float a = over.a / 255f;
        return new Color32(
            (byte)Mathf.Min(255, src.r + (int)(over.r * a)),
            (byte)Mathf.Min(255, src.g + (int)(over.g * a)),
            (byte)Mathf.Min(255, src.b + (int)(over.b * a)),
            src.a);
    }

    private static Color32 BlendWhite(Color32 src, float strength)
    {
        int add = (int)(255 * strength);
        return new Color32(
            (byte)Mathf.Min(255, src.r + add),
            (byte)Mathf.Min(255, src.g + add),
            (byte)Mathf.Min(255, src.b + add),
            src.a);
    }

    private static int D2(int x, int y, int cx, int cy)
    {
        int dx = x - cx;
        int dy = y - cy;
        return dx * dx + dy * dy;
    }

    private static Color32 C(byte r, byte g, byte b, byte a) => new Color32(r, g, b, a);

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
