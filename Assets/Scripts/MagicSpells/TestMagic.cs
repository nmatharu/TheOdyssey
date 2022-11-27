using UnityEngine;

[ CreateAssetMenu( fileName = "TestMagic", menuName = "Scriptable Objects/TestMagic" ) ]
public class TestMagic : MagicSpell
{
    public override void Cast( Player player )
    {
        GameManager.Instance.SpawnGenericFloating( player.transform.position, "UNIMPLEMENTED! :D", Color.gray, 12f );
    }
}