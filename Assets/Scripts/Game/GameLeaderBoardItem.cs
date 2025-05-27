using UnityEngine;
using TMPro;

public class GameLeaderBoardItem : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _rankText, _nameText, _scoreText;

    public void SetItem(string rank, string playerName, string score)
    {
        _rankText.text = rank;
        _nameText.text = playerName;
        _scoreText.text = score;
    }
    
}