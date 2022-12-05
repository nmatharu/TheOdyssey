using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelSands : Level
{
    [ SerializeField ] int[] numRocksPerBiome = { 10, 20, 20 };
    
    [ SerializeField ] int biomeANumCacti = 15;
    [ SerializeField ] int biomeBPalmTrees = 16;
    [ SerializeField ] int biomeCNumDeadTrees = 20;
    
    List<Vector2Int> _chestAdjPoints = new();
    
    public override void Generate( WorldGenerator generator ) {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        generator.GenerateShopsAndMagic( DefaultShopPlacements, DefaultMagicPlacement );
        generator.GenerateBossZone( DefaultWorldSizeX - BossZoneOffset );
        
        GenerateBiome( generator, 0, BiomeAStart - 15, (int) ( BiomeAStart + CoreLength / 3f ) );
        GenerateBiome( generator, 1, BiomeBStart, (int) ( BiomeBStart + CoreLength / 3f ) );
        GenerateBiome( generator, 2, BiomeCStart, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        GenerateChests( generator, _chestAdjPoints, ChestsPerPlayerPerStage * generator.NumPlayers() );
        
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
    }

    void GenerateBiome( WorldGenerator generator, int biome, int biomeStart, int biomeEnd )
    {
        var points = generator.GetSpacedOutPoints( biomeStart, 1, 
            biomeEnd - biomeStart - 2, DefaultWorldSizeY - 2, 3, 3 );
        points.Shuffle();
        
        // for( var i = 0; i < points.Count && i < 2; i++ )
        // {
        //     Instantiate( generator.Gen( WorldGenIndex.Misc.Campfire ), 
        //         WorldGenerator.CoordsToWorldPos( points[ i ] ), 
        //         JBB.RandomYRot(), generator.objsParent );
        //
        //     generator.Map( points[ i ] ).OffLimits = true;
        // }

        var treeCounts = new[] { biomeANumCacti, biomeBPalmTrees, biomeCNumDeadTrees };

        for( var i = 0; i < points.Count && i < treeCounts[ biome ]; i++ )
        {
            if( generator.Map( points[ i ] ).OffLimits )
                continue;

            var o = biome switch
            {
                0 => generator.Gen( RandomCactus() ),
                1 => generator.Gen( RandomCactus() ),
                2 => generator.Gen( WorldGenIndex.Objs.DeadTree ),
                _ => null
            };

            Instantiate( o, 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            _chestAdjPoints.Add( points[ i ] );
            generator.Map( points[ i ] ).OffLimits = true;
            generator.Map( points[ i ] ).HasSurfaceObject = true;
        }
        
        points = generator.GetSpacedOutPoints( biomeStart, 1, 
            biomeEnd - biomeStart - 2, DefaultWorldSizeY - 2, 3, 3 );
        points.Shuffle();
        
        for( var i = 0; i < points.Count && i < numRocksPerBiome[ biome ]; i++ )
        {
            Instantiate( generator.Gen( RandomRock() ), 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            generator.Map( points[ i ] ).OffLimits = true;
        }
    }
    
    WorldGenIndex.Objs RandomRock() => new[]
    {
        WorldGenIndex.Objs.SandRockA,
        WorldGenIndex.Objs.SandRockB,
        WorldGenIndex.Objs.SandRockC
    }.RandomEntry();
    
    WorldGenIndex.Objs RandomCactus() => new[]
    {
        WorldGenIndex.Objs.CactusA,
        WorldGenIndex.Objs.CactusB
    }.RandomEntry();
}