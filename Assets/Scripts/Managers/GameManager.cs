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
    [ SerializeField ] GameObject skullEnemy;
    [ SerializeField ] bool spawnEnemies = false;

    [ SerializeField ] Transform damageNumbersParent;
    [ SerializeField ] GameObject damageNumberPrefab;
    [ SerializeField ] int damageNumberPoolSize = 100;
    DamageNumber[] _damageNumberPool;

    [ SerializeField ] TextMeshProUGUI fpsDisplay;

    public static GameManager Instance { get; private set; }

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

        InvokeRepeating( nameof( SpawnSkull ), 2f, 8f );

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

    void SpawnSkull()
    {
        if( !spawnEnemies ) return;
        var maxX = playersArr.Max( p => p.transform.position.x );
        for( var i = 0; i < NumPlayers(); i++ )
        {
            Instantiate( skullEnemy, new Vector3( maxX + 30f, 0, Random.Range( 2, 20 ) ),
                Quaternion.identity, enemies );
        }
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

    public int NumPlayers() => playersArr.Count( p => !p.activeInHierarchy );

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
}