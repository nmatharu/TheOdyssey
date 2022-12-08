using UnityEngine;

public class AnimSfx : MonoBehaviour
{
    void PlaySlashSfx() => AudioManager.Instance.nightmareSlash.RandomEntry().PlaySfx( 1f, 0.1f );

    void GolemFootstep() => AudioManager.Instance.golemFootsteps.RandomEntry().PlaySfx( 0.75f, 0.2f );
}