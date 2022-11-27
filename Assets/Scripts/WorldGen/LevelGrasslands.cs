using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGrasslands : Level
{
    [ SerializeField ] Vector2[] waterfallSpacingPerBiome = { new( 15, 30 ), new( 5, 12 ), new( 15, 30 ) };
    [ SerializeField ] Vector2[] waterfallWidthPerBiome = { new( 2, 7 ), new( 2, 4 ), new( 4, 9 ) };
    [ SerializeField ] int[] treesPerBiome = { 25, 100, 15 };
    [ SerializeField ] int[] rocksPerBiome = { 5, 0, 15 };

    List<Vector2Int> _treePoints = new();

    public override void Generate( WorldGenerator generator )
    {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );

        generator.GenerateShopsAndMagic( DefaultShopPlacements, DefaultMagicPlacement );

        // generator.GrasslandsPub();
        Waterfalls( generator, BiomeAStart, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        // Biome A - Med waterfalls, med trees
        GenerateBiome( generator, 0, BiomeAStart, (int) ( BiomeAStart + CoreLength / 3f ) );

        // Biome B - Few waterfalls, dense trees
        GenerateBiome( generator, 1, BiomeBStart, (int) ( BiomeBStart + CoreLength / 3f ) );

        // Biome C - Lots waterfalls, few trees
        GenerateBiome( generator, 2, BiomeCStart, (int) ( BiomeCStart + CoreLength / 3f ) );
        
        generator.GenerateBossZone( DefaultWorldSizeX - BossZoneOffset );
        TransitionBlocks( generator );
        
        GenerateChests( generator, _treePoints, ChestsPerPlayerPerStage * generator.NumPlayers() );

        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );

        // generator.ShowOffLimits();
    }

    void Waterfalls( WorldGenerator generator, int xStart, int xEnd )
    {
        GenerateWaterfall( generator, 0, 10 );
        for( var x = xStart; x < xEnd; x += RandomV2Range( waterfallSpacingPerBiome[ 0 ] ) )
        {
            var w = RandomV2Range( waterfallWidthPerBiome[ 0 ] );
            if( x + w < xEnd - 2 && generator.RectIsClear( x, w ) )
                GenerateWaterfall( generator, x, w );
        }
    }

    void GenerateBiome( WorldGenerator generator, int biome, int biomeStart, int biomeEnd )
    {

        var points = generator.GetSpacedOutPoints( biomeStart, 1, 
            biomeEnd - biomeStart - 2, DefaultWorldSizeY - 2, 4, 4 );
        points.Shuffle();

        for( var i = 0; i < points.Count && i < 2; i++ )
        {
            Instantiate( generator.Gen( WorldGenIndex.Misc.Campfire ), 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            generator.Map( points[ i ] ).OffLimits = true;
        }

        for( var i = 0; i < points.Count && i < treesPerBiome[ biome ]; i++ )
        {
            if( generator.Map( points[ i ] ).OffLimits )
                continue;

            Instantiate( RandomTree( generator ), 
                WorldGenerator.CoordsToWorldPos( points[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            _treePoints.Add( points[ i ] );
            generator.Map( points[ i ] ).OffLimits = true;
            generator.Map( points[ i ] ).HasSurfaceObject = true;
        }

        var rockPoints = new List<Vector2Int>();

        foreach( var p in points )
        {
            for( var x = p.x - 1; x <= p.x + 1; x++ )
            {
                for( var y = p.y - 1; y <= p.y + 1; y++ )
                {
                    if( !generator.Map( x, y ).OffLimits && y is > 0 and < DefaultWorldSizeY - 1 )
                        rockPoints.Add( new Vector2Int( x, y ) );
                }
            }
        }
        
        rockPoints.Shuffle();
        for( var i = 0; i < rocksPerBiome[ biome ]; i++ )
        {
            Instantiate( generator.Gen( WorldGenIndex.Objs.Rock ), 
                WorldGenerator.CoordsToWorldPos( rockPoints[ i ] ), 
                JBB.RandomYRot(), generator.objsParent );

            generator.Map( rockPoints[ i ] ).OffLimits = true;
        }
    }

    GameObject RandomTree( WorldGenerator g )
    {
        var v = Random.value;
        return v switch
        {
            < 0.50f => g.Gen( WorldGenIndex.Objs.TreeLarge ),
            < 0.86f => g.Gen( WorldGenIndex.Objs.TreeMed ),
            _ => g.Gen( WorldGenIndex.Objs.TreeSmall )
        };
    }

    int RandomV2Range( Vector2 v2 ) => (int) Random.Range( v2.x, v2.y );

    void GenerateWaterfall( WorldGenerator gen, int xStart, int width )
    {
        var logY = DefaultWorldSizeY / 2 + Random.Range( -3, 3 );
        for( var x = xStart; x < xStart + width; x++ )
        {
            // var breakFromLoop = false;
            // for( var y = 0; y < DefaultWorldSizeY; y++ )
            //     if( gen.Map()[ x, y ].OffLimits )    breakFromLoop = true;
            // if( breakFromLoop )
            //     break;

            for( var y = 0; y < DefaultWorldSizeY; y++ )
            {
                Destroy( gen.Map( x, y ).Ground );
                gen.Map( x, y ).Ground = Instantiate( gen.Gen( WorldGenIndex.Blocks.Empty ),
                    WorldGenerator.CoordsToWorldPos( x, y ), Quaternion.identity, gen.blocksParent );
                gen.Map( x, y ).GenIndex = (int) WorldGenIndex.Blocks.Empty;
                
                gen.MakeOffLimits( x-1, y );
                gen.MakeOffLimits( x, y );
                gen.MakeOffLimits( x+1, y );
                
                gen.Map( x, y ).EmptyCollider = true;
            }

            Destroy( gen.Map( x, logY ).Ground );
            gen.Map( x, logY ).Ground = Instantiate( gen.Gen( WorldGenIndex.Blocks.Log ),
                WorldGenerator.CoordsToWorldPos( x, logY ), Quaternion.identity, gen.blocksParent );
            gen.Map( x, logY ).GenIndex = (int) WorldGenIndex.Blocks.Log;
            gen.Map( x, logY ).EmptyCollider = false;
        }

        var w = Instantiate( gen.Gen( WorldGenIndex.Misc.WaterfallPfx ),
            new Vector3( 2 * ( xStart + width / 2f ) - 1, 0, DefaultWorldSizeY * 2 ),
            Quaternion.identity, gen.miscParent );

        w.GetComponent<WaterfallPfx>().SetWidth( width );
    }
}