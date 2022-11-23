using UnityEngine;

public class SineFloat : MonoBehaviour
{
    [ SerializeField ] float sinePeriod;
    [ SerializeField ] float sineStrength;

    Vector3 _pos;

    void Start() => _pos = transform.position;

    // TODO Look shopnpc at player (rotate armature)
    
    void Update() => transform.position =
        new Vector3( _pos.x, _pos.y + sineStrength * Mathf.Sin( Time.time / sinePeriod ), _pos.z );
}