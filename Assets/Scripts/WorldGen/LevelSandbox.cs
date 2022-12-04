using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSandbox : Level
{
    [ SerializeField ] Transform screenRight;
    
    public override void Generate( WorldGenerator generator )
    {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );

        GenerateWaterfall( generator, 0, 10 );
        
        for( var i = 0; i < 30; i++ )
        {
            var offsetX = FindObjectsOfType<SandboxInteractable>().Length + 1;
            Instantiate( generator.Gen( WorldGenIndex.Objs.MagicShrine ), 
                WorldGenerator.CoordsToWorldPos( 2 * i + 15 + offsetX, 1 ),
                Quaternion.identity, generator.objsParent ).GetComponent<MagicShrine>().Init( i );
        }
        
        screenRight.position += Vector3.right * 2 * ( RuneIndex.Instance.runeIndex.Length - 19 );

        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );

        this.Invoke( () =>
        {
            var ps = FindObjectsOfType<Player>();
            foreach( var p in ps )
                p.InitSandbox();
        }, 1f );
    }
    
    
    
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