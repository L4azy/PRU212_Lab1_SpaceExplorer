using System;

[Serializable]
public class PlayerScoreInput
{
	public string playerName;
	public int points;

	public PlayerScoreInput(string name, int points)
	{
		playerName = name;
		this.points = points;
	}
}
