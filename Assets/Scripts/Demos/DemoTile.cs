public class DemoTile
{
    readonly int _x;
    readonly int _y;
    int TileType { get; }
    TileTop _tileTop;

    const int Water = 3;
    
    public enum TileTop
    {
        None,
        Rock
    }
    
    public DemoTile( int x, int y, int tileType, TileTop tileTop )
    {
        _x = x;
        _y = y;
        TileType = tileType;
        _tileTop = tileTop;
    }

    public override string ToString() => $"Tile: {_x}, {_y}, {TileType}, {_tileTop}";

    public bool IsTraversable() => TileType != Water && _tileTop == TileTop.None;
    bool IsBuildableOver() => TileType == Water;
}