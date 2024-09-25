using Unity.Services.Analytics;
using UnityEngine;
namespace UI
{
	public class UIController : MonoBehaviour
	{
		[SerializeField] Transform leaderboardParent;
		[SerializeField] LeaderboardElement leaderboardItemPrefab;

		private void Start()
		{
			UpdateLeaderboard();
		}

		public void UpdateLeaderboard()
		{
			PlayFabLogin.Instance.GetLeaderboard(PlayFabLogin.STATESTIC_NAME, (leaderboardData) =>
			{
				for (int i = 0; i < leaderboardParent.childCount; i++)
				{
					Destroy(leaderboardParent.GetChild(i));
				}
				foreach (var leaderboardItem in leaderboardData)
				{
					var element = Instantiate(leaderboardItemPrefab, leaderboardParent);
					element.Rank.text = leaderboardItem.Rank.ToString();
					element.Name.text = leaderboardItem.Name;
					element.Score.text = leaderboardItem.Score.ToString();
				}
			});

		}
	}
}
public class LeaderboardData
{
	public string Name;
	public int Rank;
	public float Score;
	public bool isMe = false;
}
