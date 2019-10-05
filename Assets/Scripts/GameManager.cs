using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Level CurrentLevel;

    private Camera _cam;

    private void Start()
    {
        _cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        CurrentLevel = new Level();

        CurrentLevel.Generate();

        for (int i = 0; i < CurrentLevel.Size; i++)
        {
            for (int j = 0; j < CurrentLevel.Size; j++)
            {
                WorldGenerator.Instance.SpawnStaticWorldBlock(CurrentLevel, new Vector2Int(i, j), CurrentLevel.Map[i, j]);
            }
        }

        if (CurrentLevel.TryGetAnyWalkablePosition(out Vector2Int posToSpawnAiUnit))
        {
            Debug.Log($"spawning at {posToSpawnAiUnit}");

            WorldGenerator.Instance.SpawnUnit(CurrentLevel, posToSpawnAiUnit);

            Span<Vector2Int> neighbours = stackalloc Vector2Int[4];
            int count = CurrentLevel.GetNeighbours(posToSpawnAiUnit, in neighbours);

            if (count > 0)
            {
                var player = WorldGenerator.Instance.CreatePlayer();
                CurrentLevel.AddPlayer(player, neighbours[0]);
                player.UpdatePosition();
                _cam.GetComponent<FollowingCamera>().Target = player.transform;
            }
            else
            {
                throw new InvalidOperationException("nowhere to spawn the player");
            }
        }

    }

    public void StepCurrentLevel()
    {
        foreach (var unit in CurrentLevel.Units)
        {
            unit.Tick();
        }
    }
}
