using UnityEngine;

public class PlayerMagic : MonoBehaviour
{
    // The logic for magic is mostly contained within the ScriptableObjects themselves,
    // with the exception of things like particle fx that are children of the player
    [ SerializeField ] ParticleSystem aoeHealPfx;
    Player _player;

    void Start() => _player = GetComponent<Player>();

    public void PlayAoeHealPfx() => aoeHealPfx.Play();
}