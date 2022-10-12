using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    public static void PathFind( Vector2 startXY, Vector2 targetXY )
    {
        Debug.Log( $"Pathing from {startXY} to {targetXY}" );
    }

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
                if( i is 0 or 1 && Random.value < 0.01f)
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