using UnityEngine;

public class TestMagic : MagicSpell
{
    public override void Cast( Player player )
    {
        cooldownSeconds = 20f;
        GameManager.Instance.SpawnGenericFloating( player.transform.position, "UNIMPLEMENTED! :D", Color.gray, 12f );
        // player.IncomingDamage( 5, 5 );
    }
}
