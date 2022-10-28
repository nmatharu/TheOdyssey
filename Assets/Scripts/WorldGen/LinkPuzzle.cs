using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LinkPuzzle
{
    int _size;
    PuzzlePiece[] _pieces;
    List<int>[] _lightFlipMap;
    bool[] _lightMap;

    public LinkPuzzle( IReadOnlyList<GameObject> pieces, bool isAltar )
    {
        _size = pieces.Count;
        _pieces = new PuzzlePiece[ _size ];
        _lightFlipMap = new List<int>[ _size ];

        for( var i = 0; i < _size; i++ )
        {
            _pieces[ i ] = pieces[ i ].GetComponent<PuzzlePiece>();
            _pieces[ i ].LinkToPuzzle( i, this );
            _lightFlipMap[ i ] = new List<int>();
        }

        do
        {
            // Set up pieces
            for( var i = 0; i < _size; i++ )
            {
                // So a piece must flip at least 1 piece (excluding itself),
                // and at most, n - 1 pieces (all of them except itself)
                // var nFlips = Random.Range( 1, _size - 1 );
                var nFlips = Random.Range( 1, 3 );
                var flippers = new List<int>();
                for( var j = 0; j < _size; j++ )
                    if( j != i )
                        flippers.Add( j );

                // Shuffle and skim off the number you need
                _lightFlipMap[ i ] = new List<int>();
                flippers.Shuffle();
                for( var j = 0; j < nFlips; j++ )
                    _lightFlipMap[ i ].Add( flippers[ j ] );
            
                _lightFlipMap[ i ].Sort();
            }
        } while( AnyDuplicates() );

        _lightMap = new bool[ _size ];
        for( var i = 0; i < _size; i++ )
            _lightMap[ i ] = true;
       
        // Shuffle puzzle, do 50 flips first
        for( var i = 0; i < 50; i++ )
            FlipMap( Random.Range( 0, _size ) );
        
        // Make sure the puzzle takes at least 2 moves to complete
        while( !ComplexEnough( isAltar ) )
            FlipMap( Random.Range( 0, _size ) );

        // Set gameObject lights appropriately
        for( var i = 0; i < _size; i++ )
            _pieces[ i ].LightUp( _lightMap[ i ] );
    }

    // Can't be solved in one move
    bool ComplexEnough( bool isAltar )
    {
        Debug.Log( "Complex enough?" );
        if( Solved() ) return false;

        for( var i = 0; i < _size; i++ )
        {
            FlipMap( i );
            var solved = Solved();
            FlipMap( i );
            if( solved ) return false;
        }
        
        // Altar puzzles have the additional requirement that they cannot be solved by simply
        // hitting them left to right (which would be the most likely way a player could breeze through the
        // puzzle without trying-- just hitting them all as they pass by)
        if( isAltar )
        {
            var altarComplexEnough = true;
            for( var i = 0; i < _size; i++ )
            {
                FlipMap( i );
                if( Solved() ) altarComplexEnough = false;
            }

            for( var i = _size - 1; i >= 0; i-- )
                FlipMap( i );

            if( !altarComplexEnough ) Debug.Log( "Altar not complex enough" );
            return altarComplexEnough;
        }

        return true;
    }

    bool AnyDuplicates()
    {
        // Debug.Log( "checkin for duplicates " );
        var mapList = new List<List<int>>();
        for( var i = 0; i < _size; i++ )
        {
            var list = _lightFlipMap[ i ].ToList();
            list.Add( i );
            list.Sort();
            mapList.Add( list );
        }

        return mapList.Where( ( t1, i ) => 
            mapList.Where( ( t, j ) => i != j && t1.SequenceEqual( t ) ).Any() ).Any();
    }

    void FlipMap( int index )
    {
        Toggle( index );
        foreach( var otherI in _lightFlipMap[ index ] )
            Toggle( otherI );
        
    }

    bool Solved() => !_lightMap.Contains( false );
    void Toggle( int index ) => _lightMap[ index ] = !_lightMap[ index ];
    
    public void FlipLight( int index )
    {
        FlipMap( index );
        foreach( var otherI in _lightFlipMap[ index ] )
            _pieces[ otherI ].Toggle();

        if( !Solved() ) return;
        foreach( var p in _pieces )
            p.Solve();

    }

    public void LockAll()
    {
        foreach( var p in _pieces )
        {
            p.Lock();
        }
    }
}