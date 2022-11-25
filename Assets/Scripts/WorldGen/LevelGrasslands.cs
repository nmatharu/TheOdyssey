public class LevelGrasslands : Level
{
    public override void Generate( WorldGenerator generator )
    {
        environmentalParent.gameObject.SetActive( true );
        generator.GenerateBase( baseBlock, DefaultWorldSizeX, DefaultWorldSizeY );
        
        generator.GenerateShopsAndMagic( DefaultShopPlacements, DefaultMagicPlacement );

        // generator.GrasslandsPub();

        generator.GenerateBossZone( DefaultWorldSizeX - BossZoneOffset );
        generator.DuplicateTopRow( DefaultWorldSizeX, DefaultWorldSizeY );

        generator.ShowOffLimits();
    }
}