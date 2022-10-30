using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRot : MonoBehaviour
{
    [ SerializeField ] Vector3 randomEulerRot;
    void Start() =>
        transform.rotation = Quaternion.Euler( new Vector3( 
            Random.value * randomEulerRot.x, 
            Random.value * randomEulerRot.y, 
            Random.value * randomEulerRot.z ) );
}
