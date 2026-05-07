using System.Text;
using TMPro;
using UnityEngine;

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
            SetText(legendText, "凡例: H中心地 o開拓地 .平地 #岩場 ~川 Dダンジョン R襲撃元");
            return;
        }

        SetText(mapText, BuildMapText(_mapData));
        SetText(legendText, $"凡例: H中心地 o開拓地 .平地 #岩場 ~川 Dダンジョン R襲撃元 / 開拓済み {_mapData.DevelopedTileCount} マス");
    }

    private static string BuildMapText(MapData mapData)
    {
        StringBuilder builder = new StringBuilder();
        for (int y = mapData.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                builder.Append(ToMapChar(mapData, x, y));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static char ToMapChar(MapData mapData, int x, int y)
    {
        if (x == mapData.HomeX && y == mapData.HomeY)
        {
            return 'H';
        }

        if (mapData.IsDeveloped(x, y))
        {
            return 'o';
        }

        MapTileType tileType = mapData.GetTile(x, y);
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
