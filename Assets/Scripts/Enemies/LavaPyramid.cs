using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class LavaPyramid : MonoBehaviour
{
    [ SerializeField ] Transform raycastPoint;
    [ SerializeField ] float raycastDistance;
    [ SerializeField ] float damage;
    [ SerializeField ] ParticleSystem raycastPfx;
    
    [ SerializeField ] Transform topTransform;
    [ SerializeField ] Transform recoilTransform;
    [ SerializeField ] Vector3 recoil;
    [ SerializeField ] float recoilReturnSpeed;

    [ SerializeField ] float fireRate = 4f;
    [ SerializeField ] float lockOnSpeed = 10f;
    [ SerializeField ] Renderer topRenderer;
    [ SerializeField ] float glowIntensity = 12f;
    [ SerializeField ] float recoilSpeed = 15f;
    [ SerializeField ] float recoilLength = 0.15f;
    [ SerializeField ] float chargeTime = 2f;
    [ SerializeField ] ParticleSystem chargeUpPfx;
    [ SerializeField ] ParticleSystem firePfx;
    [ SerializeField ] ImageFader indicator;

    Transform _targetPlayer;
    Material _mat;
    Rigidbody _body;
    Enemy _enemy;
    Color _emissiveColor;
    bool _recoiling;

    AudioSource _chargeSfx;

    static readonly int EmissiveColor = Shader.PropertyToID( "_EmissionColor" );

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _enemy = GetComponent<Enemy>();
        _chargeSfx = GetComponent<AudioSource>();
        _chargeSfx.clip = AudioManager.Instance.pyramidCharges.RandomEntry();
        
        _mat = topRenderer.material;
        _emissiveColor = _mat.GetColor( EmissiveColor );

        InvokeRepeating( nameof( FindNewTarget ), 0f, Random.Range( 0.5f, 1f ) );

        InvokeRepeating( nameof( Fire ), 1f + Random.value, 60f / fireRate );
        // InvokeRepeating( nameof( FireFireball ), Random.Range( 1f, 2f ), fireballFireRate );
    }

    void FixedUpdate() => _body.velocity = Vector3.zero;

    void Update()
    {
        var pos = transform.position;
        if( _targetPlayer != null )
        {
            var look = _targetPlayer.position - pos;
            var lookRot = look != Vector3.zero ? Quaternion.LookRotation( look ) : Quaternion.identity;
            var lookTarget = Quaternion.Slerp( topTransform.rotation, lookRot, lockOnSpeed * Time.deltaTime );
            topTransform.rotation = lookTarget;
        }

        if( _recoiling ) return;
        recoilTransform.localEulerAngles = Vector3.Lerp( recoilTransform.localEulerAngles,
            new Vector3( 0, 180, 0 ), recoilReturnSpeed * Time.deltaTime );
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawRay( raycastPoint.position, topTransform.forward * raycastDistance );
    // }

    void Fire() => StartCoroutine( FireCoroutine() );

    IEnumerator FireCoroutine()
    {
        _mat.EnableKeyword( "_EMISSION" );

        _chargeSfx.Play();
        chargeUpPfx.Play();
        indicator.FadeIn();

        for( var elapsed = 0f; elapsed < chargeTime; elapsed += Time.deltaTime )
        {
            // _mat.color = new Color( 1f, 1f, 1f ) * 8f;
            _mat.SetColor( EmissiveColor, _emissiveColor * ( 1f + elapsed / chargeTime * glowIntensity ) );
            yield return null;
        }

        _mat.SetColor( EmissiveColor, _emissiveColor );

        _recoiling = true;
        firePfx.Play();
        raycastPfx.Play();

        // Fire Raycast
        var hits = Physics.RaycastAll( raycastPoint.position, topTransform.forward, raycastDistance );
        foreach( var h in hits )
        {
            var p = h.collider.gameObject.GetComponent<Player>();
            if( p != null )
                p.IncomingDamage( damage, _enemy.Level() );
        }

        AudioManager.Instance.pyramidBlast.RandomEntry().PlaySfx( 1f, 0.2f );
        indicator.FadeOut();

        for( var elapsed = 0f; elapsed < recoilLength; elapsed += Time.deltaTime )
        {
            recoilTransform.localEulerAngles =
                Vector3.Lerp( recoilTransform.localEulerAngles, recoil, recoilSpeed * Time.deltaTime );
            _mat.SetColor( EmissiveColor,
                _emissiveColor * ( 1f + ( recoilLength - elapsed ) / recoilLength * glowIntensity ) );
            yield return null;
        }

        _mat.SetColor( EmissiveColor, _emissiveColor );
        _recoiling = false;
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var players = GameManager.Instance.Players();
        if( players.Empty() ) return;
        var closestPlayer = players[ 0 ].transform;

        foreach( var p in players )
            if( JBB.DistXZSquared( pos, p.transform.position ) < JBB.DistXZSquared( pos, closestPlayer.position ) )
                closestPlayer = p.transform;

        _targetPlayer = closestPlayer;
    }
}