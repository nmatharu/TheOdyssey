using System.Collections.Generic;

public class GameConfig
{
    public int Difficulty = 1;
    public int PlayerCount = 1;
    public bool Sandbox = false;
    public List<PlayerConfig> Players = new();
    public bool SMAA = false;
    public bool TrailerMode = false;

    public class PlayerConfig
    {
        public readonly int PlayerSkin;
        public readonly string PlayerName;
        public readonly int PlayerId;

        public PlayerConfig( int playerId, string playerName, int playerSkin )
        {
            PlayerSkin = playerSkin;
            PlayerName = playerName;
            PlayerId = playerId;
        }
    }
}