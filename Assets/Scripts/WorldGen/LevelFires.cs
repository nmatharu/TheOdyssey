using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelFires : Level
{
    List<Vector2Int> _chestAdjPoints = new();

    public override void Generate( WorldGenerator generator ) {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        generator.GenerateShopsAndMagic( DefaultShopPlacements, DefaultMagicPlacement );
        generator.GenerateBossZone( DefaultWorldSizeX - BossZoneOffset );
        
        
        // generator.GrasslandsPub();
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
        
        GenerateBiome( generator, BiomeAStart, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        GenerateChests( generator, _chestAdjPoints, ChestsPerPlayerPerStage * generator.NumPlayers() );
        
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
    }

    void GenerateBiome( WorldGenerator generator, int startX, int endX )
    {
        var points = generator.GetSpacedOutPoints( startX, 1, 
            endX - startX - 2, DefaultWorldSizeY - 2, 3, 3 );
        points.Shuffle();
        
        for( var i = 0; i < points.Count && i < 6; i++ )
        {
            Instantiate( generator.Gen( WorldGenIndex.Misc.Campfire ), 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            generator.Map( points[ i ] ).OffLimits = true;
        }
        
        for( var i = 0; i < points.Count && i < 60; i++ )
        {
            if( generator.Map( points[ i ] ).OffLimits )
                continue;

            var o = new[]
                    { generator.Gen( WorldGenIndex.Objs.LavaRock ), 
                        generator.Gen( WorldGenIndex.Objs.LavaRock ), 
                        generator.Gen( WorldGenIndex.Objs.LavaRock ), 
                        generator.Gen( WorldGenIndex.Objs.LavaTree ) }
                .RandomEntry();

            Instantiate( o, 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            _chestAdjPoints.Add( points[ i ] );
            generator.Map( points[ i ] ).OffLimits = true;
            generator.Map( points[ i ] ).HasSurfaceObject = true;
        }
    }
}