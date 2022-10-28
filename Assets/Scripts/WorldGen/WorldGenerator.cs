using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [ SerializeField ] Transform blocksParent;
    [ SerializeField ] Transform objsParent;
    [ SerializeField ] Transform edgesParent;
    [ SerializeField ] Transform wallsParent;
    [ SerializeField ] Transform miscParent;

    [ SerializeField ] int worldSizeX = 600;
    [ SerializeField ] int worldSizeY = 12;

    [ SerializeField ] int offMapThresholdX = 30;
    [ SerializeField ] int offMapThresholdY = 15;

    [ SerializeField ] GameObject[] genBlocks; // blocks to generate, e.g. grass, sand
    [ SerializeField ] GameObject[] genObjects; // objects to generate on top of blocks, e.g. trees, chests
    [ SerializeField ] GameObject[] genMisc; // misc objects in environment, e.g. waterfall particle systems

    Tile[ , ] _tileMap;
    Node[ , ] _nodeMap;

    float _perlinSeed;
    Level _currentLevel;

    public static WorldGenerator Instance { get; private set; }
    bool _generating;

    public static Vector2Int WorldPosToCoords( Vector3 pos ) =>
        new Vector2Int( (int) ( pos.x + 1 ) / 2, (int) ( pos.z + 1 ) / 2 );

    public static Vector3 CoordsToWorldPos( Vector2Int coords ) => new( 2 * coords.x, 0, 2 * coords.y );
    static Vector3 CoordsToWorldPos( int x, int y ) => new( 2 * x, 0, 2 * y );

    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _currentLevel = new LevelGrasslands();
        StartCoroutine( Generate() );
    }

    IEnumerator Generate()
    {
        _generating = true;

        InitMaps( _currentLevel.WorldSize() );
        // InitMaps( new Vector2Int( 600, 12 ) );
        // _currentLevel.Generate( this );

        GenerateGrasslands();
        
        yield return new WaitForSeconds( 2f );
        _generating = false;
    }

    void GenerateGrasslands()
    {
        _perlinSeed = Random.value;

        for( var x = 0; x < worldSizeX; x++ )
        {
            for( var y = 0; y < worldSizeY; y++ )
            {
                var o = Instantiate( Gen( WorldGenIndex.Blocks.Grass ), 
                    CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile( o, x, y, (int) WorldGenIndex.Blocks.Grass, CoordsToWorldPos( x, y ), false );
            }
        }

        // for( var x = 0; x < worldSizeX; x++ )
        // {
        //     for( var y = 0; y < worldSizeY; y++ )
        //     {
        //         if( GrasslandsNoiseMap( x, y ) == (int) WorldGenIndex.Blocks.Empty )
        //         {
        //             Destroy( _tileMap[ x, y ].Ground );
        //             _tileMap[ x, y ].Ground = Instantiate( Gen( WorldGenIndex.Blocks.Empty ), 
        //                 CoordsToWorldPos( x, y ), Quaternion.identity, blocksParent );
        //             _tileMap[ x, y ].GenIndex = (int) WorldGenIndex.Blocks.Empty;
        //             _tileMap[ x, y ].EmptyCollider = true;
        //         }
        //     }
        // }
        
        // // Generate base blocks
        // for( var x = 0; x < worldSizeX; x++ )
        // {
        //     for( var y = 0; y < worldSizeY; y++ )
        //     {
        //         var i = GrasslandsNoiseMap( x, y );
        //         var o = Instantiate( genBlocks[ i ],
        //             CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
        //         _tileMap[ x, y ] = new Tile
        //         {
        //             Ground = o,
        //             X = x,
        //             Y = y,
        //             GenIndex = i,
        //             Position = CoordsToWorldPos( x, y ),
        //             EmptyCollider = i == 0
        //         };
        //     }
        // }
        
        // Duplicate top row
        for( var x = 0; x < worldSizeX; x++ )
        {
            var toCopy = _tileMap[ x, worldSizeY - 1 ];
            var o = Instantiate( genBlocks[ toCopy.GenIndex ], 
                CoordsToWorldPos( x, worldSizeY ), JBB.Random90Rot(), blocksParent );
            _tileMap[ x, worldSizeY ] = new Tile( o, x, worldSizeY, toCopy.GenIndex, CoordsToWorldPos( x, worldSizeY ),
                toCopy.EmptyCollider );
        }

        // generate npc stuff
        // generate rivers, waterfalls etc.
        // generate trees n stuff in the gaps
        
        // waterfall at
        var waterfallX = 14;
        var waterfallWidth = Random.Range( 1, 5 );
        var logY = worldSizeY / 2 + Random.Range( -3, 3 );
        for( var x = waterfallX; x < waterfallX + waterfallWidth; x++ )
        {
            for( var y = 0; y < worldSizeY + 1; y++ )
            {
                Destroy( _tileMap[ x, y ].Ground );
                _tileMap[ x, y ].Ground = Instantiate( Gen( WorldGenIndex.Blocks.Empty ), 
                    CoordsToWorldPos( x, y ), Quaternion.identity, blocksParent );
                _tileMap[ x, y ].GenIndex = (int) WorldGenIndex.Blocks.Empty;
                _tileMap[ x, y ].EmptyCollider = true;
            }

            Destroy( _tileMap[ x, logY ].Ground );
            _tileMap[ x, logY ].Ground = Instantiate( Gen( WorldGenIndex.Blocks.Log ),
                CoordsToWorldPos( x, logY ), Quaternion.identity, blocksParent );
            _tileMap[ x, logY ].GenIndex = (int) WorldGenIndex.Blocks.Log;
            _tileMap[ x, logY ].EmptyCollider = false;
        }
        var w = Instantiate( Gen( WorldGenIndex.Misc.WaterfallPfx ),
            new Vector3( 2 * ( waterfallX + waterfallWidth / 2f ) - 1, 0, worldSizeY * 2 ),
            Quaternion.identity, miscParent);
        w.GetComponent<WaterfallPfx>().SetWidth( waterfallWidth );
        
        
        // find way to put consistent gaps between objects
        for( var x = 0; x < worldSizeX; x++ )
        {
            for( var y = 1; y < worldSizeY; y++ )
            {
                if( x < 20 && y > 4 && y < worldSizeY - 4 ) continue;
                
                if( !_tileMap[ x, y ].EmptyCollider && !_tileMap[ x, y ].HasSurfaceObject && Random.value < 0.025f )
                {
                    Instantiate( genObjects[ Random.Range( 0, 4 ) ], 
                        CoordsToWorldPos( x, y ), JBB.RandomYRot(), blocksParent );
                    _tileMap[ x, y ].HasSurfaceObject = true;
                }
            }
        }

        foreach( var x in new[] { 20, 100, 300 } )
        {
            var size = 4;
            var pieces = new List<GameObject>();
            var startingPos = worldSizeY / 2 - size / 2;
            
            for( var y = 0; y < worldSizeY; y++ )
            {
                _tileMap[ x, y ].HasSurfaceObject = true;

                if( y >= startingPos && y < startingPos + size )
                {
                    var o = Instantiate( Gen( WorldGenIndex.Objs.LinkPuzzle ), 
                        CoordsToWorldPos( x, y ), Quaternion.identity, objsParent );
                    pieces.Add( o );
                }
                else
                {
                    Instantiate( Gen( WorldGenIndex.Objs.LinkPuzzleFence ), 
                        CoordsToWorldPos( x, y ), Quaternion.identity, objsParent );
                }
            }

            var puzzle = new LinkPuzzle( pieces, true );
            
            break;
        }

        // puts chests, campfires, etc near objects
        
        // GoofyFun();
    }

    void GoofyFun()
    {
        for( var d = 0; d < 40; d++ )
        {
            for( var x = d; x >= 0; x-- )
            {
                var y = d - x;
                if( y > worldSizeY )
                    continue;

                var x1 = x;
                _tileMap[ x1, worldSizeY - y ].Ground.SetActive( false );
                Debug.Log( $"invoking {x} {y}" );
                this.Invoke( () => StartCoroutine( Xd( _tileMap[ x1, worldSizeY - y ].Ground.transform ) ), x * 0.12f + y * 0.05f );
            }
        }

        IEnumerator Xd( Transform t )
        {
            t.gameObject.SetActive( true );
            var ogPos = t.position;
            var wait = new WaitForFixedUpdate();
            t.position = new Vector3( ogPos.x, ogPos.y - 100, ogPos.z );

            for( var i = 0; i < 60; i++ )
            {
                t.position = Vector3.Lerp( t.position, ogPos, 0.1f );
                yield return wait;
            }

            t.position = ogPos;
        }
    }

    void InitMaps( Vector2Int size )
    {
        _tileMap = new Tile[ size.x, size.y ];
        _nodeMap = new Node[ size.x, size.y ];
    }

    public bool OffMap( Vector3 pos )
    {
        var v2 = WorldPosToCoords( pos );
        return v2.x < -offMapThresholdX || v2.x > worldSizeX + offMapThresholdX ||
               v2.y < -offMapThresholdY || v2.y > worldSizeY + offMapThresholdY;
    }

    int GrasslandsNoiseMap( int x, int y )
    {
        if( x < 20 )    return (int) WorldGenIndex.Blocks.Grass;
        
        var val = Mathf.PerlinNoise(
            ( x + _perlinSeed ) / worldSizeY + _perlinSeed,
            ( y + _perlinSeed ) / worldSizeY / 2f + _perlinSeed );
        
        return (int) ( val < 0.2f ? WorldGenIndex.Blocks.Empty : WorldGenIndex.Blocks.Grass );
    }

    public bool IsGenerating() => _generating;

    GameObject Gen( WorldGenIndex.Blocks block ) => genBlocks[ (int) block ];
    GameObject Gen( WorldGenIndex.Objs obj ) => genObjects[ (int) obj ];
    GameObject Gen( WorldGenIndex.Misc misc ) => genMisc[ (int) misc ];
}