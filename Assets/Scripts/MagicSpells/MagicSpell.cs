using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MagicSpell : ScriptableObject
{
    public string magicName;
    [ TextArea( 1, 2 ) ]
    public string magicDescription;
    public Sprite magicIcon;
    public float cooldownSeconds;
    public abstract bool Cast( Player player );
}
