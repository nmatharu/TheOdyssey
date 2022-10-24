using UnityEngine;

public class InGameStatistics
{
    int _timeAliveTimeSteps;
    int _kills;
    int _deaths;
    float _metresTravelled;
    float _damageDealt;
    float _mostDamageDealt;
    float _damageTaken;
    int _highestLevel;
    float _goldCollected;

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
    public void CollectGold( float gold ) => _goldCollected += gold;
}