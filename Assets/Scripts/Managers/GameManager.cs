using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [ SerializeField ] Transform players;
    [ SerializeField ] Transform enemies;
    [ SerializeField ] GameObject[] playersArr;
    [ SerializeField ] Transform projectiles;

    // TEMP
    [ SerializeField ] GameObject[] testEnemies;
    
    [ SerializeField ] bool spawnEnemies = false;

    [ SerializeField ] Transform damageNumbersParent;
    [ SerializeField ] GameObject damageNumberPrefab;
    [ SerializeField ] int damageNumberPoolSize = 100;
    DamageNumber[] _damageNumberPool;

    [ SerializeField ] TextMeshProUGUI fpsDisplay;
    [ SerializeField ] GameObject pauseScreen;

    public static GameManager Instance { get; private set; }
    private bool _fpsLimitOn;

    bool _paused;
    int _pausedBy;
    
    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad( this );
        }

        InvokeRepeating( nameof( SpawnSkull ), 2f, 10f );
        InvokeRepeating( nameof( CheckForNextWave ), 1f, 0.25f );

        _damageNumberPool = new DamageNumber[ damageNumberPoolSize ];
        for( var i = 0; i < damageNumberPoolSize; i++ )
        {
            _damageNumberPool[ i ] = Instantiate( damageNumberPrefab, damageNumbersParent ).GetComponent<DamageNumber>();
            _damageNumberPool[ i ].gameObject.SetActive( false );
        }
    }

    void Start()
    {
        InvokeRepeating( nameof( UpdateFpsDisplay ), 0f, 0.1f );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.Semicolon ) )
        {
            _fpsLimitOn = !_fpsLimitOn;
            Application.targetFrameRate = _fpsLimitOn ? 60 : -1;
        }
    }

    void SpawnSkull()
    {
        if( !spawnEnemies ) return;
        var maxX = playersArr.Max( p => p.transform.position.x );
        for( var i = 0; i < NumPlayers() + 2; i++ )
        {
            this.Invoke( () => EnemySpawner.Instance.Spawn( testEnemies.RandomEntry(),
                new Vector3( Random.Range( maxX - 20f, maxX + 20f ), 0, Random.Range( 2, 20 ) ) ), i * 0.5f );
        }
    }

    void CheckForNextWave()
    {
        var maxX = playersArr.Max( p => p.transform.position.x );
        if( maxX > EnemySpawner.Instance.NextXTrigger() )
        {
            var waveSize = EnemySpawner.Instance.NextWaveSize();
            var spawnPoints = ValidSpawnPointsAround( maxX, waveSize );
            EnemySpawner.Instance.LetItRip( spawnPoints );
        }
    }

    Vector3[] ValidSpawnPointsAround( float centerX, int num )
    {
        var spawnPoints = new Vector3[ num ];
        for( var i = 0; i < spawnPoints.Length; i++ )
        {
            spawnPoints[ i ] = new Vector3( centerX + Random.Range( -10f, 10f ), 0, Random.Range( 2, 20 ) );
        }
        return spawnPoints;
    }

    public void AwardGold( int spawnCost )
    {
        // Award gold to all players based on the spawn cost of the killed enemy
    }

    // This implementation is for testing purposes
    public GameObject SpawnPlayer( int playerId )
    {
        playersArr[ playerId ].SetActive( true );
        return playersArr[ playerId ];
    }

    public int NumPlayers() => playersArr.Count( p => p.activeInHierarchy );

    public void SpawnDamageNumber( Vector3 pos, int dmg, bool friendly )
    {
        foreach( var dn in _damageNumberPool )
        {
            if( dn.gameObject.activeInHierarchy )   continue;
            dn.Play( pos, dmg, friendly );
            return;
        }
    }

    void UpdateFpsDisplay() => fpsDisplay.text = (int) ( 1f / Time.unscaledDeltaTime ) + " FPS";

    public Transform Projectiles() => projectiles;
    public Transform Players() => players;

    public void PauseGame( int playerId )
    {
        switch( _paused )
        {
            case true when playerId == _pausedBy:
                Time.timeScale = 1;
                _paused = false;
                pauseScreen.SetActive( false );
                break;
            case false:
                Time.timeScale = 0;
                _paused = true;
                pauseScreen.SetActive( true );
                _pausedBy = playerId;
                break;
        }
    }

    public bool Paused() => _paused;
}