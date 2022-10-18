using System;
using UnityEditor;
using UnityEngine;

public class LevelGrasslands : MonoBehaviour, Level
{
    [ SerializeField ] int test;
    public void Generate( Tile[ , ] tileMap ) {
    }

    public Vector2Int WorldSize() => new Vector2Int( Level.DefaultWorldSizeX, Level.DefaultWorldSizeY );
}