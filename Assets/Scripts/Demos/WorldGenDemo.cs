using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenDemo : MonoBehaviour
{
    [ SerializeField ] GameObject[] tiles;
    [ SerializeField ] Transform tilesParent;
    
    [ SerializeField ] int worldSizeX;
    [ SerializeField ] int worldSizeY;
    [ SerializeField ] int worldGenStartX = -10;
    [ SerializeField ] int worldGenStartY = 0;
    int[ , ] _tileIndices;
    float perlinSeed;
    
    void Start() => StartCoroutine( GenerateWorld() );

    IEnumerator GenerateWorld()
    {
        perlinSeed = Random.value;
        _tileIndices = new int[ worldSizeX, worldSizeY ];
        for( var x = worldGenStartX; x < worldSizeX + worldGenStartX; x++ )
        {
            for( var y = worldGenStartY; y < worldSizeY + worldGenStartY; y++ )
            {
                var i = GetTileIndexAt( x, y );
                _tileIndices[ x, y ] = i;
                Instantiate( tiles[ i ], new Vector3( x * 2, 0, y * 2 ), 
                    Quaternion.identity, tilesParent );
            }
        }
        yield break;
    }

    int GetTileIndexAt( int x, int y )
    {
        var val = GetNoiseValueAt( x, y );
        return val < 0.4f ? 1 : 0; 
    }

    float GetNoiseValueAt( int x, int y ) => Mathf.PerlinNoise( (float) x / worldSizeX + perlinSeed, (float) y / worldSizeY + perlinSeed );
}
