using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrineRandomSpin : MonoBehaviour
{
    [ SerializeField ] Vector3 eulerRot;
    [ SerializeField ] float sinePeriod = 1f;
    [ SerializeField ] float sineStrength = 1f;

    float _offset;
    
    void Start()
    {
        _offset = Random.value * 5f;
        sinePeriod += Random.value;
        sineStrength += Random.value;
    }

    void Update() => transform.Rotate( eulerRot * Time.deltaTime * ( sineStrength * Mathf.Sin( ( Time.time + _offset ) / sinePeriod ) ) );
}
