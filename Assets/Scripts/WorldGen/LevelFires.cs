using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelFires : Level
{
    public override void Generate( WorldGenerator generator ) {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        // generator.GrasslandsPub();
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
    }
}