using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Hive : MonoBehaviour
{
    [ SerializeField ] Transform spoutPos;
    [ SerializeField ] float lockOnSpeed = 1f;

    [ SerializeField ] int[] enemyBudgetPerPlayerCount = { 5, 7, 9 };
    [ SerializeField ] Enemy[] enemies;
    [ SerializeField ] Light pointLight;

    [ SerializeField ] float fireCooldown = 10f;
    [ SerializeField ] float fireStartDelay = 5f;

    [ SerializeField ] float fireChargeTime = 2f;
    [ SerializeField ] float fireLightIntensity = 5000;
    [ SerializeField ] float fireLightSustainSeconds = 1f;
    [ SerializeField ] float fireLightDropOffTime = 0.5f;

    [ SerializeField ] float enemySpawnDelay = 5f;

    [ SerializeField ] float explosionDamage = 8f;
    [ SerializeField ] int minExplosions = 4;
    [ SerializeField ] int maxExplosions = 24;

    [ SerializeField ] GameObject hiveExploder;
    HiveExploder[] _exploders;
    
    Transform _targetPlayer;
    Rigidbody _body;
    Enemy _enemy;
    int _budget;

    Light _sceneLight;
    float _sceneLightIntensity;
    [ SerializeField ] float sceneLightDarkenIntensity = 1f;
    
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _enemy = GetComponent<Enemy>();
        _budget = enemyBudgetPerPlayerCount[ GameManager.Instance.NumPlayersInParty() - 1 ];

        _sceneLight = FindObjectOfType<LevelFires>().gameObject.GetComponentInChildren<Light>();
        _sceneLightIntensity = _sceneLight.intensity;

        _exploders = new HiveExploder[ maxExplosions ];
        for( var i = 0; i < _exploders.Length; i++ )
        {
            var o = Instantiate( hiveExploder, transform.position, Quaternion.identity, EnemySpawner.Instance.EnemiesParent() );
            _exploders[ i ] = o.GetComponent<HiveExploder>();
        }

        InvokeRepeating( nameof( FindNewTarget ), 0, 1f );
        InvokeRepeating( nameof( SpawnGoons ), enemySpawnDelay, 2.5f );
        InvokeRepeating( nameof( Fire ), fireStartDelay, fireCooldown );
    }

    void Fire() => StartCoroutine( FireCoroutine() );

    IEnumerator FireCoroutine()
    {
        foreach( var e in _exploders )
            e.gameObject.SetActive( false );
        
        for( var elapsed = 0f; elapsed < fireChargeTime; elapsed += Time.deltaTime )
        {
            pointLight.intensity = Mathf.Lerp( 0, fireLightIntensity, elapsed / fireChargeTime );
            _sceneLight.intensity =
                Mathf.Lerp( _sceneLightIntensity, sceneLightDarkenIntensity, 5f * elapsed / fireChargeTime );
            yield return null;
        }

        var numExploders = ( int ) ( Mathf.Lerp( minExplosions, maxExplosions, 1 - _enemy.HpPct() ) );
        var wait = new WaitForSeconds( fireLightSustainSeconds / numExploders );
        var spawnPoints = WorldGenerator.Instance.BossWaveSpawnPoints( numExploders );

        for( var i = 0; i < numExploders; i++ )
        {
            _exploders[ i ].gameObject.SetActive( true );
            _exploders[ i ].transform.position = spawnPoints[ i ];
            _exploders[ i ].ShowIndicator();
            yield return wait;
        }

        for( var i = 0; i < numExploders; i++ )
        {
            _exploders[ i ].Explode( explosionDamage, _enemy.Level() );
            yield return wait;
        }
        
        for( var elapsed = 0f; elapsed < fireLightDropOffTime; elapsed += Time.deltaTime )
        {
            pointLight.intensity = JBB.Map( elapsed / fireLightDropOffTime, 0, 1, fireLightIntensity, 0 );
            _sceneLight.intensity = Mathf.Lerp( sceneLightDarkenIntensity, _sceneLightIntensity, elapsed / fireLightDropOffTime );
            yield return null;
        }

        yield return new WaitForSeconds( 1f );
        for( var i = 0; i < numExploders; i++ )
            _exploders[ i ].gameObject.SetActive( false );
    }

    void FixedUpdate()
    {
        _body.velocity = Vector3.zero;
    }

    void SpawnGoons()
    {
        var es = FindObjectsOfType<Enemy>();
        var currentInUse = es.Sum( e => e.spawnCost );

        if( currentInUse >= _budget - 2 )    return;
        
        
        var remainingBudget = _budget - currentInUse;
        
        var enemiesToSpawn = new List<Enemy>();
        for( var iters = 0; iters < 100 && remainingBudget > 0; iters++ )
        {
            var e = enemies.RandomEntry();
            if( e.spawnCost > remainingBudget ) continue;
            enemiesToSpawn.Add( e );
            remainingBudget -= e.spawnCost;
        }

        foreach( var e in enemiesToSpawn )
            EnemySpawner.Instance.LetItRipBossWave( new List<Enemy>{ e } );
    }

    void Update()
    {
        var pos = transform.position;
        if( _targetPlayer != null )
        {
            var look = _targetPlayer.position - pos;
            var lookRot = look != Vector3.zero ? Quaternion.LookRotation( look ) : Quaternion.identity;
            var lookTarget = Quaternion.Slerp( transform.rotation, lookRot, lockOnSpeed * Time.deltaTime );
            transform.rotation = lookTarget;
        }
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var players = GameManager.Instance.Players();
        if( players.Empty() ) return;
        var closestPlayer = players[ 0 ].transform;

        foreach( var p in players )
            if( JBB.DistXZSquared( pos, p.transform.position ) < JBB.DistXZSquared( pos, closestPlayer.position ) )
                closestPlayer = p.transform;

        _targetPlayer = closestPlayer;
    }
}