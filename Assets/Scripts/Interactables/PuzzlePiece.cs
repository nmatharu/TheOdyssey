using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : Interactable
{
    [ SerializeField ] GameObject puzzleLight;
    [ SerializeField ] ParticleSystem togglePfx;
    [ SerializeField ] ParticleSystem clearPfx;
    [ SerializeField ] bool altar;
    [ SerializeField ] float lockTime = 0.5f;
    
    int _index;
    LinkPuzzle _puzzle;
    
    bool _locked; // Cooldown on using puzzle
    bool _solved; // Puzzle completed, destruction animation
    
    public override void Interact( Player player )
    {
        Toggle();
        _puzzle.FlipLight( _index );
    }
    
    public void LinkToPuzzle( int index, LinkPuzzle puzzle )
    {
        _index = index;
        _puzzle = puzzle;
    }

    public void LightUp( bool lightUp ) => puzzleLight.SetActive( lightUp );
    public void Toggle()
    {
        togglePfx.Play();
        puzzleLight.SetActive( !Lit() );
        
        // _locked = true;
        // this.Invoke( () => _locked = false, lockTime );

        _puzzle.LockAll();
    }

    public override bool InteractionLocked( Player player ) => _locked || _solved;

    bool Lit() => puzzleLight.activeInHierarchy;

    public void Solve()
    {
        _solved = true;

        var timeToDestroy = 0.5f + 0.3f * ( _puzzle.Size() - _index - 1 );
        this.Invoke( () =>
        {
            Instantiate( clearPfx, transform.position, Quaternion.identity,
                GameManager.Instance.effectsParent ).Play();
            Destroy( gameObject, 0.05f );
        }, timeToDestroy );
    }

    public void Lock()
    {
        _locked = true;
        this.Invoke( () => _locked = false, lockTime );
    }
}
