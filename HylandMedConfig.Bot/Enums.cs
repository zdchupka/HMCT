namespace HylandMedConfig.Common
{
	public enum UserStatus
	{
		Offline,
		Inactive,
		Active,
	}

	public enum TcpCommands : int
	{
		Hello = 2012,
		OK = 0,
		Login = 1,
		Error = 6,
		RequestUserStats = 7,
		SendMessage = 10,
		Received = 11,
		ServerDown = 12,
		UserLoggedOn = 13,
		UserLoggedOut = 14,
		AppShutdown = 15,
		EnteredText = 16,
		ClearedText = 17,
		UserEnteredText = 18,
		UserClearedText = 19,
		UserIdle = 20,
		UserActive = 21,
		UserReadyForFooz = 22,
		UserNotReadyForFooz = 23,
		FoozGameReady = 25,
		ChangeMood = 26,
		UserVotedOnPoll = 31,
		MoodChanged = 34,
		SendMessageSuccess = 36,
		UserStatsReceived = 45,
		UserRatedMessage = 55,
		SendWhiteboardPoints = 70,
		WhiteboardPointsReceived = 71,
		LockWhiteboard = 72,
		WhiteboardLocked = 73,
		UnlockWhiteboard = 74,
		WhiteboardUnlocked = 75,
		ClearWhiteboard = 76,
		WhiteboardCleared = 77,
		UserRemote = 90,
		UserNotRemote = 91,
		OnUserRemote = 92,
		OnUserNotRemote = 93,

		ClosePoll = 94,
	}

	public enum MessageRating : int
	{
		Neutral = 0,
		ThumbsUp = 1,
		ThumbsDown = 2,
	}
}
