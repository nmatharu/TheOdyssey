using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelFires : Level
{
    List<Vector2Int> _chestAdjPoints = new();

    [ SerializeField ] int lavaFallMinSpacing = 6;
    [ SerializeField ] int lavaFallMaxSpacing = 20;
    [ SerializeField ] Vector2 lavaFallBotSpacing = new( 12, 36 );
    
    public override void Generate( WorldGenerator generator ) {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        generator.GenerateShopsAndMagic( DefaultShopPlacements, DefaultMagicPlacement );
        generator.GenerateBossZone( DefaultWorldSizeX - BossZoneOffset );
        
        LavaFalls( generator, BiomeAStart - 15, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        // generator.GrasslandsPub();
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
        
        GenerateBiome( generator, BiomeAStart - 15, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        GenerateChests( generator, _chestAdjPoints, ChestsPerPlayerPerStage * generator.NumPlayers() );
        
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );
        
        // generator.ShowOffLimits();
    }

    void GenerateBiome( WorldGenerator generator, int startX, int endX )
    {
        var points = generator.GetSpacedOutPoints( startX, 1, 
            endX - startX - 2, DefaultWorldSizeY - 2, 3, 3 );
        points.Shuffle();

        // GenerateLavaFallTop( generator, 20 );
        
        for( var i = 0; i < points.Count && i < 40; i++ )
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
    
    int RandomV2Range( Vector2 v2 ) => (int) Random.Range( v2.x, v2.y );
    
    void LavaFalls( WorldGenerator generator, int xStart, int xEnd )
    {
        for( var x = xStart + 2; x < xEnd; x += Random.Range( lavaFallMinSpacing, lavaFallMaxSpacing + 1 ) )
        {
            const int w = 5;
            if( x + w < xEnd - 2 && generator.RectIsClear( x - 2, w ) )
                GenerateLavaFallTop( generator, x );
        }
        
        for( var x = xStart + 2; x < xEnd; x += RandomV2Range( lavaFallBotSpacing ) )
        {
            Instantiate( generator.Gen( WorldGenIndex.Misc.LavaFallBot ),
                WorldGenerator.CoordsToWorldPos( x, 0 ), Quaternion.identity, generator.miscParent );
        }
    }
    
    void GenerateLavaFallTop( WorldGenerator gen, int xMid )
    {
        // var logY = DefaultWorldSizeY / 2 + Random.Range( -3, 3 );
        for( var x = xMid - 2; x <= xMid + 2; x++ )
        {
            // var breakFromLoop = false;
            // for( var y = 0; y < DefaultWorldSizeY; y++ )
            //     if( gen.Map()[ x, y ].OffLimits )    breakFromLoop = true;
            // if( breakFromLoop )
            //     break;

            for( var y = DefaultWorldSizeY - 1; y < DefaultWorldSizeY; y++ )
            {
                Destroy( gen.Map( x, y ).Ground );
                gen.Map( x, y ).Ground = Instantiate( gen.Gen( WorldGenIndex.Blocks.LavaPit ),
                    WorldGenerator.CoordsToWorldPos( x, y ), Quaternion.identity, gen.blocksParent );
                gen.Map( x, y ).GenIndex = (int) WorldGenIndex.Blocks.Empty;
                
                gen.MakeOffLimits( x-1, y );
                gen.MakeOffLimits( x, y );
                gen.MakeOffLimits( x+1, y );
                
                gen.Map( x, y ).EmptyCollider = true;
            }

            // Destroy( gen.Map( x, logY ).Ground );
            // gen.Map( x, logY ).Ground = Instantiate( gen.Gen( WorldGenIndex.Blocks.Log ),
            //     WorldGenerator.CoordsToWorldPos( x, logY ), Quaternion.identity, gen.blocksParent );
            // gen.Map( x, logY ).GenIndex = (int) WorldGenIndex.Blocks.Log;
            // gen.Map( x, logY ).EmptyCollider = false;
        }

        var w = Instantiate( gen.Gen( WorldGenIndex.Misc.LavaFallTop ),
            WorldGenerator.CoordsToWorldPos( xMid, 0 ),
            Quaternion.identity, gen.miscParent );

        // w.GetComponent<WaterfallPfx>().SetWidth( width );
    }

    public override float EndLevelX() => Mathf.Infinity;
}