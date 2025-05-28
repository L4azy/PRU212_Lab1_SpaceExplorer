using System.Collections.Generic;

public struct HighscoreListChangedEvent
{
	public List<HighScoreElement> HighscoreList { get; }

	public HighscoreListChangedEvent(List<HighScoreElement> highscoreList)
	{
		HighscoreList = highscoreList;
	}

	public struct LeaderboardUpdatedEvent { }
}
