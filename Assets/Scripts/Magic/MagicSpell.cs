using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MagicSpell : MonoBehaviour
{
    [ SerializeField ] public string magicName;
    [ SerializeField ] public string magicDescription;
    [ SerializeField ] protected float cooldownSeconds;

    public abstract void Cast( Player player );
    public float Cooldown() => cooldownSeconds;
}
