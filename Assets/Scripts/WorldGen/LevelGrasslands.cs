using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGrasslands : Level
{
    public override void Generate( WorldGenerator generator ) {
        generator.GenerateBase( WorldGenIndex.Blocks.Grass, DefaultWorldSizeX, DefaultWorldSizeY );
        generator.GrasslandsPub();
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
    }
}