using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandWorm : MonoBehaviour
{
    [ SerializeField ] List<SandWormNode> nodes;
    [ SerializeField ] Transform toFollow;
    [ SerializeField ] float dmg = 10f;
    [ SerializeField ] public float speed = 5f;
    [ SerializeField ] public float headLookSlerp = 12f;
    [ SerializeField ] public float lookSlerp = 10f;
    [ SerializeField ] float sinSpeed = 3f;
    [ SerializeField ] float sinStrength = 15f;
    [ SerializeField ] Transform wormParent;
    [ SerializeField ] float distanceBetweenNodes = 2f;
    
    [ SerializeField ] int numSegmentsAfterHead = 8;
    [ SerializeField ] GameObject nodePrefab;
    [ SerializeField ] GameObject dangerIndicator;
    [ SerializeField ] GameObject burrowPfx;
    
    public SandWormNode head;
    Enemy _enemy;


    GameObject _headIndicator; 
    CanvasGroup _indicatorImage;
    
    Transform _move;
    Transform _targetPlayer;
    
    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();

        var pos = transform.position;
        _headIndicator = Instantiate( dangerIndicator, new Vector3( pos.x, 0.01f, pos.z ), Quaternion.identity );
        _indicatorImage = _headIndicator.GetComponentInChildren<CanvasGroup>();

        // transform.position += Vector3.down;
        var initRot = Quaternion.Euler( -90, 0, 0 );
        transform.rotation = initRot;
        for( var i = 0; i < numSegmentsAfterHead; i++ )
        {
            var o = Instantiate( nodePrefab, transform.position - new Vector3( 0, distanceBetweenNodes, 0 ) * ( i + 1 ),
                initRot, EnemySpawner.Instance.EnemiesParent() );
            nodes.Add( o.GetComponent<SandWormNode>() );
        }

        head = nodes[ 0 ];
        
        head.Init( _enemy, this, toFollow );

        for( var i = 1; i < nodes.Count; i++ )
        {
            nodes[ i ].Init( _enemy, this, nodes[ i - 1 ].backPoint );
            nodes[ i ].InitAutoRot( i, nodes.Count );
            
            _enemy.AddRenderer( nodes[ i ].meshRenderer );
        }

        InvokeRepeating( nameof( FindNewTarget ), 0, 1f );
        
        StartCoroutine( Movement() );
    }

    void Update()
    {
        var pos = transform.position;
        _headIndicator.transform.position = new Vector3( pos.x, 0.01f, pos.z );
        _indicatorImage.alpha = JBB.ClampedMap01( Mathf.Abs( pos.y ), 10, 4 );
    }

    void OnCollisionEnter( Collision c )
    {
        var p = c.collider.GetComponent<Player>();

        if( p == null ) return;
        p.IncomingDamage( dmg, _enemy.Level() );
    }

    IEnumerator Movement()
    {
        var elapsed = 0f;
        var wait = new WaitForSeconds( 1f );
        var targetX = 0f;
        var targetY = 10f;
        var targetZ = 0f;
        var aboveGround = false;
        var goingDown = false;
        for( ;; )
        {
            if( _targetPlayer == null )
                yield return null;

            var y = sinStrength * Mathf.Sin( elapsed * sinSpeed );
            var dir = Mathf.Cos( elapsed * sinSpeed );
            
            // Surfacing frame
            if( !aboveGround && y > 0 )
            {
                var n = Vector2.MoveTowards(
                    new Vector2( transform.position.x, transform.position.z ),
                    new Vector2( _targetPlayer.position.x, _targetPlayer.position.z ), speed / sinSpeed );
                targetX = n.x;
                targetZ = n.y;
                aboveGround = true;
                
                DrawBurrowPfx();
            }

            if( aboveGround && !goingDown && dir < 0 )
            {
                targetY = -sinStrength;
                goingDown = true;
                // DrawIndicator( targetX, targetZ );
            }

            // Burrowing frame
            if( aboveGround && y < 0 )
            {
                var n = Vector2.MoveTowards(
                    new Vector2( transform.position.x, transform.position.z ),
                    new Vector2( _targetPlayer.position.x, _targetPlayer.position.z ), speed / sinSpeed );
                aboveGround = false;
                targetX = n.x;
                targetZ = n.y;

                DrawBurrowPfx();
            }

            if( !aboveGround && goingDown && dir > 0 )
            {
                targetY = sinStrength;
                goingDown = false;
                // DrawIndicator( targetX, targetZ );
            }

            head.SetV3Target( new Vector3( targetX, targetY, targetZ ) );
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // void DrawIndicator( float x, float z )
    // {
    //     var o = Instantiate( dangerIndicator, new Vector3( x, 0.01f, z ), Quaternion.identity );
    // }

    void DrawBurrowPfx()
    {
        Instantiate( burrowPfx, new Vector3( transform.position.x, 0, 
            transform.position.z ), Quaternion.identity );
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var players = GameManager.Instance.Players();
        if( players.Empty() )   return;
        var furthestPlayer = players[ 0 ].transform;
        
        foreach( var p in players )
            if( JBB.DistXZSquared( pos, p.transform.position ) > JBB.DistXZSquared( pos, furthestPlayer.position ) )
                furthestPlayer = p.transform;

        _targetPlayer = furthestPlayer;
    }

    void OnDestroy()
    {
        if( _headIndicator != null )
        {
            var aif = _headIndicator.GetComponentInChildren<AutoImageFader>();
            if( aif != null )   aif.FadeOut();
        }
        for( var i = 1; i < nodes.Count; i++ )
            Destroy( nodes[ i ].gameObject );
    }
}
