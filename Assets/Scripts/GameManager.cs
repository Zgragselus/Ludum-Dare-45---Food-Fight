using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Level CurrentLevel;

    private Camera _cam;

    public int LevelResolution = 64;
    public int LevelMinRoomSize = 5;
    public int LevelMaxDepthOffset = 3;

    public Player CurrentPlayer;

    private void Start()
    {
        // temporary spawning code
        // should setup the whole game - e.g., 10 levels, including the first "tutorial" level and the "boss" level
        _cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        CurrentLevel = new Level();

        CurrentLevel.Generate(LevelResolution, LevelMinRoomSize, LevelMaxDepthOffset);

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
                CurrentPlayer = player;
            }
            else
            {
                throw new InvalidOperationException("nowhere to spawn the player");
            }
        }

    }

    public void StepCurrentLevel()
    {
        // do all regular ticks
        foreach (var unit in CurrentLevel.Units)
        {
            unit.Tick();
        }

        // kill units that died this turn - since we do not have speed factor in the game, all units get a chance to do a hit even if they might die during the round
        foreach (var unit in CurrentLevel.Units)
        {
            unit.PostTick();
        }

        var results = new List<(Unit, Vector2Int, ActionType type)>();

        CurrentLevel.DoActions(results);
    }
}
