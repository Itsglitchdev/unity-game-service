using UnityEngine;
using Unity.Services.Leaderboards;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameLeaderboard : MonoBehaviour
{
    private const string LEADERBOARD_ID = "Play_Flare";

    [Header("References")]
    [SerializeField] private GameObject gameLeaderBoardItemPrefab;
    [SerializeField] private GameObject gameLeaderBoardPanelParent;

    private async void OnEnable()
    {
        await LoadLeaderboard();
    }

    private async Task LoadLeaderboard()
    {
        // Fetch leaderboard entries (Top 25)
        var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
            LEADERBOARD_ID,
            new GetScoresOptions { Limit = 25 }
        );

        // Clear old leaderboard items
        foreach (Transform child in gameLeaderBoardPanelParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new leaderboard items
        foreach (var entry in scoresResponse.Results)
        {
            GameObject item = Instantiate(gameLeaderBoardItemPrefab, gameLeaderBoardPanelParent.transform);
            GameLeaderBoardItem leaderboardItem = item.GetComponent<GameLeaderBoardItem>();

            string displayName = string.IsNullOrEmpty(entry.PlayerName)
                ? $"Guest_{entry.PlayerId.Substring(0, 4)}"
                : entry.PlayerName;

            leaderboardItem.SetItem((entry.Rank + 1).ToString(), displayName, entry.Score.ToString());
        }
    }
}
