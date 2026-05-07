public class MapData
{
    private readonly MapTileType[,] _tiles;
    private readonly bool[,] _developedTiles;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int HomeX { get; private set; }
    public int HomeY { get; private set; }
    public int DevelopedTileCount { get; private set; }

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new MapTileType[width, height];
        _developedTiles = new bool[width, height];
    }

    public MapTileType GetTile(int x, int y)
    {
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, MapTileType tileType)
    {
        _tiles[x, y] = tileType;
    }

    public void SetHome(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return;
        }

        HomeX = x;
        HomeY = y;
        MarkDeveloped(x, y);
    }

    public bool IsDeveloped(int x, int y)
    {
        return IsInBounds(x, y) && _developedTiles[x, y];
    }

    public bool MarkDeveloped(int x, int y)
    {
        if (!IsInBounds(x, y) || _developedTiles[x, y])
        {
            return false;
        }

        _developedTiles[x, y] = true;
        DevelopedTileCount++;
        return true;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsDevelopableTile(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return false;
        }

        MapTileType tile = GetTile(x, y);
        return tile == MapTileType.Grass || tile == MapTileType.Rock;
    }

    public bool HasDevelopedAdjacentTile(int x, int y)
    {
        return IsDeveloped(x + 1, y)
            || IsDeveloped(x - 1, y)
            || IsDeveloped(x, y + 1)
            || IsDeveloped(x, y - 1);
    }

    public bool IsValidDevelopmentTarget(int x, int y)
    {
        return !IsDeveloped(x, y)
            && IsDevelopableTile(x, y)
            && HasDevelopedAdjacentTile(x, y);
    }

    public int GetDistanceFromHome(int x, int y)
    {
        return Abs(x - HomeX) + Abs(y - HomeY);
    }

    public bool TryGetNextDevelopmentCandidate(out int targetX, out int targetY, out int distanceFromHome)
    {
        targetX = -1;
        targetY = -1;
        distanceFromHome = int.MaxValue;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (IsDeveloped(x, y) || !IsDevelopableTile(x, y) || !HasDevelopedAdjacentTile(x, y))
                {
                    continue;
                }

                int distance = GetDistanceFromHome(x, y);
                if (distance < distanceFromHome)
                {
                    targetX = x;
                    targetY = y;
                    distanceFromHome = distance;
                }
            }
        }

        return targetX >= 0;
    }

    private static int Abs(int value)
    {
        return value < 0 ? -value : value;
    }
}
