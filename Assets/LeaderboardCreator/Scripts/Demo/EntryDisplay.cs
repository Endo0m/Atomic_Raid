using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Dan.Models;

public class EntryDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText, usernameText, scoreText, timeText;

    public void SetEntry(Entry entry)
    {
        rankText.text = entry.RankSuffix();
        usernameText.text = entry.Username;
        scoreText.text = entry.Score.ToString();

        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(entry.Date);
        timeText.text = $"{dateTime.Hour:00}:{dateTime.Minute:00}:{dateTime.Second:00} (UTC)\n{dateTime:dd/MM/yyyy}";

        GetComponent<Image>().color = entry.IsMine() ? Color.yellow : Color.white;
    }
}