using System.Collections;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [ SerializeField ] Transform blocksParent;
    [ SerializeField ] Transform objsParent;
    [ SerializeField ] Transform edgesParent;
    [ SerializeField ] Transform wallsParent;

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

        // Generate base blocks
        for( var x = 0; x < worldSizeX; x++ )
        {
            for( var y = 0; y < worldSizeY; y++ )
            {
                var i = GrasslandsNoiseMap( x, y );
                var o = Instantiate( genBlocks[ i ],
                    CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile
                {
                    Ground = o,
                    X = x,
                    Y = y,
                    GenIndex = i,
                    Position = CoordsToWorldPos( x, y ),
                    EmptyCollider = i == 0
                };
            }
        }
        
        // Duplicate top row
        for( var x = 0; x < worldSizeX; x++ )
        {
            var toCopy = _tileMap[ x, worldSizeY - 1 ];
            var o = Instantiate( genBlocks[ toCopy.GenIndex ], 
                CoordsToWorldPos( x, worldSizeY ), JBB.Random90Rot(), blocksParent );
            _tileMap[ x, worldSizeY ] = new Tile
            {
                Ground = o,
                X = x,
                Y = worldSizeY,
                GenIndex = toCopy.GenIndex,
                Position = toCopy.Position
            };
        }

        // generate npc stuff
        // generate rivers, waterfalls etc.
        // generate trees n stuff in the gaps
        
        // find way to put consistent gaps between objects
        for( var x = 0; x < worldSizeX; x++ )
        {
            for( var y = 1; y < worldSizeY; y++ )
            {
                if( x < 20 && y > 4 && y < worldSizeY - 4 ) continue;
                
                if( !_tileMap[ x, y ].EmptyCollider && Random.value < 0.025f )
                {
                    Instantiate( genObjects[ Random.Range( 0, 4 ) ], 
                        CoordsToWorldPos( x, y ), JBB.RandomYRot(), blocksParent );
                    _tileMap[ x, y ].HasSurfaceObject = true;
                }
            }
        }
        
        // puts chests, campfires, etc near objects
        
        // GoofyFun();
    }

    void GoofyFun()
    {
        for( var d = 0; d < 32; d++ )
        {
            for( var x = d; x >= 0; x-- )
            {
                var y = d - x;
                if( y > worldSizeY + 1 )
                    break;

                var x1 = x;
                this.Invoke( () => StartCoroutine( Xd( _tileMap[ x1, y ].Ground.transform ) ), d * 0.05f );
            }
        }

        IEnumerator Xd( Transform t )
        {
            var ogPos = t.position;
            var wait = new WaitForFixedUpdate();
            t.position = new Vector3( ogPos.x, ogPos.y - 10, ogPos.z );

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
        if( x < 20 )    return WorldGenIndex.BlockGrassDark;
        
        var val = Mathf.PerlinNoise(
            ( x + _perlinSeed ) / worldSizeY + _perlinSeed,
            ( y + _perlinSeed ) / worldSizeY / 2f + _perlinSeed );
        
        return val < 0.2f ? WorldGenIndex.BlockEmpty : WorldGenIndex.BlockGrassDark;
    }

    public bool IsGenerating() => _generating;
}