using System.Text;
using UnityEngine;
using TMPro;

#pragma warning disable 0649
public class MapPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text mapText;
    [SerializeField] private TMP_Text legendText;

    private MapData _mapData;

    public void Bind(MapData mapData)
    {
        _mapData = mapData;
        Refresh();
    }

    public void Refresh(MapData mapData)
    {
        _mapData = mapData;
        Refresh();
    }

    public void Refresh()
    {
        if (_mapData == null)
        {
            SetText(mapText, "マップ: 未接続");
            SetText(legendText, "凡例: .草地 #岩地 ~川 Dダンジョン R襲撃起点");
            return;
        }

        SetText(mapText, BuildMapText(_mapData));
        SetText(legendText, "凡例: .草地 #岩地 ~川 Dダンジョン R襲撃起点");
    }

    private static string BuildMapText(MapData mapData)
    {
        StringBuilder builder = new StringBuilder();
        for (int y = mapData.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                builder.Append(ToMapChar(mapData.GetTile(x, y)));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static char ToMapChar(MapTileType tileType)
    {
        if (tileType == MapTileType.Rock)
        {
            return '#';
        }

        if (tileType == MapTileType.River)
        {
            return '~';
        }

        if (tileType == MapTileType.DungeonEntrance)
        {
            return 'D';
        }

        if (tileType == MapTileType.InitialRaidOrigin)
        {
            return 'R';
        }

        return '.';
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
#pragma warning restore 0649
