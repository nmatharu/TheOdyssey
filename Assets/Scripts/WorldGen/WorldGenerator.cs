using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [ SerializeField ] DynamicCameras cameras;
    [ SerializeField ] public Transform blocksParent;
    [ SerializeField ] public Transform objsParent;
    [ SerializeField ] public Transform edgesParent;
    [ SerializeField ] public Transform wallsParent;
    [ SerializeField ] public Transform miscParent;

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
    [ SerializeField ] Level _currentLevel;
    int _numPlayers;

    public static WorldGenerator Instance { get; private set; }
    bool _generating;

    public static Vector2Int WorldPosToCoords( Vector3 pos ) =>
        new Vector2Int( (int) ( pos.x + 1 ) / 2, (int) ( pos.z + 1 ) / 2 );

    public static Vector3 CoordsToWorldPos( Vector2Int coords ) => new( 2 * coords.x, 0, 2 * coords.y );
    public static float CoordXToWorldX( float coordX ) => 2 * coordX;
    public static Vector3 CoordsToWorldPos( int x, int y ) => new( 2 * x, 0, 2 * y );
    public static Vector3 CoordsToWorldPos( float x, float y ) => new( 2 * x, 0, 2 * y );

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
        if( _currentLevel == null)
            _currentLevel = GetComponentInChildren<LevelSands>();

        _numPlayers = GameManager.Instance.NumPlayersInParty();
        Debug.Log( $"Generating world w/ {_numPlayers} players" );
        _currentLevel.gameObject.SetActive( true );
        StartCoroutine( Generate() );
    }

    IEnumerator Generate()
    {
        _generating = true;

        InitMaps( _currentLevel.WorldSize() );
        
        cameras.CalcXClampMax( WorldSizeX() );
        cameras.SetBossZoneX( _currentLevel.BossZoneCenterX() );
        _currentLevel.Generate( this );
        
        GameManager.Instance.SetCurrentLevel( _currentLevel );

        // GenerateGrasslands();
        // GenerateGrasslands2();
        
        yield return new WaitForSeconds( 2f );
        _generating = false;
    }

    void GenerateGrasslands()
    {
        _perlinSeed = Random.value;

        // Instantiate( Gen( WorldGenIndex.Misc.Water ), Vector3.zero, Quaternion.identity, miscParent );
        Instantiate( Gen( WorldGenIndex.Misc.Cliffs ), Vector3.zero, Quaternion.identity, edgesParent );
        GenerateGrassBase();
        
        var puzzlePlacements = new Vector2Int[] { new( 74, 4 ), new( 196, 5 ), new( WorldSizeX() - 21, 6 ) };
        foreach( var p in puzzlePlacements ) 
            GenerateLinkPuzzle( p.x, p.y );

        for( var x = 105; x < WorldSizeX() - 21; x += 60 )
            GenerateNpc( x );
        
        GenerateWaterfall( 0, 8 );
        for( var x = 30; x < WorldSizeX() - 21; x += 30 )
            GenerateWaterfall( x, Random.Range( 2, 7 ) );
        
        // GenerateLinkAltars();
        
        // generate npc stuff
        // generate rivers, waterfalls etc.
        // generate trees n stuff in the gaps
        
        GenerateEnvironmentalObjects();

        
        
        // this.Invoke( () => _bossZone.CloseLeft(), 5f );
        // this.Invoke( () => _bossZone.OpenRight(), 10f );

        // puts chests, campfires, etc near objects

        DuplicateTopRow( WorldSizeX(), WorldSizeY() );
        
        // GoofyFun();
    }

    public void GenerateBossZone( int xStart )
    {
        _bossZone = Instantiate( Gen( WorldGenIndex.Misc.BossZone ), 
                CoordsToWorldPos( xStart, 0 ), Quaternion.identity, GameManager.Instance.miscParent )
            .GetComponent<BossZone>();
        _bossZone.SetStartEnd( 3, 3 + 6 );
        for( var y = 0; y < WorldSizeY(); y++ )
        {
            _tileMap[ xStart, y ].OffLimits = true;
            _tileMap[ xStart, y ].HasSurfaceObject = true;

            if( y is < 4 or > 7 )
                Instantiate( Gen( WorldGenIndex.Objs.LinkPuzzleFence ), 
                    CoordsToWorldPos( xStart, y ), Quaternion.identity, objsParent );
        }
    }

    void GenerateGrasslands2()
    {
        _perlinSeed = Random.value;

        Instantiate( Gen( WorldGenIndex.Misc.Water ), Vector3.zero, Quaternion.identity, miscParent );
        Instantiate( Gen( WorldGenIndex.Misc.Cliffs ), Vector3.zero, Quaternion.identity, edgesParent );
        GenerateGrassBase();

        for( var x = 105; x < WorldSizeX() - 21; x += 60 )
            GenerateNpc( x );
        
        GenerateWaterfall( 0, 8 );
        for( var x = 30; x < WorldSizeX() - 21; x += 30 )
            GenerateWaterfall( x, Random.Range( 2, 7 ) );
        
        GenerateEnvironmentalObjects();

        _bossZone = Instantiate( Gen( WorldGenIndex.Misc.BossZone ), 
                CoordsToWorldPos( WorldSizeX() - 20, 0 ), Quaternion.identity, GameManager.Instance.miscParent )
            .GetComponent<BossZone>();
        _bossZone.SetStartEnd( 3, 3 + 6 );

        DuplicateTopRow( WorldSizeX(), WorldSizeY() );
    }

    public void GrasslandsPub()
    {
        for( var x = 105; x < WorldSizeX() - 21; x += 60 )
            GenerateNpc( x );
        
        GenerateWaterfall( 0, 8 );
        for( var x = 30; x < WorldSizeX() - 21; x += 30 )
            GenerateWaterfall( x, Random.Range( 2, 7 ) );

        GenerateEnvironmentalObjects();
    }

    void GenerateGrassBase()
    {
        for( var x = 0; x < WorldSizeX(); x++ )
        {
            for( var y = 0; y < WorldSizeY(); y++ )
            {
                var o = Instantiate( Gen( WorldGenIndex.Blocks.Grass ), 
                    CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile( o, x, y, (int) WorldGenIndex.Blocks.Grass, CoordsToWorldPos( x, y ), false );
            }
        }
    }

    public void GenerateBase( WorldGenIndex.Blocks block, int xs, int ys )
    {
        for( var x = 0; x < xs; x++ )
        {
            for( var y = 0; y < ys; y++ )
            {
                var o = Instantiate( Gen( block ), CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile( o, x, y, ( int ) block, CoordsToWorldPos( x, y ), false );
            }
        }
    }
    
    public void GenerateBase( GameObject ground, int xs, int ys )
    {
        for( var x = 0; x < xs; x++ )
        {
            for( var y = 0; y < ys; y++ )
            {
                var o = Instantiate( ground, CoordsToWorldPos( x, y ), JBB.Random90Rot(), blocksParent );
                _tileMap[ x, y ] = new Tile( o, x, y, 0, CoordsToWorldPos( x, y ), false );
            }
        }
    }

    void GenerateLinkPuzzle( int x, int size )
    {
        var pieces = new List<GameObject>();
        var startingPos = WorldSizeY() / 2 - size / 2;
            
        for( var y = 0; y < WorldSizeY(); y++ )
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
            var randomPoint = new Vector2Int( Random.Range( 70, WorldSizeX() - 21 ), Random.Range( 1, WorldSizeY() - 1 ) );
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
                    if( !_tileMap.WithinArrayBounds( x, y ) || y >= WorldSizeY() )
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
        var logY = WorldSizeY() / 2 + Random.Range( -3, 3 );
        for( var x = xStart; x < xStart + width; x++ )
        {
            for( var y = 0; y < WorldSizeY(); y++ )
                if( _tileMap[ x, y ].OffLimits )    break;
            
            for( var y = 0; y < WorldSizeY(); y++ )
            {
                Destroy( _tileMap[ x, y ].Ground );
                _tileMap[ x, y ].Ground = Instantiate( Gen( WorldGenIndex.Blocks.Empty ), 
                    CoordsToWorldPos( x, y ), Quaternion.identity, blocksParent );
                _tileMap[ x, y ].GenIndex = (int) WorldGenIndex.Blocks.Empty;
                _tileMap[ x, y ].OffLimits = true;
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
            new Vector3( 2 * ( xStart + width / 2f ) - 1, 0, WorldSizeY() * 2 ),
            Quaternion.identity, miscParent);
        w.GetComponent<WaterfallPfx>().SetWidth( width );
    }

    void GenerateNpc( int xStart )
    {
        Instantiate( Gen( WorldGenIndex.NPCs.NPCTest ), 
            CoordsToWorldPos( xStart + 2, WorldSizeY() - 2 ), Quaternion.identity, objsParent );
        for( var x = xStart; x < xStart + 5; x++ )
        {
            for( var y = WorldSizeY() - 1; y >= WorldSizeY() - 6; y-- )
            {
                if( y >= WorldSizeY() - 3 )
                    _tileMap[ x, y ].HasSurfaceObject = true;
                _tileMap[ x, y ].OffLimits = true;
            }
        }
    }

    void GenerateCrystalBarrier( int x )
    {
        for( var y = 0; y < WorldSizeY(); y++ )
        {
            _tileMap[ x, y ].OffLimits = true;
            _tileMap[ x, y ].HasSurfaceObject = true;

            Instantiate( Gen( y is >= 4 and <= 7 ? WorldGenIndex.Objs.CrystalGate : WorldGenIndex.Objs.LinkPuzzleFence ), 
                CoordsToWorldPos( x, y ), Quaternion.identity, objsParent );
        }
    }

    void GenerateEnvironmentalObjects()
    {
        for( var x = 21; x < WorldSizeX() - 21; x++ )
        {
            for( var y = 1; y < WorldSizeY(); y++ )
            {
                if( x < 20 && y > 4 && y < WorldSizeY() - 4 ) continue;

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
    
    public void GenerateShopsAndMagic( IEnumerable<int> shopXs, int magicX )
    {
        foreach( var x in shopXs )
            GenerateNpc( x );
        GenerateMagic( magicX );
    }

    public void GenerateMagic( int xMid )
    {
        foreach( var y in new[] { 2, 5, 8 } )
            Instantiate( Gen( WorldGenIndex.Objs.MagicShrine ), 
                CoordsToWorldPos( xMid, y ), Quaternion.identity, objsParent );

        for( var x = xMid - 2; x <= xMid + 2; x++ )
        {
            for( var y = 0; y < WorldSizeY(); y++ )
            {
                _tileMap[ x, y ].OffLimits = true;
            }
        }
    }

    public void DuplicateTopRow( int xs, int ys )
    {
        for( var x = 0; x < xs; x++ )
        {
            var toCopy = _tileMap[ x, ys - 1 ];
            Instantiate( toCopy.Ground, CoordsToWorldPos( x, ys ), JBB.Random90Rot(), blocksParent );
        }
    }

    void GoofyFun()
    {
        for( var d = 0; d < 40; d++ )
        {
            for( var x = d; x >= 0; x-- )
            {
                var y = d - x;
                if( y > WorldSizeY() )
                    continue;

                var x1 = x;
                _tileMap[ x1, WorldSizeY() - y ].Ground.SetActive( false );
                Debug.Log( $"invoking {x} {y}" );
                this.Invoke( () => StartCoroutine( Xd( _tileMap[ x1, WorldSizeY() - y ].Ground.transform ) ), x * 0.12f + y * 0.05f );
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

    // has no OffLimits squares
    public bool RectIsClear( int xStart, int width ) => RectIsClear( xStart, 0, width, WorldSizeY() );
    
    public bool RectIsClear( int xStart, int yStart, int width, int height )
    {
        for( var x = xStart; x < xStart + width; x++ )
        {
            for( var y = yStart; y < yStart + height; y++ )
            {
                if( _tileMap[ x, y ].OffLimits )
                {
                    return false;
                }
            }
        }
        return true;
    }

    public List<Vector2Int> GetSpacedOutPoints( int xStart, int width, int minSpace, int maxSpace ) =>
        GetSpacedOutPoints( xStart, 0, width, WorldSizeY(), minSpace, maxSpace );
    public List<Vector2Int> GetSpacedOutPoints( int xStart, int yStart, int width, int height, int minSpace, int maxSpace )
    {
        var points = new List<Vector2Int>();

        for( var x = xStart + Random.Range( 0, 4 ); x < xStart + width; x++ )
        {
            for( var y = yStart + Random.Range( 0, 4 ); y < yStart + height; y++ )
            {
                if( _tileMap[ x, y ].Spacing || _tileMap[ x, y ].OffLimits )
                    continue;
                
                points.Add( new Vector2Int( x, y ) );
                int spacingX = Random.Range( minSpace, maxSpace + 1 );
                int spacingY = Random.Range( minSpace, maxSpace + 1 );

                for( var sx = x - spacingX; sx <= x + spacingX && sx < WorldSizeX(); sx++ )
                {
                    for( var sy = y - spacingY; sy <= y + spacingY && sy < WorldSizeY(); sy++ )
                    {
                        if( sx == x + spacingX && sy == y + spacingY )
                            continue;
                        
                        if( !_tileMap.WithinArrayBounds( sx, sy ) )
                            continue;

                        _tileMap[ sx, sy ].Spacing = true;
                    }
                }
            }
        }
        
        for( var x = xStart; x < xStart + width; x++ )
            for( var y = yStart; y < yStart + height; y++ )
                _tileMap[ x, y ].Spacing = false;

        return points;
    }

    public bool OffMap( Vector3 pos )
    {
        var v2 = WorldPosToCoords( pos );
        return v2.x < -offMapThresholdX || v2.x > WorldSizeX() + offMapThresholdX ||
               v2.y < -offMapThresholdY || v2.y > WorldSizeY() + offMapThresholdY;
    }

    public Vector3[] ValidSpawnPointsAround( float centerX, int num )
    {
        var spawnPoints = new List<Vector3>();
        var maxIters = 200 + num;
        for( var i = 0; i < maxIters; i++ )
        {
            var range = 10 + 10 * i / maxIters;
            var point = new Vector3( centerX + Random.Range( -range, range ), 0,
                Random.Range( 2, ( WorldSizeY() - 2 ) * 2 ) );
            
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
                Random.Range( 2, ( WorldSizeY() - 2 ) * 2 ) ) );
        }
        
        return spawnPoints.ToArray();
    }

    public Vector3[] BossWaveSpawnPoints( int enemiesCount )
    {
        var spawnPoints = new Vector3[ enemiesCount ];
        for( var i = 0; i < spawnPoints.Length; i++ )
            spawnPoints[ i ] = _currentLevel.RandomBossZonePoint();
        return spawnPoints;
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
            ( x + _perlinSeed ) / WorldSizeY() + _perlinSeed,
            ( y + _perlinSeed ) / WorldSizeY() / 2f + _perlinSeed );
        
        return (int) ( val < 0.2f ? WorldGenIndex.Blocks.Empty : WorldGenIndex.Blocks.Grass );
    }

    public void MakeOffLimits( int x, int y, bool offLimits = true )
    {
        if( _tileMap.WithinArrayBounds( x, y ) )
            _tileMap[ x, y ].OffLimits = offLimits;
    }
    public void MakeOffLimits( Vector2Int p, bool offLimits = true ) => MakeOffLimits( p.x, p.y, offLimits );

    public bool IsGenerating() => _generating;

    public GameObject Gen( WorldGenIndex.Blocks block ) => genBlocks[ (int) block ];
    public GameObject Gen( WorldGenIndex.Objs obj ) => genObjects[ (int) obj ];
    public GameObject Gen( WorldGenIndex.Misc misc ) => genMisc[ (int) misc ];
    public GameObject Gen( WorldGenIndex.NPCs npc ) => genNpcs[ (int) npc ];

    public int NumPlayers() => _numPlayers;
    int WorldSizeX() => _tileMap.GetLength( 0 );
    int WorldSizeY() => _tileMap.GetLength( 1 );

    public void BossStarted() => cameras.BossLock();
    public void BossFinished() => cameras.BossUnlock();
    public GameObject BossSpawner() => _currentLevel.BossSpawner();
    public Vector3 BossZoneCenter() => CoordsToWorldPos( _currentLevel.BossZoneCenterCoords() );
    
    public Tile[ , ] Map() => _tileMap;
    public Tile Map( int x, int y ) => _tileMap[ x, y ];
    public Tile Map( Vector2Int p ) => _tileMap[ p.x, p.y ];
 
    public void ShowOffLimits()
    {
        for( var x = 0; x < WorldSizeX(); x++ )
        {
            for( var y = 0; y < WorldSizeY(); y++ )
            {
                if( _tileMap[ x, y ].OffLimits )
                    Instantiate( Gen( WorldGenIndex.Misc.DebugOffLimits ),
                        CoordsToWorldPos( x, y ), Quaternion.identity, miscParent );
            }
        }
    }

    void OnDrawGizmos()
    {
        // if( _tileMap == null )
        //     return;
        //
        // for( var x = 0; x < WorldSizeX(); x++ )
        // {
        //     Handles.Label( CoordsToWorldPos( x, 5 ), x.ToString() );
        // }
    }
}