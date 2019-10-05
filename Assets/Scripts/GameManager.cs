using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Level CurrentLevel;

    private void Start()
    {
        CurrentLevel = new Level();

        CurrentLevel.Generate();

        string tmp = "";
        for (int i = 0; i < CurrentLevel.Size; i++)
        {
            for (int j = 0; j < CurrentLevel.Size; j++)
            {
                tmp += (char)CurrentLevel.Map[i, j];
            }
            tmp += "\r\n";
        }
        var textGo = GameObject.Find("DebugText");
        var text = textGo.GetComponent<Text>();
        text.text = tmp;
    }
}
