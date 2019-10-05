using System;
using System.Collections.Generic;
using System.Linq;
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
        var results = new List<(Unit unit, Vector2Int position, ActionType type, object payload)>();

        // do all regular ticks
        foreach (var unit in CurrentLevel.ActiveUnits)
        {
            unit.SubmitActions();
        }

        // simulate all actions - record events to play in visuals etc.
        CurrentLevel.DoActions(results);

        foreach (var waits in results.Where(x => x.type == ActionType.Wait))
        {
            waits.unit.Wait();
        }

        foreach (var moves in results.Where(x => x.type == ActionType.Move))
        {
            moves.unit.Move(moves.position);
        }

        foreach (var pickups in results.Where(x => x.type == ActionType.PickUpItem))
        {
            pickups.unit.Pickup();
        }

        foreach (var attacks in results.Where(x => x.type == ActionType.Attack))
        {
            attacks.unit.Attack();
        }

        foreach (var deaths in results.Where(x => x.type == ActionType.Die))
        {
            deaths.unit.Die();
        }

        // kill units that died this turn - since we do not have speed factor in the game, all units get a chance to do a hit even if they might die during the round
        var foo = new List<Unit>(CurrentLevel.ActiveUnits);
        foreach (var unit in foo)
        {
            unit.PostTick();
        }
    }
}
