using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using UnityEngine.Events;
using System;
public class PlayFabLogin : MonoBehaviour
{
	public static PlayFabLogin Instance;
	private const string TITLE_ID = " C0FA1";
	public const string STATESTIC_NAME = "NewLeaderboard";
	public const string UserDataKey = "Characters";
	private string PlayerId;
	private string displayName;
	private Action onloginSuccess;
	private Action<string> onloginFailure;
	private Action<LeaderboardData[]> OnLeaderboardGetAction, OnLeaderboardGetAroundPlayerAction;

	private void Awake()
	{
		Instance = this;
	}

	#region Public Methods
	public void Register(string username, string displayName, string password, Action onloginSuccess = null, Action<string> onLoginFailure = null)
	{
		if (password.Length < 6)
		{
			print("Password is too short it should be grater than 6 characters");
		}

		var request = new RegisterPlayFabUserRequest
		{
			Email = username,
			Username = displayName,
			DisplayName = displayName,
			Password = password,
			RequireBothUsernameAndEmail = false,
		};
		this.onloginSuccess = onloginSuccess;
		this.onloginFailure = onLoginFailure;
		PlayFabClientAPI.RegisterPlayFabUser(request, OnLoginSuccess, OnLoginFailure);
	}

	public void Login(string username, Action onloginSuccess = null, Action<string> onLoginFailure = null)
	{
		var request = new LoginWithCustomIDRequest()
		{
			CustomId = username,
			CreateAccount = true,
			InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
			{
				GetPlayerProfile = true
			}
		};
		this.onloginFailure = onLoginFailure;
		this.onloginSuccess = onloginSuccess;
		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

	}
	public void Login(string email, string pass, Action onloginSuccess = null, Action<string> onLoginFailure = null)
	{
		var request = new LoginWithEmailAddressRequest()
		{
			Email = email,
			Password = pass,
			InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
			{
				GetPlayerProfile = true
			}
		};
		this.onloginFailure = onLoginFailure;
		this.onloginSuccess = onloginSuccess;
		PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
	}
	public void ResetPassword(string email)
	{
		var request = new SendAccountRecoveryEmailRequest
		{
			Email = email,
			TitleId = TITLE_ID,
		};
		PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
	}
	public void LogOut()
	{
		PlayFabClientAPI.ForgetAllCredentials();
		PlayerId= null;
		displayName = null;

	}

	public void SendUserDataAsJson()
	{
		string data = JsonConvert.SerializeObject(new Character("Shivanagouda", 0, 0));
		var request = new UpdateUserDataRequest
		{
			Data = new System.Collections.Generic.Dictionary<string, string>()
			{
				{UserDataKey,data }
			}
		};
		PlayFabClientAPI.UpdateUserData(request, OnDataUpdated, OnError);
	}
	public void UpdateUserDisplayName(string newName)
	{
		var request = new UpdateUserTitleDisplayNameRequest
		{
			DisplayName = newName
		};
		PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameChanged, OnError);
	}


	public void GetUserData()
	{
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
	}
	public void SendLeaderboard(string statesticName, int score)
	{
		var request = new UpdatePlayerStatisticsRequest
		{
			Statistics = new System.Collections.Generic.List<StatisticUpdate>()
			{
				new StatisticUpdate
				{
					StatisticName=statesticName,
					Value=score
				}
			}
		};
		PlayFabClientAPI.UpdatePlayerStatistics(request, OnStatisticsUpdated, OnStatesticUpdateError);
	}

	public void GetLeaderboard(string statesticName, Action<LeaderboardData[]> OnLeaderboardGetAction)
	{
		var request = new GetLeaderboardRequest
		{
			StatisticName = statesticName,
			StartPosition = 0,
			MaxResultsCount = 10,
		};
		this.OnLeaderboardGetAction = OnLeaderboardGetAction;
		PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
	}
	public void GetLeaderboardAroundPlayer(string statesticName, Action<LeaderboardData[]> OnLeaderboardGet)
	{
		var request = new GetLeaderboardAroundPlayerRequest
		{
			StatisticName = statesticName,
			MaxResultsCount = 9,
		}; 
		OnLeaderboardGetAroundPlayerAction = OnLeaderboardGet;
		PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardGetAroundPlayer, OnError);
	}
	#endregion
	#region Callback Methods

	private void OnLoginSuccess(LoginResult result)
	{
		SendLeaderboard(STATESTIC_NAME, 100);
		PlayerId = result.PlayFabId;
		if (result.InfoResultPayload.PlayerProfile != null)
		{
			displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
		}
		else
		{
			displayName = null;
		}
		onloginSuccess?.Invoke();

	}
	private void OnLoginSuccess(RegisterPlayFabUserResult result)
	{
		onloginSuccess?.Invoke();

	}
	private void OnLoginFailure(PlayFabError error)
	{
		print("Login Failure");
		onloginFailure?.Invoke(error.ErrorMessage);
	}
	private void OnPasswordReset(SendAccountRecoveryEmailResult result)
	{
		print("Password reset email send to");
	}
	private void OnDisplayNameChanged(UpdateUserTitleDisplayNameResult result)
	{
		print("Username changed");
	}

	private void OnStatisticsUpdated(UpdatePlayerStatisticsResult result)
	{
		print("Leaderboard Updated");
	}
	private void OnStatesticUpdateError(PlayFabError error)
	{
		print("Statestics not sent caused an error");
	}
	private void OnDataUpdated(UpdateUserDataResult result)
	{
		print("data is updated");
	}
	private void OnDataRecieved(GetUserDataResult result)
	{
		if (result.Data != null && result.Data.ContainsKey(UserDataKey))
		{
			Character character = JsonConvert.DeserializeObject<Character>(result.Data[UserDataKey].Value);
		}
	}


	private void OnLeaderboardGet(GetLeaderboardResult result)
	{
		LeaderboardData[] leaderboardData = new LeaderboardData[result.Leaderboard.Count];
		int i = 0;
		foreach (var item in result.Leaderboard)
		{
			leaderboardData[i] = new LeaderboardData
			{
				Rank = item.Position,
				Name = item.DisplayName,
				Score = item.StatValue
			};
			i++;
		}
		OnLeaderboardGetAction?.Invoke(leaderboardData);

	}
	private void OnLeaderboardGetAroundPlayer(GetLeaderboardAroundPlayerResult result)
	{
		int PlayerIndex = 0;
		int i = 0;
		LeaderboardData[] leaderboardData = new LeaderboardData[result.Leaderboard.Count];
		foreach (var item in result.Leaderboard)
		{
			leaderboardData[i] = new LeaderboardData
			{
				Rank = item.Position,
				Name = item.DisplayName,
				Score = item.StatValue
			};
			
			if (item.PlayFabId == PlayerId)
				leaderboardData[i].isMe=true;
			i++;
		}
		OnLeaderboardGetAroundPlayerAction?.Invoke(leaderboardData);
	}
	private void OnError(PlayFabError error)
	{
		print("errorOccured");
	}
	#endregion
}
public class Character
{
	public string name;
	public int level;
	public float health;

	public Character(string name, int level, float health)
	{
		this.name = name;
		this.level = level;
		this.health = health;
	}
}