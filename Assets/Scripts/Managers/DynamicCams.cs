using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCams : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] Camera[] cameras;

    int _numPlayers = 1;
    [ SerializeField ] float[] cameraFollowSpeeds;
    [ SerializeField ] float cameraOffset;
    [ SerializeField ] float projectionFactor = 1.3f;
    [ SerializeField ] float splitViewPortOffsetY = 1f;

    [ SerializeField ] float cameraSplitDistance = 10f;

    [ SerializeField ] Image[] splitScreenLines;

    [ SerializeField ] float singleCam3pSmoothingMinLerp = 10f;
    [ SerializeField ] float singleCam3pSmoothingMaxLerp = 1000f;

    [ SerializeField ] CanvasScaler scaler;

    [ SerializeField ] float camXClampMin = 21f;

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

    void Start()
    {
        SortPlayerXPoses();

        _lerpXs = new float[ 6 ];
        _targetXs = new float[ 6 ];
        CalcTargetValues();
        CopyTargetToLerp();

        _cameraFollowSpeed = cameraFollowSpeeds[ 0 ];
        splitScreenLineHeight = scaler.referenceResolution.y;

        var fwd = cameras[ 0 ].transform.forward;
        _initY = cameras[ 0 ].transform.position.y;
        // Debug.Log( _initY );
        cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    void Update()
    {
        SortPlayerXPoses();
        CalcTargetValues();
        LerpCurrentValues();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch( _numPlayers )
        {
            case 1:
                SingleCam();
                break;
            case 2:
                DualCam();
                break;
            case 3:
                TriCam();
                break;
        }
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
        for( var i = 0; i < _numPlayers; i++ )
        {
            var w = 1f / _numPlayers;
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _lerpXs[ i ], w / 2f + w * i );
        }

        // TODO add smoothing to dual cam

        splitScreenLines[ 0 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 2 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 1 ].rectTransform.sizeDelta =
            new Vector2( SplitScreenBarWidth( _playerMaxX - _playerMinX - cameraSplitDistance ),
                splitScreenLineHeight );
    }

    void TriCam()
    {
        splitScreenLines[ 1 ].rectTransform.sizeDelta = Vector2.zero;

        var a = _playerMinX;
        var b = _sortedPlayerXPoses[ 1 ];
        var c = _playerMaxX;

        var ac = c - a;
        var ab = b - a;
        var bc = c - b;
        var acThreshold = cameraSplitDistance * 4f / 3f;
        var adjThreshold = cameraSplitDistance * 2f / 3f; // TODO extract these to constants
        var twelfth = cameraSplitDistance / 6f; // 12th of screen -> 6th of cam split distance (half of screen)

        var w = 1f / _numPlayers;

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
                bias = JBB.Map( bc, 0, adjThreshold, twelfth, 0 );
            else
                bias = JBB.Map( ab, 0, adjThreshold, -twelfth, 0 );

            var groupDistScaling = JBB.ClampedMap01( ac, adjThreshold, cameraSplitDistance );
            var pairingScaling = JBB.ClampedMap01( Mathf.Abs( ab - bc ), 0, twelfth );

            // NavHelpers.LogCommaSeparated( groupDistScaling, pairingScaling );

            _singleCam3pTarget = ( a + c ) / 2f + bias * Mathf.Max( groupDistScaling * pairingScaling );
            _singleCam3pSmoothing = JBB.ClampedMap( ac, adjThreshold, cameraSplitDistance,
                singleCam3pSmoothingMinLerp, singleCam3pSmoothingMaxLerp );
            _singleCam3pLerp =
                Mathf.Lerp( _singleCam3pLerp, _singleCam3pTarget, _singleCam3pSmoothing * Time.deltaTime );

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
                SplitScreenBarWidth( c - abMid - cameraSplitDistance ), splitScreenLineHeight );
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
                SplitScreenBarWidth( bcMid - a - cameraSplitDistance ), splitScreenLineHeight );
            return;
        }


        // All 3 players are far from each other
        EnableCameras( 0, 1, 2 );
        for( var i = 0; i < _numPlayers; i++ )
        {
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _lerpXs[ i ], w / 2f + w * i );
            splitScreenLines[ 0 ].rectTransform.sizeDelta =
                new Vector2( SplitScreenBarWidth( c - b - adjThreshold ), splitScreenLineHeight );
            splitScreenLines[ 2 ].rectTransform.sizeDelta =
                new Vector2( SplitScreenBarWidth( b - a - adjThreshold ), splitScreenLineHeight );
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
        _numPlayers = adjustedXPoses.Count;
        _sortedPlayerXPoses = adjustedXPoses.OrderBy( p => p ).ToArray();
    }

    float CameraCompensation( Vector3 pos )
    {
        // var rot = Mathf.Atan2( cameraUp.z, cameraUp.x );
        var right = new Vector3( cameraUp.z, 0, -cameraUp.x );
        return Mathf.Max( camXClampMin, Vector3.Project( pos, right ).x * projectionFactor );
    }

    void LerpCurrentValues()
    {
        for( var i = 0; i < _lerpXs.Length; i++ )
            _lerpXs[ i ] = Mathf.Lerp( _lerpXs[ i ], _targetXs[ i ], Time.deltaTime * _cameraFollowSpeed );
    }

    void CalcTargetValues()
    {
        for( var i = 0; i < _sortedPlayerXPoses.Length; i++ )
            _targetXs[ i ] = _sortedPlayerXPoses[ i ];

        if( _sortedPlayerXPoses.Length == 0 ) return;

        _playerMinX = _sortedPlayerXPoses.Min();
        _playerMaxX = _sortedPlayerXPoses.Max();

        _targetXs[ 3 ] = ( _playerMinX + _playerMaxX ) / 2f;

        if( _numPlayers < 3 ) return;
        _targetXs[ 4 ] = ( _sortedPlayerXPoses[ 0 ] + _sortedPlayerXPoses[ 1 ] ) / 2f;
        _targetXs[ 5 ] = ( _sortedPlayerXPoses[ 1 ] + _sortedPlayerXPoses[ 2 ] ) / 2f;
    }

    void CopyTargetToLerp()
    {
        for( var i = 0; i < _lerpXs.Length; i++ )
            _lerpXs[ i ] = _targetXs[ i ];
    }

    float SplitScreenBarWidth( float dist ) => 
        1f + JBB.ClampedMap( Mathf.Sqrt( dist ), 0, 20, 0, 49 );
}