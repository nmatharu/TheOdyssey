using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicCamDemo : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] float cameraFollowSpeed = 10f;
    float cameraOffset;
    float _targetX;
    
    // Start is called before the first frame update
    void Start()
    {
        cameraOffset = transform.position.x;
        _targetX = cameraOffset;
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePlayerMedianX();
        LerpCameraToTarget();
    }

    void LerpCameraToTarget()
    {
        var pos = transform.position;
        transform.position = Vector3.Lerp( pos, 
            new Vector3( cameraOffset + _targetX, pos.y, pos.z ),
            cameraFollowSpeed * Time.deltaTime );
    }

    void CalculatePlayerMedianX()
    {
        var transforms = playersParent.Cast<Transform>()
            .Where( p => p.gameObject.activeInHierarchy ).ToArray();
        var minX = transforms.Min( p => p.transform.position.x );
        var maxX = transforms.Max( p => p.transform.position.x );
        _targetX = ( minX + maxX ) / 2;
    }
}
