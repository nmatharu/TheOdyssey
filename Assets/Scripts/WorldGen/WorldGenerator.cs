using System.Collections;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [ SerializeField ] Transform tilesParent;
    [ SerializeField ] Transform edgesParent;
    [ SerializeField ] Transform wallsParent;

    [ SerializeField ] int worldSizeX = 600;
    [ SerializeField ] int worldSizeY = 12;

    [ SerializeField ] int offMapThresholdX = 30;
    [ SerializeField ] int offMapThresholdY = 15;

    Tile[ , ] _tileMap;
    Node[ , ] _nodeMap;

    float _perlinSeed;
    Level _currentLevel;

    public static WorldGenerator Instance { get; private set; }
    bool _generating;

    public static Vector2Int WorldPosToCoords( Vector3 pos ) =>
        new Vector2Int( (int) ( pos.x + 1 ) / 2, (int) ( pos.z + 1 ) / 2 );

    public static Vector3 CoordsToWorldPos( Vector2Int coords ) => new( 2 * coords.x, 0, 2 * coords.y );

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
        
        // InitMaps( _currentLevel.WorldSize() );
        InitMaps( new Vector2Int( 600, 12 ) );
        // _currentLevel.Generate( _tileMap );
        
        yield return new WaitForSeconds( 2f );
        _generating = false;
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

    public bool IsGenerating() => _generating;
}