using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [ SerializeField ] public Transform effectsParent;
    [ SerializeField ] public Transform miscParent;
    [ SerializeField ] public Transform interactablesParent;
    [ SerializeField ] Player[] playersArr;
    [ SerializeField ] Transform projectiles;

    [ SerializeField ] Transform damageNumbersParent;
    [ SerializeField ] GameObject damageNumberPrefab;
    [ SerializeField ] int damageNumberPoolSize = 100;
    DamageNumber[] _damageNumberPool;

    [ SerializeField ] TextMeshProUGUI fpsDisplay;
    [ SerializeField ] GameObject pauseScreen;

    [ SerializeField ] public float respawnHealthPct = 0.25f;
    [ SerializeField ] float baseHealthRegen = 6;

    public static GameManager Instance { get; private set; }
    private bool _fpsLimitOn;

    bool _paused;
    int _pausedBy;
    int _enemyLevel = 1;

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

        _damageNumberPool = new DamageNumber[ damageNumberPoolSize ];
        for( var i = 0; i < damageNumberPoolSize; i++ )
        {
            _damageNumberPool[ i ] =
                Instantiate( damageNumberPrefab, damageNumbersParent ).GetComponent<DamageNumber>();
            _damageNumberPool[ i ].gameObject.SetActive( false );
        }
    }

    void Start()
    {
        InvokeRepeating( nameof( CheckForNextWave ), 1f, 0.25f );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.Semicolon ) )
        {
            _fpsLimitOn = !_fpsLimitOn;
            Application.targetFrameRate = _fpsLimitOn ? 60 : -1;
        }

        UpdateFpsDisplay();
    }

    void CheckForNextWave()
    {
        var maxX = playersArr.Max( p => p.transform.position.x );
        if( maxX > WorldGenerator.CoordXToWorldX( EnemySpawner.Instance.NextXTrigger() ) )
        {
            var waveSize = EnemySpawner.Instance.NextWaveSize();
            var spawnPoints = WorldGenerator.Instance.ValidSpawnPointsAround( maxX, waveSize );
            EnemySpawner.Instance.LetItRip( spawnPoints );
        }
    }

    public void AwardGold( int spawnCost )
    {
        foreach( var p in playersArr )
            if( p.gameObject.activeInHierarchy && !p.dead )
                p.AwardSoul();
    }

    // This implementation is for testing purposes
    public GameObject SpawnPlayer( int playerId )
    {
        playersArr[ playerId ].gameObject.SetActive( true );
        return playersArr[ playerId ].gameObject;
    }

    public void SpawnDamageNumber( Vector3 pos, int dmg, bool friendly )
    {
        foreach( var dn in _damageNumberPool )
        {
            if( dn.gameObject.activeInHierarchy ) continue;
            dn.Play( pos, dmg, friendly );
            return;
        }
    }

    public void SpawnCostNumber( Vector3 pos, int cost )
    {
        foreach( var dn in _damageNumberPool )
        {
            if( dn.gameObject.activeInHierarchy ) continue;
            dn.SpendMoney( pos, cost );
            return;
        }
    }

    public void SpawnGenericFloating( Vector3 pos, string s, Color color, float textSize )
    {
        foreach( var dn in _damageNumberPool )
        {
            if( dn.gameObject.activeInHierarchy ) continue;
            dn.Play( pos, s, color, textSize );
            return;
        }
    }

    void UpdateFpsDisplay() => fpsDisplay.text = (int) ( 1f / Time.unscaledDeltaTime ) + " FPS";

    public Transform Projectiles() => projectiles;

    public Player[] Players() =>
        ( from Player p in playersArr where !p.dead && p.gameObject.activeInHierarchy select p ).ToArray();

    public Player[] CameraPlayers() =>
        ( from Player p in playersArr where p.showOnCamera && p.gameObject.activeInHierarchy select p ).ToArray();

    int NumPlayersInParty() => playersArr.Count( p => p.gameObject.activeInHierarchy );

    // TODO take into account Level and Num players
    public float EnemyHealthMultiplier( int enemyLevel ) => ( 1f + ( NumPlayersInParty() - 1 ) * 0.5f ) * Mathf.Pow( 1.1f, enemyLevel - 1 );
    public float EnemyDamageMultiplier( int enemyLevel ) => ( 1f + ( NumPlayersInParty() - 1 ) * 0.5f ) * Mathf.Pow( 1.1f, enemyLevel - 1 );

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

    public void RespawnAll()
    {
        foreach( var p in playersArr )
            p.Respawn();
    }

    public int EnemyLevel() => _enemyLevel;

    public int RandomRunePrice( ItemDirector.RuneTiers tier )
    {
        return tier switch
        {
            ItemDirector.RuneTiers.Common => Random.Range( 14, 20 ),
            ItemDirector.RuneTiers.Rare => Random.Range( 26, 34 ),
            ItemDirector.RuneTiers.Legendary => Random.Range( 42, 52 ),
            _ => throw new ArgumentOutOfRangeException( nameof( tier ), tier, null )
        };
    }

    public int RandomRunePrice( Rune.RuneTier tier )
    {
        return tier switch
        {
            Rune.RuneTier.Common => Random.Range( 13, 19 ),
            Rune.RuneTier.Rare => Random.Range( 25, 33 ),
            Rune.RuneTier.Legendary => Random.Range( 41, 51 ),
            Rune.RuneTier.Primordial => Random.Range( 77, 99 ),
            _ => throw new ArgumentOutOfRangeException( nameof( tier ), tier, null )
        };
    }

    public float BaseHealthRegen() => baseHealthRegen;
}