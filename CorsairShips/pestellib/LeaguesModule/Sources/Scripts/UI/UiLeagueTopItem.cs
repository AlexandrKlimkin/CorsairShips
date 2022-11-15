using UnityEngine;
using UnityEngine.UI;

public class UiLeagueTopItem : MonoBehaviour
{
    [SerializeField] private Text _rank;
    [SerializeField] private Text _name;
    [SerializeField] private Text _score;

    public void SetData(int rank, string name, long score, bool highlight)
    {
        _rank.text = rank.ToString();
        _name.text = name;
        _score.text = score.ToString();
        if (highlight)
        {
            _rank.fontStyle = FontStyle.Bold;
            _name.fontStyle = FontStyle.Bold;
            _score.fontStyle = FontStyle.Bold;
        }
    }
}
