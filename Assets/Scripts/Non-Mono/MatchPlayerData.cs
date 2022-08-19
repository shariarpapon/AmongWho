[System.Serializable]
public class MatchPlayerData
{
    public string playerName;
    public UnityEngine.Color color;
    public bool isDead;

    public MatchPlayerData(string playerName, UnityEngine.Color color)
    {
        this.playerName = playerName;
        this.color = color;
    }

}
