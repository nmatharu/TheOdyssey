using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGrasslands : Level
{
    public override void Generate( WorldGenerator generator ) {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        generator.GrasslandsPub();
        // generator.GenerateBossZone( DefaultWorldSizeX - 20 );
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
    }
}