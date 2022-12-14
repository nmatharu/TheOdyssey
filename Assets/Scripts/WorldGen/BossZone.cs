using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossZone : MonoBehaviour
{
    [ SerializeField ] GameObject[] leftSpikes;
    [ SerializeField ] GameObject[] rightSpikes;
    [ SerializeField ] ParticleSystem steamPfx;

    [ SerializeField ] float downPosY = -5f;
    [ SerializeField ] float upPosY = -1f;

    [ SerializeField ] int closeTransitionFrames = 15;

    int _leftSpikeStart;
    int _leftSpikeEnd;
    
    void Start()
    {
        foreach( var spike in leftSpikes )
            SetYPos( spike, downPosY );

        StartCoroutine( CheckStartBoss() );
    }

    public void SetStartEnd( int start, int end )
    {
        _leftSpikeStart = start;
        _leftSpikeEnd = end;
    }

    void Update()
    {
        // TODO Check if all players in zone
    }

    public void CloseLeft() => StartCoroutine( CloseLeftCoroutine() );
    public void OpenRight() => StartCoroutine( OpenRightCoroutine() );

    IEnumerator CheckStartBoss()
    {
        var wait = new WaitForSeconds( 1f );
        for( ;; )
        {
            if( GameManager.Instance.PlayersInBossZone( transform.position.x + 3f ) )
            {
                StartBoss();
                yield break;
            }
            yield return wait;
        }
    }

    void StartBoss()
    {
        // Debug.Log( "BOSS STARTED" );
        CloseLeft();

        GameManager.Instance.KillAllEnemies();
        WorldGenerator.Instance.BossStarted();
        this.Invoke( () => EnemySpawner.Instance.StartBoss(), 1.5f );
        this.Invoke( () => StartCoroutine( CheckEndBoss() ), 5f );
    }

    IEnumerator CheckEndBoss()
    {
        var wait = new WaitForSeconds( 1f );
        for( ;; )
        {
            if( FindObjectsOfType<HiveSpawner>().Length != 0 )
            {
                yield return wait;
                continue;
            }

            if( GameManager.Instance.AllEnemiesDead() )
            {
                WorldGenerator.Instance.BossFinished();

                var musicIndex = GameManager.Instance.CurrentLevel().musicIndex;
                if( musicIndex != 2 )
                {
                    this.Invoke( () => AudioManager.Instance.PlayMusic( AudioManager.Instance.
                            levelBossEndRiffs[ musicIndex ], false, true ), 0.1f );
                }

                OpenRight();
                yield break;
            }
            yield return wait;
        }
    }

    IEnumerator CloseLeftCoroutine()
    {
        AudioManager.Instance.spikes.PlaySfx();
        AudioManager.Instance.ToTargetMusicVolume( 0.25f, 0f );
        var wait = new WaitForFixedUpdate();
        for( var frame = 0; frame < closeTransitionFrames; frame++ )
        {
            for( var i = _leftSpikeStart; i < _leftSpikeEnd; i++ )
            {
                SetYPos( leftSpikes[ i ], JBB.Map( frame, 
                    0, closeTransitionFrames - 1, downPosY, upPosY ) );
            }
            yield return wait;
        }
    }
    
    IEnumerator OpenRightCoroutine()
    {
        AudioManager.Instance.spikes.PlaySfx();
        var wait = new WaitForFixedUpdate();
        for( var frame = 0; frame < closeTransitionFrames; frame++ )
        {
            foreach( var spike in rightSpikes )
            {
                SetYPos( spike, JBB.Map( frame, 
                    0, closeTransitionFrames - 1, upPosY, downPosY ) );
            }
            yield return wait;
        }
    }

    void SetYPos( GameObject spike, float posY )
    {
        var pos = spike.transform.position;
        spike.transform.position = new Vector3( pos.x, posY, pos.z );
    }
}
