using UnityEngine;

public class InGameStatistics
{
    public int _timeAliveTimeSteps;
    public int _kills;
    public int _deaths;
    public float _metresTravelled;
    public float _damageDealt;
    public float _mostDamageDealt;
    public float _damageTaken;
    public int _highestLevel;
    public int _goldCollected;

    public void AliveForFixedTimeStep() => _timeAliveTimeSteps++;

    public void KillEnemy() => _kills++;
    public void Die() => _deaths++;
    public void Move( float metres ) => _metresTravelled += metres;
    public void DealDamage( float dmg )
    {
        _damageDealt += dmg;
        if( dmg > _mostDamageDealt )
            _mostDamageDealt = dmg;
    }
    public void TakeDamage( float dmg ) => _damageTaken += dmg;
    public void LevelChanged( int level )
    {
        if( level > _highestLevel )
            _highestLevel = level;
    }
    public void CollectGold( int gold ) => _goldCollected += gold;
}