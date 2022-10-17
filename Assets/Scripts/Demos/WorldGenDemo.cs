using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenDemo : MonoBehaviour
{
    [ SerializeField ] GameObject[] tiles;
    [ SerializeField ] GameObject[] obstacles;
    [ SerializeField ] Transform tilesParent;

    [ SerializeField ] int worldSizeX;
    [ SerializeField ] int worldSizeY;
    [ SerializeField ] int worldGenStartX = -10;
    [ SerializeField ] int worldGenStartY = 0;
    public static GameObject[ , ] TileIndices;
    public static DemoTile[ , ] Tiles;
    float perlinSeed;

    bool fpsLimitOn = false;

    void Start()
    {
        // Application.targetFrameRate = 60;
        StartCoroutine( GenerateWorld() );
    }

    // Preliminary A* pathfinding demo:
    // https://www.youtube.com/watch?v=mZfyt03LDH4&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=4
    public static AStarNode PathFind( Vector2Int startXY, Vector2Int targetXY )
    {
        // Debug.Log( $"Pathing from {startXY} to {targetXY}" );
        var iters = 0;
        
        var open = new List<AStarNode>(); // set of nodes to be evaluated
        var closed = new HashSet<AStarNode>(); // set of nodes already evaluated
        var start = new AStarNode( startXY.x, startXY.y );
        open.Add( start );

        while( open.Count > 0 )
        {
            var curr = MinFromOpenList( open );
            open.Remove( curr );
            closed.Add( curr );

            if( NodeIsTarget( curr, targetXY ) )
            {
                // Debug.Log( $"Returning after {iters} iterations" );
                return curr;
            }

            // Debug.Log( $"{startXY} to {targetXY} : {iters} BEFORE FOREACH" );
            foreach( var neighbour in GetNeighbours( curr ) )
            {
                var neighbourTile = Tiles[ neighbour.X, neighbour.Y ];
                if( !neighbourTile.IsTraversable() || closed.Contains( neighbour ) )
                    continue;

                var newMovementCostToNeighbour = curr.GCost + GetDistance( curr, neighbour );
                if( newMovementCostToNeighbour < neighbour.GCost || !open.Contains( neighbour ) )
                {
                    neighbour.GCost = newMovementCostToNeighbour;
                    neighbour.HCost = GetDistance( neighbour, new AStarNode( targetXY.x, targetXY.y ) );
                    neighbour.Parent = curr;
                    
                    if( !open.Contains( neighbour ) )
                        open.Add( neighbour );
                    
                }
            }

            if( iters > 99 )
            {
                Debug.Log( $"Returning after {iters} iterations" );
                return curr;
            }

            // Debug.Log( $"{startXY} to {targetXY} : {iters} END OF LOOP" );
            iters++;
        }

        return null;
    }

    static int GetDistance( AStarNode a, AStarNode b )
    {
        var dstX = Mathf.Abs( a.X - b.X );
        var dstY = Mathf.Abs( a.Y - b.Y );
        if( dstX > dstY )
        {
            return 14 * dstY + 10 * ( dstX - dstY );
        }
        return 14 * dstX + 10 * ( dstY - dstX );
    }

    static List<AStarNode> GetNeighbours( AStarNode node )
    {
        var neighbours = new List<AStarNode>();
        for( var x = -1; x <= 1; x++ )
        {
            for( var y = -1; y <= 1; y++ )
            {
                if( x == 0 && y == 0 ) continue;

                var checkX = node.X + x;
                var checkY = node.Y + y;

                if( checkX >= 0 && checkX < Tiles.GetLength( 0 ) && checkY >= 0 && checkY < Tiles.GetLength( 1 ) )
                {
                    neighbours.Add( new AStarNode( checkX, checkY ) );
                }

            }
        }

        return neighbours;
    }

    static AStarNode MinFromOpenList( List<AStarNode> open )
    {
        var n = open[ 0 ];
        foreach( var o in open )
        {
            if( o.FCost() < n.FCost() || o.FCost() == n.FCost() && o.HCost < n.HCost )
            {
                n = o;
            }
        }

        return n;
    }

    public static bool NodeIsTarget( AStarNode node, Vector2Int target ) => node.X == target.x && node.Y == target.y;

    void Update()
    {
        // if( Input.GetKeyDown( KeyCode.P ) )
        // {
        //     ScreenCapture.CaptureScreenshot( "screenshot" + DateTime.Now.Millisecond + ".png" );
        // }

        if( Input.GetKeyDown( KeyCode.Semicolon ) )
        {
            fpsLimitOn = !fpsLimitOn;
            Application.targetFrameRate = fpsLimitOn ? 60 : -1;
        }
    }

    IEnumerator GenerateWorld()
    {
        perlinSeed = Random.value;
        // Debug.Log( perlinSeed );
        TileIndices = new GameObject[ worldSizeX, worldSizeY ];
        Tiles = new DemoTile[ worldSizeX, worldSizeY ];
        for( var x = worldGenStartX; x < worldSizeX + worldGenStartX; x++ )
        {
            for( var y = worldGenStartY; y < worldSizeY + worldGenStartY; y++ )
            {
                var i = GetTileIndexAt( x, y );
                DemoTile.TileTop tileTop = DemoTile.TileTop.None;
                if( i is 0 or 1 && Random.value < 0.01f )
                {
                    Instantiate( obstacles[ 0 ], new Vector3( x * 2, 0, y * 2 ),
                        Quaternion.identity, tilesParent );
                    tileTop = DemoTile.TileTop.Rock;
                }

                Tiles[ x, y ] = new DemoTile( x, y, i, tileTop );
                TileIndices[ x, y ] = Instantiate( tiles[ i ], new Vector3( x * 2, 0, y * 2 ),
                    Quaternion.identity, tilesParent );
            }
        }

        yield break;
    }

    int GetTileIndexAt( int x, int y )
    {
        var val = GetNoiseValueAt( x, y );
        switch( val )
        {
            case < 0.2f:
                return 3;
            case < 0.25f:
                return 2;
            case < 0.6f:
                return 1;
            default:
                return 0;
        }
    }

    float GetNoiseValueAt( int x, int y ) => Mathf.PerlinNoise(
        ( x + perlinSeed ) / worldSizeY + perlinSeed,
        ( y + perlinSeed ) / worldSizeY + perlinSeed );
}