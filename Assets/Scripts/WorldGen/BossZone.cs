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
    }

    public void SetStartEnd( int start, int end )
    {
        _leftSpikeStart = start;
        _leftSpikeEnd = end;
    }

    public void CloseLeft() => StartCoroutine( CloseLeftCoroutine() );
    public void OpenRight() => StartCoroutine( OpenRightCoroutine() );

    IEnumerator CloseLeftCoroutine()
    {
        var wait = new WaitForFixedUpdate();
        for( var frame = 0; frame < closeTransitionFrames; frame++ )
        {
            for( var i = _leftSpikeStart; i <= _leftSpikeEnd; i++ )
            {
                SetYPos( leftSpikes[ i ], JBB.Map( frame, 
                    0, closeTransitionFrames - 1, downPosY, upPosY ) );
            }
            yield return wait;
        }
    }
    
    IEnumerator OpenRightCoroutine()
    {
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
