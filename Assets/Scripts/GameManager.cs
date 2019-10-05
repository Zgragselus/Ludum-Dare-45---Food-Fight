using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Level CurrentLevel;

    private Text _debugText;

    private AiControllerUnit _unit;

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
        _debugText = textGo.GetComponent<Text>();


        if (CurrentLevel.TryGetAnyWalkablePosition(out Vector2Int posToSpawnAiUnit))
        {
            Debug.Log($"spawning at {posToSpawnAiUnit}");

            var unitGo = new GameObject("unit");
            _unit = unitGo.AddComponent<AiControllerUnit>();
            _unit.CurrentPosition = posToSpawnAiUnit;
            _unit.CurrentDirection = Vector2Int.up; 
        }
    }

    private void Update()
    {
        string tmp = "";
        for (int i = 0; i < CurrentLevel.Size; i++)
        {
            for (int j = 0; j < CurrentLevel.Size; j++)
            {
                if (_unit.CurrentPosition == new Vector2Int(i, j))
                {
                    tmp += '@';
                }
                else
                {
                    tmp += (char)CurrentLevel.Map[i, j];
                }
            }
            tmp += "\r\n";
        }
        _debugText.text = tmp;
    }
}
