using UnityEngine;

public class AnimSfx : MonoBehaviour
{
    void PlaySlashSfx() => AudioManager.Instance.nightmareSlash.RandomEntry().PlaySfx( 1f, 0.1f );
}