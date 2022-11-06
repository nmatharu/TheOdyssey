using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCameras : MonoBehaviour
{
    [ SerializeField ] Camera[] cameras;

    int _numPlayers = 1;
    [ SerializeField ] float cameraOffset = -2f;
    [ SerializeField ] float projectionFactor = 1.049149f;
    [ SerializeField ] float splitViewPortOffsetY = -4.73f;

    [ SerializeField ] float cameraSplitDistance = 22f;

    [ SerializeField ] Image[] splitScreenLines;

    [ SerializeField ] float singlePlayerSmoothing = 5f;
    [ SerializeField ] float singleCam2pSmoothingMinLerp = 5f;
    [ SerializeField ] float singleCam2pSmoothingMaxLerp = 20f;
    [ SerializeField ] float singleCam3pSmoothingMinLerp = 5f;
    [ SerializeField ] float singleCam3pSmoothingMaxLerp = 200f;

    [ SerializeField ] CanvasScaler scaler;

    [ SerializeField ] float camXClampMin = 21f;

    float _initY;

    float[] _sortedPlayerXPoses;
    
    // x-positions of players 1-3
    readonly float[] _xs = new float[ 3 ];
    
    // min and max x-position
    float _min;
    float _max;
    
    // two-thirds left => l, two-thirds right => r, midpoint => m
    float _l, _r, _m;

    float _adjThreshold;
    float _twelfth;

    float _singleCam1PLerp;
    float _singleCam2PLerp, _singleCam2PSmoothing;
    float _singleCam3PLerp, _singleCam3PTarget, _singleCam3PSmoothing;

    Vector3 _cameraUp;

    float _splitScreenLineHeight;

    void Start()
    {
        SortPlayerXPoses();
        CalcTargetValues();

        var fwd = cameras[ 0 ].transform.forward;
        _initY = cameras[ 0 ].transform.position.y;

        _cameraUp = new Vector3( fwd.x, 0, fwd.z );

        var aspectRatio = cameras[ 0 ].aspect;

        cameraSplitDistance *= aspectRatio / ( 16f / 9f );
        _adjThreshold = cameraSplitDistance * 2f / 3f;
        _twelfth = cameraSplitDistance / 6f;
        
        splitViewPortOffsetY *= aspectRatio / ( 16f / 9f );
        _splitScreenLineHeight = scaler.referenceResolution.y * ( 16f / 9f ) / aspectRatio;
    }

    void Update()
    {
        SortPlayerXPoses();
        CalcTargetValues();
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

        _singleCam1PLerp = Mathf.Lerp( _singleCam1PLerp, _m, singlePlayerSmoothing * Time.deltaTime );
        SetCameraPosition( cameras[ 0 ], _singleCam1PLerp, 0.5f );

        foreach( var l in splitScreenLines )
            l.rectTransform.sizeDelta = Vector2.zero;
    }

    void DualCam()
    {
        _singleCam2PSmoothing = JBB.ClampedMap( _m, _adjThreshold, cameraSplitDistance,
            singleCam2pSmoothingMinLerp, singleCam2pSmoothingMaxLerp );
        _singleCam2PLerp = Mathf.Lerp( _singleCam2PLerp, _m, _singleCam2PSmoothing * Time.deltaTime );
        var smoothingBehind = _singleCam2PLerp - _m;
        
        if( _max - _min < cameraSplitDistance )
        {
            EnableCameras( 0 );
            cameras[ 0 ].rect = new Rect( 0, 0, 1f, 1f );

            SetCameraPosition( cameras[ 0 ], _singleCam2PLerp, 0.5f );

            foreach( var l in splitScreenLines )
                l.rectTransform.sizeDelta = Vector2.zero;
            return;
        }

        EnableCameras( 0, 1 );
        for( var i = 0; i < _numPlayers; i++ )
        {
            var w = 1f / _numPlayers;
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _xs[ i ] + smoothingBehind, w / 2f + w * i );
        }

        splitScreenLines[ 0 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 2 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 1 ].rectTransform.sizeDelta =
            new Vector2( SplitScreenBarWidth( _max - _min - cameraSplitDistance ),
                _splitScreenLineHeight );
    }

    void TriCam()
    {
        splitScreenLines[ 1 ].rectTransform.sizeDelta = Vector2.zero;

        var a = _xs[ 0 ];
        var b = _xs[ 1 ];
        var c = _xs[ 2 ];

        var ac = c - a;
        var ab = b - a;
        var bc = c - b;

        var w = 1f / _numPlayers;

        var abMid = ( a + b ) / 2f;
        var bcMid = ( b + c ) / 2f;

        splitScreenLines[ 0 ].rectTransform.sizeDelta = Vector2.zero;
        splitScreenLines[ 2 ].rectTransform.sizeDelta = Vector2.zero;

        EnableCameras( 0 );
        var bias = JBB.Map( bc < ab ? bc : ab, 0, _adjThreshold, ( bc < ab ? 1 : -1 ) * _twelfth, 0 );
        var groupDistScaling = JBB.ClampedMap01( ac, _adjThreshold, cameraSplitDistance );
        var pairingScaling = JBB.ClampedMap01( Mathf.Abs( ab - bc ), 0, _twelfth );
        
        _singleCam3PTarget = ( a + c ) / 2f + bias * Mathf.Max( groupDistScaling * pairingScaling );
        _singleCam3PSmoothing = JBB.ClampedMap( ac, _adjThreshold, cameraSplitDistance,
            singleCam3pSmoothingMinLerp, singleCam3pSmoothingMaxLerp );
        _singleCam3PLerp =
            Mathf.Lerp( _singleCam3PLerp, _singleCam3PTarget, _singleCam3PSmoothing * Time.deltaTime );

        var smoothingBehind = _singleCam3PLerp - _singleCam3PTarget;
        
        // if( acDist < acThreshold && abDist < adjThreshold && bcDist < adjThreshold )
        // if( acDist < acThreshold )
        if( c - abMid < cameraSplitDistance && bcMid - a < cameraSplitDistance )
        {
            cameras[ 0 ].rect = new Rect( 0, 0, 1f, 1f );
            SetCameraPosition( cameras[ 0 ], _singleCam3PLerp, 0.5f );
            foreach( var l in splitScreenLines )
                l.rectTransform.sizeDelta = Vector2.zero;
            return;
        }

        EnableCameras( 0, 1 );

        // One player is far off to the left
        if( ab < _adjThreshold && bc > _adjThreshold )
        {
            cameras[ 0 ].rect = new Rect( 0, 0, 2 * w, 1f );
            cameras[ 1 ].rect = new Rect( 2 * w, 0, w, 1f );
            SetCameraPosition( cameras[ 0 ], _l + smoothingBehind, 1 / 3f );
            SetCameraPosition( cameras[ 1 ], c + smoothingBehind, 5 / 6f );

            splitScreenLines[ 0 ].rectTransform.sizeDelta = new Vector2(
                SplitScreenBarWidth( c - abMid - cameraSplitDistance ), _splitScreenLineHeight );
            return;
        }


        // One player is far off to the right
        if( ab > _adjThreshold && bc < _adjThreshold )
        {
            cameras[ 0 ].rect = new Rect( 0, 0, w, 1f );
            cameras[ 1 ].rect = new Rect( w, 0, 2 * w, 1f );
            SetCameraPosition( cameras[ 0 ], a + smoothingBehind, 1 / 6f );
            SetCameraPosition( cameras[ 1 ], _r + smoothingBehind, 2 / 3f );

            splitScreenLines[ 2 ].rectTransform.sizeDelta = new Vector2(
                SplitScreenBarWidth( bcMid - a - cameraSplitDistance ), _splitScreenLineHeight );
            return;
        }


        // All 3 players are far from each other
        EnableCameras( 0, 1, 2 );
        for( var i = 0; i < _numPlayers; i++ )
        {
            cameras[ i ].rect = new Rect( w * i, 0f, w, 1f );
            SetCameraPosition( cameras[ i ], _xs[ i ] + smoothingBehind, w / 2f + w * i );
            splitScreenLines[ 0 ].rectTransform.sizeDelta =
                new Vector2( SplitScreenBarWidth( c - b - _adjThreshold ), _splitScreenLineHeight );
            splitScreenLines[ 2 ].rectTransform.sizeDelta =
                new Vector2( SplitScreenBarWidth( b - a - _adjThreshold ), _splitScreenLineHeight );
        }
    }

    void EnableCameras( params int[] cameraIndices )
    {
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
        var adjustedXPoses = ( from p in GameManager.Instance.CameraPlayers() 
            select CameraCompensation( p.transform.position ) ).ToList();
        _numPlayers = adjustedXPoses.Count;
        _sortedPlayerXPoses = adjustedXPoses.OrderBy( p => p ).ToArray();
    }

    float CameraCompensation( Vector3 pos )
    {
        var right = new Vector3( _cameraUp.z, 0, -_cameraUp.x );
        return Mathf.Max( camXClampMin, Vector3.Project( pos, right ).x * projectionFactor );
    }

    void CalcTargetValues()
    {
        for( var i = 0; i < _sortedPlayerXPoses.Length; i++ )
        {
            _xs[ i ]  = _sortedPlayerXPoses[ i ];
        }
        
        if( _sortedPlayerXPoses.Length == 0 ) return;

        _min = _sortedPlayerXPoses.Min();
        _max = _sortedPlayerXPoses.Max();

        _m = ( _min + _max ) / 2f;

        if( _numPlayers < 3 ) return;
        _l = ( _sortedPlayerXPoses[ 0 ] + _sortedPlayerXPoses[ 1 ] ) / 2f;
        _r = ( _sortedPlayerXPoses[ 1 ] + _sortedPlayerXPoses[ 2 ] ) / 2f;
    }

    static float SplitScreenBarWidth( float dist ) => 
        1f + JBB.ClampedMap( Mathf.Sqrt( dist ), 0, 20, 0, 49 );
}