using UnityEngine;

public class TestMagic : MagicSpell
{
    public override void Cast( Player player )
    {
        cooldownSeconds = 20f;
        player.IncomingDamage( 5, 5 );
    }
}
