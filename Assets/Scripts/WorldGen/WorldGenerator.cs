using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [ SerializeField ] GameObject[] genNpcs; // npcs to interact with, shops, quests, etc.

    Tile[ , ] _tileMap;
    Node[ , ] _nodeMap;

    BossZone _bossZone;

    float _perlinSeed;
    Level _currentLevel;

    public static WorldGenerator Instance { get; private set; }
    bool _generating;

    public static Vector2Int WorldPosToCoords( Vector3 pos ) =>
        new Vector2Int( (int) ( pos.x + 1 ) / 2, (int) ( pos.z + 1 ) / 2 );

    public static Vector3 CoordsToWorldPos( Vector2Int coords ) => new( 2 * coords.x, 0, 2 * coords.y );
    public static float CoordXToWorldX( float coordX ) => 2 * coordX;
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

        Instantiate( Gen( WorldGenIndex.Misc.Water ), Vector3.zero, Quaternion.identity, miscParent );
        Instantiate( Gen( WorldGenIndex.Misc.Cliffs ), Vector3.zero, Quaternion.identity, edgesParent );
        GenerateGrassBase();
        
        var puzzlePlacements = new Vector2Int[] { new( 74, 4 ), new( 196, 5 ), new( worldSizeX - 21, 6 ) };
        foreach( var p in puzzlePlacements ) 
            GenerateLinkPuzzle( p.x, p.y );

        for( var x = 105; x < worldSizeX - 21; x += 60 )
            GenerateNpc( x );
        
        GenerateWaterfall( 0, 8 );
        for( var x = 30; x < worldSizeX - 21; x += 30 )
            GenerateWaterfall( x, Random.Range( 2, 7 ) );
        
        // GenerateLinkAltars();
        
        // generate npc stuff
        // generate rivers, waterfalls etc.
        // generate trees n stuff in the gaps
        
        GenerateEnvironmentalObjects();

        _bossZone = Instantiate( Gen( WorldGenIndex.Misc.BossZone ), 
            CoordsToWorldPos( worldSizeX - 20, 0 ), Quaternion.identity, GameManager.Instance.miscParent )
            .GetComponent<BossZone>();
        _bossZone.SetStartEnd( 3, 3 + 6 );
        // this.Invoke( () => _bossZone.CloseLeft(), 5f );
        // this.Invoke( () => _bossZone.OpenRight(), 10f );

        // puts chests, campfires, etc near objects

        DuplicateTopRow();
        
        // GoofyFun();
    }

    void GenerateGrassBase()
    {
        for( var x = 0; x < worldSizeX; x++ )
        {
            for( var y = 0; y < worldSizeY; y++ )
            {
                var o = Instantiate( Gen( WorldGenIndex.Blocks.Grass ), 
                    CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile( o, x, y, (int) WorldGenIndex.Blocks.Grass, CoordsToWorldPos( x, y ), false );
            }
        }
    }

    void GenerateLinkPuzzle( int x, int size )
    {
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

            _tileMap[ x - 1, y ].OffLimits = true;
            _tileMap[ x, y ].OffLimits = true;
            _tileMap[ x + 1, y ].OffLimits = true;
        }

        var puzzle = new LinkPuzzle( pieces, true );
    }

    void GenerateLinkAltars()
    {
        var n = 5;
        var altars = new GameObject[ n ];
        var points = new List<Vector2Int>();

        int iters = 0;
        while( points.Count < n )
        {
            var randomPoint = new Vector2Int( Random.Range( 70, worldSizeX - 21 ), Random.Range( 1, worldSizeY - 1 ) );
            if( ValidPoint( randomPoint.x, randomPoint.y ) )
                points.Add( randomPoint );

            iters++;
            if( iters > 1000 )
            {
                Debug.Log( "hmmm" );
                break;
            }
        }

        bool ValidPoint( int middleX, int middleY )
        {
            for( var x = middleX - 2; x <= middleX + 2; x++ )
            {
                for( var y = middleY - 1; y <= middleY + 1; y++ )
                {
                    if( !_tileMap.WithinArrayBounds( x, y ) || y >= worldSizeY )
                    {
                        Debug.Log( "returning false bc array : " + x + " " + y );
                        return false;
                    }

                    // Debug.Log(
                        // $"{x} {y} has a surface obj? {_tileMap[ x, y ].HasSurfaceObject} is off limits? {_tileMap[ x, y ].OffLimits}" );
                    if( _tileMap[ x, y ].HasSurfaceObject || _tileMap[ x, y ].OffLimits )
                    {
                        Debug.Log( "returning false bc offlimits or object" );
                        return false;
                    }
                }
            }

            Debug.Log( "returning true" );
            return true;
        }
        
        points.Sort( ( p1, p2 ) => p1.x - p2.x );

        var pieces = new List<GameObject>();
        foreach( var p in points )
        {
            var o = Instantiate( Gen( WorldGenIndex.Objs.LinkAltar ), 
                CoordsToWorldPos( p.x, p.y ), Quaternion.identity, objsParent );
            pieces.Add( o );
        }

        var _ = new LinkPuzzle( pieces, true );
    }

    void GenerateWaterfall( int xStart, int width )
    {
        var logY = worldSizeY / 2 + Random.Range( -3, 3 );
        for( var x = xStart; x < xStart + width; x++ )
        {
            for( var y = 0; y < worldSizeY; y++ )
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

            _tileMap[ x <= 0 ? 0 : x - 1, logY ].OffLimits = true;
            _tileMap[ x, logY ].OffLimits = true;
            _tileMap[ x + 1, logY ].OffLimits = true;
        }
        var w = Instantiate( Gen( WorldGenIndex.Misc.WaterfallPfx ),
            new Vector3( 2 * ( xStart + width / 2f ) - 1, 0, worldSizeY * 2 ),
            Quaternion.identity, miscParent);
        w.GetComponent<WaterfallPfx>().SetWidth( width );
    }

    void GenerateNpc( int xStart )
    {
        Instantiate( Gen( WorldGenIndex.NPCs.NPCTest ), 
            CoordsToWorldPos( xStart + 2, worldSizeY - 2 ), Quaternion.identity, objsParent );
        for( var x = xStart; x < xStart + 5; x++ )
        {
            for( var y = worldSizeY - 1; y >= worldSizeY - 6; y-- )
            {
                if( y >= worldSizeY - 3 )
                    _tileMap[ x, y ].HasSurfaceObject = true;
                _tileMap[ x, y ].OffLimits = true;
            }
        }
    }

    void GenerateEnvironmentalObjects()
    {
        for( var x = 21; x < worldSizeX - 21; x++ )
        {
            for( var y = 1; y < worldSizeY; y++ )
            {
                if( x < 20 && y > 4 && y < worldSizeY - 4 ) continue;

                var val = Random.value;
                if( !_tileMap[ x, y ].EmptyCollider && !_tileMap[ x, y ].HasSurfaceObject && !_tileMap[ x, y ].OffLimits && val < 0.025f )
                {
                    if( val < 0.001f )
                    {
                        Instantiate( Gen( WorldGenIndex.Misc.Campfire ), CoordsToWorldPos( x, y ), 
                            JBB.RandomYRot(), objsParent );
                        _tileMap[ x, y ].HasSurfaceObject = true;
                        continue;
                    }

                    Instantiate( genObjects[ Random.Range( 0, 4 ) ], 
                        CoordsToWorldPos( x, y ), JBB.RandomYRot(), blocksParent );
                    _tileMap[ x, y ].HasSurfaceObject = true;
                }
            }
        }
    }

    void DuplicateTopRow()
    {
        for( var x = 0; x < worldSizeX; x++ )
        {
            var toCopy = _tileMap[ x, worldSizeY - 1 ];
            var o = Instantiate( genBlocks[ toCopy.GenIndex ], 
                CoordsToWorldPos( x, worldSizeY ), JBB.Random90Rot(), blocksParent );
            _tileMap[ x, worldSizeY ] = new Tile( o, x, worldSizeY, toCopy.GenIndex, CoordsToWorldPos( x, worldSizeY ),
                toCopy.EmptyCollider );
        }
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

    public Vector3[] ValidSpawnPointsAround( float centerX, int num )
    {
        var spawnPoints = new List<Vector3>();
        var maxIters = 200 + num;
        for( var i = 0; i < maxIters; i++ )
        {
            var range = 10 + 10 * i / maxIters;
            var point = new Vector3( centerX + Random.Range( -range, range ), 0,
                Random.Range( 2, ( worldSizeY - 2 ) * 2 ) );
            
            if( !InvalidSpawnPoint( point ) )
                spawnPoints.Add( point );
            
            if( spawnPoints.Count == num )
                break;
        }

        // If for some reason we didn't get our n points
        while( spawnPoints.Count < num )
        {
            Debug.Log( "Still looking for more points..." );
            spawnPoints.Add( new Vector3( centerX + Random.Range( -10, 10 ), 0, 
                Random.Range( 2, ( worldSizeY - 2 ) * 2 ) ) );
        }
        
        return spawnPoints.ToArray();
    }

    bool InvalidSpawnPoint( Vector3 pos )
    {
        var coords = WorldPosToCoords( pos );
        var tile = _tileMap[ coords.x, coords.y ];
        return tile.EmptyCollider || tile.Is( WorldGenIndex.Blocks.Log );
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
    GameObject Gen( WorldGenIndex.NPCs npc ) => genNpcs[ (int) npc ];
}