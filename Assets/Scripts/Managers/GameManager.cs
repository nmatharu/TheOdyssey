using System;
using System.Collections;
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
    [ SerializeField ] EnemyLevelGraphic enemyLevelGraphic;
    [ SerializeField ] GameObject pauseScreen;

    [ SerializeField ] public float respawnHealthPct = 0.25f;
    [ SerializeField ] public float[] enemyBudgetPlayerCountScaling = { 1f, 1.5f, 2f };
    [ SerializeField ] public float[] enemyHealthPlayerCountScaling = { 1f, 4 / 3f, 3 / 2f };
    [ SerializeField ] public float[] baseHpRegenPerMinPerDifficulty = { 30f, 10f, 0f, 0f };
    [ SerializeField ] public float[] enemyDmgMultiplierPerDifficulty = { 0.5f, 1f, 1.5f, 1.5f };
    [ SerializeField ] public float[] levelIncrementTimeDifficultySeconds = { 90f, 60f, 40f, 25f };
    [ SerializeField ] public int[] initialGoldPerPlayerCount = { 15, 20, 25 };
    [ SerializeField ] public int[] totalChestGoldPerPlayerCount = { 36, 54, 72 };

    public static GameManager Instance { get; private set; }
    private bool _fpsLimitOn;

    bool _paused;
    int _pausedBy;
    int _enemyLevel = 1;

    float _runTimeElapsed = 0f;
    Coroutine _levelIncrementRoutine;

    Level _currentLevel;

    // 0 - casual, 1 - normal, 2 - brutal, 3 - unreal
    [ SerializeField ] int _difficulty = 1;

    // Lots of scaling in the game uses this 1 + 0.333x gain, provided 0-indexed and 1-indexed methods
    // How players health scales with stacks of vitality, e.g. 30 hp (0 stacks), 40 hp, 50 hp, etc.
    public static float Scale0( int n, float gain = 1/3f ) => 1f + ( gain * n );
    public static float Scale1( int n, float gain = 1/3f ) => 1f + ( gain * ( n - 1 ) );

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
            _damageNumberPool[ i ] = Instantiate( damageNumberPrefab, damageNumbersParent ).GetComponent<DamageNumber>();
            _damageNumberPool[ i ].gameObject.SetActive( false );
        }
    }

    void Start()
    {
        Application.targetFrameRate = -1;

        InvokeRepeating( nameof( CheckForNextWave ), 1f, 0.25f );
        _levelIncrementRoutine = StartCoroutine( LevelIncrementor() );
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

    public Level CurrentLevel() => _currentLevel;

    public void SetCurrentLevel( Level level )
    {
        _currentLevel = level;
        RequestNextWave();
    }

    public void RequestNextWave()
    {
        if( _currentLevel.OutOfWaves() )
        {
            EnemySpawner.Instance.OutOfWaves();
            return;
        }

        EnemySpawner.Instance.SetWave( _currentLevel.NextWave() );
    }

    void CheckForNextWave()
    {
        var maxX = playersArr.Max( p => p.transform.position.x );
        if( maxX > EnemySpawner.Instance.NextXTrigger() )
        {
            // var waveSize = EnemySpawner.Instance.NextWaveSize();
            // var spawnPoints = WorldGenerator.Instance.ValidSpawnPointsAround( maxX, waveSize );
            EnemySpawner.Instance.LetItRip2( maxX );
        }
    }

    public bool PlayersInBossZone( float zoneXStart )
    {
        if( AllPlayersDead() )
            return false;
        
        foreach( var p in playersArr )
        {
            if( p.gameObject.activeInHierarchy && !p.dead && p.transform.position.x < zoneXStart )
                return false;
        }

        return true;
    }

    public bool AllEnemiesDead() => 
        FindObjectsOfType<Enemy>().Length == 0 && FindObjectsOfType<SpawnPillar>().Length == 0;

    public void AwardGold( int spawnCost )
    {
        foreach( var p in playersArr )
            if( p.gameObject.activeInHierarchy && !p.dead )
                p.AwardCurrency( spawnCost );
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

    IEnumerator LevelIncrementor()
    {
        var elapsed = 0f;
        for( ;; )
        {
            var levelIncrementTimeSeconds = levelIncrementTimeDifficultySeconds[ _difficulty ];

            if( elapsed < levelIncrementTimeSeconds )
            {
                elapsed += Time.deltaTime;
            }
            else
            {
                _enemyLevel++;
                elapsed -= levelIncrementTimeSeconds;
            }

            _runTimeElapsed += Time.deltaTime;

            enemyLevelGraphic.UpdateGraphic( _enemyLevel, elapsed / levelIncrementTimeSeconds, _runTimeElapsed );
            yield return null;
        }
    }

    void UpdateFpsDisplay() => fpsDisplay.text = (int) ( 1f / Time.unscaledDeltaTime ) + " FPS";

    public Transform Projectiles() => projectiles;

    public Player[] Players() =>
        ( from Player p in playersArr where !p.dead && p.gameObject.activeInHierarchy select p ).ToArray();

    public Player[] CameraPlayers() =>
        ( from Player p in playersArr where p.showOnCamera && p.gameObject.activeInHierarchy select p ).ToArray();

    public int NumPlayersInParty() => playersArr.Count( p => p.gameObject.activeInHierarchy );
    public bool AllPlayersDead() => playersArr.All( p => !p.gameObject.activeInHierarchy || p.dead );
    
    public float EnemyHealthMultiplier( int enemyLevel ) =>
        enemyHealthPlayerCountScaling[ NumPlayersInParty() - 1 ] * Mathf.Sqrt( Scale1( enemyLevel ) );

    public float EnemyDamageMultiplier( int enemyLevel ) => 
        enemyDmgMultiplierPerDifficulty[ _difficulty ] * Mathf.Sqrt( Scale1( enemyLevel ) );

    public float EnemyWaveBudgetMultiplier() => enemyBudgetPlayerCountScaling[ NumPlayersInParty() - 1 ];
    public float BaseHpRegenPerMin() => baseHpRegenPerMinPerDifficulty[ _difficulty ];

    public int GetInitGold() => initialGoldPerPlayerCount[ NumPlayersInParty() - 1 ]; 

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

    public int RandomRunePrice( Rune.RuneTier tier )
    {
        var basePrice = tier switch
        {
            Rune.RuneTier.Common => Random.Range( 10, 16 + 1 ),
            Rune.RuneTier.Rare => Random.Range( 22, 28 + 1 ),
            Rune.RuneTier.Legendary => Random.Range( 46, 52 + 1 ),
            Rune.RuneTier.Primordial => Random.Range( 77, 99 ),
            _ => throw new ArgumentOutOfRangeException( nameof( tier ), tier, null )
        };

        return (int) ( basePrice * EnemyWaveBudgetMultiplier() );
    }

    public int ChestGoldAmount()
    {
        var expectedChestGold = (float) totalChestGoldPerPlayerCount[ NumPlayersInParty() - 1 ] / Level.ChestsPerPlayerPerStage;
        return Random.value switch
        {
            < 0.8f => (int) Random.Range( expectedChestGold * 0.8f, expectedChestGold * 1.2f ),
            < 0.96f => (int) Random.Range( expectedChestGold * 1.6f, expectedChestGold * 2.4f ),
            _ => (int) Random.Range( expectedChestGold * 3.2f, expectedChestGold * 4.8f )
        };
    }

    public float PctOfExpectedChestGold( int gold ) => 
        gold / ( (float) totalChestGoldPerPlayerCount[ NumPlayersInParty() - 1 ] / Level.ChestsPerPlayerPerStage );
}