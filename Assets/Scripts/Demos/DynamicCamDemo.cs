using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCamDemo : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] Camera[] cameras;
    [ SerializeField ] int maxNumPlayers = 1;
    [ SerializeField ] float[] cameraFollowSpeeds;
    [ SerializeField ] float cameraOffset;
    [ SerializeField ] float projectionFactor = 1.3f;
    [ SerializeField ] float splitViewPortOffsetY = 1f;

    [ SerializeField ] float cameraSplitDistance = 10f;

    [ SerializeField ] Image[] splitScreenLines;

    [ SerializeField ] float theHopeOffset = 0f;

    [ SerializeField ] float singleCam3pSmoothingMinLerp = 10f;
    [ SerializeField ] float singleCam3pSmoothingMaxLerp = 1000f;

    [ SerializeField ] CanvasScaler scaler;

    float _initY;

    float[] _sortedPlayerXPoses;

    float _playerMinX;
    float _playerMaxX;
    float _cameraFollowSpeed;

    // PlayersIndices... 0 - 2
    const int MidpointIndex = 3;
    const int TwoThirdsLeftIndex = 4;
    const int TwoThirdsRightIndex = 5;
    float[] _lerpXs, _targetXs;

    float _singleCam3pLerp, _singleCam3pTarget, _singleCam3pSmoothing;
    
    Vector3 cameraUp;

    float splitScreenLineHeight;

    // TODO-- camera follow speed increases with number of splits

    // Start is called before the first frame update
    void Start()
    {
        SortPlayerXPoses();

        _lerpXs = new float[ 6 ];
        _targetXs = new float[ 6 ];
        CalcTargetValues();
        CopyTargetToLerp();

        _cameraFollowSpeed = cameraFollowSpeeds[ 0 ];
        splitScreenLineHeight = scaler.referenceResolution.y;

        var fwd = Camera.main.transform.forward;
        _initY = cameras[ 0 ].transform.position.y;
        // Debug.Log( _initY );
        cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    // Update is called once per frame
    void Update()
    {
        SortPlayerXPoses();
        CalcTargetValues();
        LerpCurrentValues();

        // Is having separate methods for each of these a great idea? Not really, but the logic for each of various
        // number of cameras is different, and combining them all into a single, flexible method escapes me at this 
        // time and it seems better to focus my limited development time on implementing all the core features --
        // I will perhaps revisit this if I have time near the end of the project
        switch( maxNumPlayers )
        {
            case 1:
                SingleCam();
                break;
            case 2:
                DualCam();
                break;
            case 3:
                // TriCam();
                TriCam2();
                break;
        }

        // NavHelpers.LogArray( _targetXs );
        // CalculatePlayerMedianX();
        // LerpCameraToTarget();
    }

    void SingleCam()
    {
        EnableCameras( 0 );
        cameras[ 0 ].rect = new Rect( 0, 0, 1f, 1f );

        SetCameraPosition( cameras[ 0 ], _lerpXs[ MidpointIndex ], 0.5f );

        foreach( var l in splitScreenLines )
            l.rectTransform.sizeDelta = Vector2.zero;
        
        // TODO add smoothing to single cam
    }

    void DualCam()
    {
        if( _playerMaxX - _playerMinX < cameraSplitDistance )
        {
            SingleCam();
            return;
        }

        EnableCameras( 0, 1 );
        for( var i = 0; i < maxNumPlayers; i++ )
        {
            var w = 1f / maxNumPlayers;
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _lerpXs[ i ], w / 2f + w * i );
        }

        // TODO add smoothing to dual cam
        
        splitScreenLines[ 1 ].rectTransform.sizeDelta =
            new Vector2( _playerMaxX - _playerMinX - cameraSplitDistance, splitScreenLineHeight );
    }

    void TriCam()
    {
        // TODO Want smoothing but not jittering? Try basing smoothing off of the difference of the median lerp to its target val
        // aka median lerp is at 0.1, target of 0.08, use that -0.02 offset for other camera positions and follow the median's difference
        
        var acDist = _playerMaxX - _playerMinX;
        var abDist = _sortedPlayerXPoses[ 1 ] - _playerMinX;
        var bcDist = _playerMaxX - _sortedPlayerXPoses[ 1 ];
        var acThreshold = cameraSplitDistance * 4f / 3f;
        var adjThreshold = cameraSplitDistance * 2f / 3f; // TODO extract these to constants
        var w = 1f / maxNumPlayers;

        splitScreenLines[ 0 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 2 ].rectTransform.sizeDelta = Vector2.zero;

        Debug.Log( abDist + ", " + adjThreshold + ", " + cameraSplitDistance );

        if( acDist < acThreshold && abDist < adjThreshold && bcDist < adjThreshold )
        {
            EnableCameras( 0 );
            cameras[ 0 ].rect = new Rect( 0, 0, 1f, 1f );

            SetCameraPosition( cameras[ 0 ], _lerpXs[ MidpointIndex ], 0.5f );

            foreach( var l in splitScreenLines )
                l.rectTransform.sizeDelta = Vector2.zero;

            return;
        }

        EnableCameras( 0, 1 );

        // One player is far off to the left
        if( abDist < adjThreshold && bcDist > adjThreshold )
        {
            var hopeOffset = GGJ.Map(
                _sortedPlayerXPoses[ 1 ] - _sortedPlayerXPoses[ 0 ], 0, adjThreshold, theHopeOffset, 0 );

            hopeOffset *=
                GGJ.ClampedMap01( _playerMaxX - ( _sortedPlayerXPoses[ 1 ] + _playerMinX ) / 2f, 
                    acThreshold + adjThreshold, adjThreshold * 1.5f );
            
            cameras[ 0 ].rect = new Rect( 0, 0, 2 * w, 1f );
            cameras[ 1 ].rect = new Rect( 2 * w, 0, w, 1f );
            SetCameraPosition( cameras[ 0 ], _lerpXs[ TwoThirdsLeftIndex ], 1 / 3f );
            SetCameraPosition( cameras[ 1 ], _lerpXs[ 2 ] + hopeOffset, 5 / 6f );

            splitScreenLines[ 0 ].rectTransform.sizeDelta = new Vector2(
                _playerMaxX - ( _sortedPlayerXPoses[ 1 ] + _playerMinX ) / 2f - adjThreshold  + 1, splitScreenLineHeight );
            return;
        }


        // One player is far off to the right
        if( abDist > adjThreshold && bcDist < adjThreshold )
        {
            var hopeOffset = GGJ.Map(
                _sortedPlayerXPoses[ 2 ] - _sortedPlayerXPoses[ 1 ], 0, adjThreshold, theHopeOffset, 0 );

            hopeOffset *=
                GGJ.ClampedMap01( ( _playerMaxX + _sortedPlayerXPoses[ 1 ] ) / 2f - _playerMinX, 
                    acThreshold + adjThreshold, adjThreshold * 1.5f );

            cameras[ 0 ].rect = new Rect( 0, 0, w, 1f );
            cameras[ 1 ].rect = new Rect( w, 0, 2 * w, 1f );
            SetCameraPosition( cameras[ 0 ], _lerpXs[ 0 ] - hopeOffset, 1 / 6f );
            SetCameraPosition( cameras[ 1 ], _lerpXs[ TwoThirdsRightIndex ], 2 / 3f );

            splitScreenLines[ 2 ].rectTransform.sizeDelta = new Vector2(
                ( _playerMaxX + _sortedPlayerXPoses[ 1 ] ) / 2f - _playerMinX - adjThreshold + 1, splitScreenLineHeight );
            return;
        }


        // All 3 players are far from each other
        EnableCameras( 0, 1, 2 );
        for( var i = 0; i < maxNumPlayers; i++ )
        {
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _lerpXs[ i ], w / 2f + w * i );
            splitScreenLines[ 0 ].rectTransform.sizeDelta =
                new Vector2( _playerMaxX - _sortedPlayerXPoses[ 1 ] - adjThreshold, splitScreenLineHeight );
            splitScreenLines[ 2 ].rectTransform.sizeDelta =
                new Vector2( _sortedPlayerXPoses[ 1 ] - _playerMinX - adjThreshold, splitScreenLineHeight );
        }
    }

    void TriCam2()
    {
        var a = _playerMinX;
        var b = _sortedPlayerXPoses[ 1 ];
        var c = _playerMaxX;
        
        
        var ac = c - a;
        var ab = b - a;
        var bc = c - b;
        var acThreshold = cameraSplitDistance * 4f / 3f;
        var adjThreshold = cameraSplitDistance * 2f / 3f; // TODO extract these to constants
        var twelfth = cameraSplitDistance / 6f; // 12th of screen -> 6th of cam split distance (half of screen)
            
        var w = 1f / maxNumPlayers;

        var abMid = ( a + b ) / 2f;
        var bcMid = ( b + c ) / 2f;
        
        splitScreenLines[ 0 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 2 ].rectTransform.sizeDelta = Vector2.zero;

        // if( acDist < acThreshold && abDist < adjThreshold && bcDist < adjThreshold )
        // if( acDist < acThreshold )
        if( c - abMid < cameraSplitDistance && bcMid - a < cameraSplitDistance )
        {
            EnableCameras( 0 );
            cameras[ 0 ].rect = new Rect( 0, 0, 1f, 1f );
            
            var bias = 0f;
            if( bc < ab )
                bias = GGJ.Map( bc, 0, adjThreshold, twelfth, 0 );
            else
                bias = GGJ.Map( ab, 0, adjThreshold, -twelfth, 0 );

            var groupDistScaling = GGJ.ClampedMap01( ac, adjThreshold, cameraSplitDistance );
            var pairingScaling = GGJ.ClampedMap01( Mathf.Abs( ab - bc ), 0, twelfth );
            
            // NavHelpers.LogCommaSeparated( groupDistScaling, pairingScaling );

            _singleCam3pTarget = ( a + c ) / 2f + bias * Mathf.Max( groupDistScaling * pairingScaling );
            _singleCam3pSmoothing = GGJ.ClampedMap( ac, adjThreshold, cameraSplitDistance, 
                singleCam3pSmoothingMinLerp, singleCam3pSmoothingMaxLerp );
            _singleCam3pLerp = Mathf.Lerp( _singleCam3pLerp, _singleCam3pTarget, _singleCam3pSmoothing * Time.deltaTime );

            // Debug.Log( _singleCam3pSmoothing );
            
            SetCameraPosition( cameras[ 0 ], _singleCam3pLerp, 0.5f );

            foreach( var l in splitScreenLines )
                l.rectTransform.sizeDelta = Vector2.zero;

            return;
        }

        EnableCameras( 0, 1 );

        // One player is far off to the left
        if( ab < adjThreshold && bc > adjThreshold )
        {
            cameras[ 0 ].rect = new Rect( 0, 0, 2 * w, 1f );
            cameras[ 1 ].rect = new Rect( 2 * w, 0, w, 1f );
            SetCameraPosition( cameras[ 0 ], _lerpXs[ TwoThirdsLeftIndex ], 1 / 3f );
            SetCameraPosition( cameras[ 1 ], _lerpXs[ 2 ], 5 / 6f );

            splitScreenLines[ 0 ].rectTransform.sizeDelta = new Vector2(
                c - abMid - cameraSplitDistance + 0.5f, splitScreenLineHeight );
            return;
        }


        // One player is far off to the right
        if( ab > adjThreshold && bc < adjThreshold )
        {
            cameras[ 0 ].rect = new Rect( 0, 0, w, 1f );
            cameras[ 1 ].rect = new Rect( w, 0, 2 * w, 1f );
            SetCameraPosition( cameras[ 0 ], _lerpXs[ 0 ], 1 / 6f );
            SetCameraPosition( cameras[ 1 ], _lerpXs[ TwoThirdsRightIndex ], 2 / 3f );

            splitScreenLines[ 2 ].rectTransform.sizeDelta = new Vector2(
                bcMid - a - cameraSplitDistance + 0.5f, splitScreenLineHeight );
            return;
        }


        // All 3 players are far from each other
        EnableCameras( 0, 1, 2 );
        for( var i = 0; i < maxNumPlayers; i++ )
        {
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _lerpXs[ i ], w / 2f + w * i );
            splitScreenLines[ 0 ].rectTransform.sizeDelta =
                new Vector2( c - b - adjThreshold + 0.5f, splitScreenLineHeight );
            splitScreenLines[ 2 ].rectTransform.sizeDelta =
                new Vector2( b - a - adjThreshold + 0.5f, splitScreenLineHeight );
        }
    }

    void EnableCameras( params int[] cameraIndices )
    {
        _cameraFollowSpeed = cameraFollowSpeeds[ cameraIndices.Length - 1 ];

        for( var i = 0; i < cameras.Length; i++ )
            cameras[ i ].enabled = cameraIndices.Contains( i );
    }

    void SetCameraPosition( Component cam, float x, float offsetYPct )
    {
        var t = cam.transform;
        var p = t.position;
        t.position = new Vector3( cameraOffset + x, _initY + ( -1 + 2f * offsetYPct ) * splitViewPortOffsetY, p.z );
    }

    void SortPlayerXPoses()
    {
        var adjustedXPoses = ( from Transform p in playersParent
            where p.gameObject.activeInHierarchy
            select CameraCompensation( p.position ) ).ToList();
        _sortedPlayerXPoses = adjustedXPoses.OrderBy( p => p ).ToArray();
        // NavHelpers.LogArray( _sortedPlayerXPoses );
    }

    float CameraCompensation( Vector3 pos )
    {
        // var rot = Mathf.Atan2( cameraUp.z, cameraUp.x );
        var right = new Vector3( cameraUp.z, 0, -cameraUp.x );
        return Vector3.Project( pos, right ).x * projectionFactor;
    }

    void LerpCurrentValues()
    {
        // _lerpXs[ MidpointIndex ] = Mathf.Lerp( _lerpXs[ MidpointIndex ], _targetXs[ MidpointIndex ], Time.deltaTime * 10f );
        // for( var i = 0; i < _lerpXs.Length; i++ )
        // {
        //     if( i == MidpointIndex ) continue;
        //     _lerpXs[ i ] = _targetXs[ i ] + ( _targetXs[ MidpointIndex ] - _lerpXs[ MidpointIndex ] );
        // }
        
        for( var i = 0; i < _lerpXs.Length; i++ )
            _lerpXs[ i ] = Mathf.Lerp( _lerpXs[ i ], _targetXs[ i ], Time.deltaTime * _cameraFollowSpeed );
    }

    void CalcTargetValues()
    {
        for( var i = 0; i < _sortedPlayerXPoses.Length; i++ )
            _targetXs[ i ] = _sortedPlayerXPoses[ i ];

        _playerMinX = _sortedPlayerXPoses.Min();
        _playerMaxX = _sortedPlayerXPoses.Max();

        _targetXs[ 3 ] = ( _playerMinX + _playerMaxX ) / 2f;

        if( maxNumPlayers < 3 ) return;
        _targetXs[ 4 ] = ( _sortedPlayerXPoses[ 0 ] + _sortedPlayerXPoses[ 1 ] ) / 2f;
        _targetXs[ 5 ] = ( _sortedPlayerXPoses[ 1 ] + _sortedPlayerXPoses[ 2 ] ) / 2f;
    }

    void CopyTargetToLerp()
    {
        for( var i = 0; i < _lerpXs.Length; i++ )
            _lerpXs[ i ] = _targetXs[ i ];
    }
}