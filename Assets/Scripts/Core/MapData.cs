public class MapData
{
    private readonly MapTileType[,] _tiles;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new MapTileType[width, height];
    }

    public MapTileType GetTile(int x, int y)
    {
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, MapTileType tileType)
    {
        _tiles[x, y] = tileType;
    }
}
