using System;

public class MapGenerator
{
    private readonly Random _random;

    public MapGenerator()
    {
        _random = new Random();
    }

    public MapData Generate()
    {
        MapData mapData = new MapData(GameConstants.MAP_WIDTH, GameConstants.MAP_HEIGHT);
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                double roll = _random.NextDouble();
                if (roll < 0.70d)
                {
                    mapData.SetTile(x, y, MapTileType.Grass);
                }
                else if (roll < 0.85d)
                {
                    mapData.SetTile(x, y, MapTileType.Rock);
                }
                else
                {
                    mapData.SetTile(x, y, MapTileType.River);
                }
            }
        }

        PlaceSpecialTile(mapData, MapTileType.DungeonEntrance);
        PlaceInitialRaidOrigin(mapData);
        return mapData;
    }

    private void PlaceSpecialTile(MapData mapData, MapTileType tileType)
    {
        int x = _random.Next(0, mapData.Width);
        int y = _random.Next(0, mapData.Height);
        mapData.SetTile(x, y, tileType);
    }

    private void PlaceInitialRaidOrigin(MapData mapData)
    {
        int centerX = mapData.Width / 2;
        int centerY = mapData.Height / 2;
        int x;
        int y;

        do
        {
            x = centerX + _random.Next(-GameConstants.RAID_ORIGIN_MAX_DISTANCE, GameConstants.RAID_ORIGIN_MAX_DISTANCE + 1);
            y = centerY + _random.Next(-GameConstants.RAID_ORIGIN_MAX_DISTANCE, GameConstants.RAID_ORIGIN_MAX_DISTANCE + 1);
            x = Clamp(x, 0, mapData.Width - 1);
            y = Clamp(y, 0, mapData.Height - 1);
        }
        while (Math.Abs(x - centerX) + Math.Abs(y - centerY) > GameConstants.RAID_ORIGIN_MAX_DISTANCE);

        mapData.SetTile(x, y, MapTileType.InitialRaidOrigin);
    }

    private int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
