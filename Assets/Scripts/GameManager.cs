using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Level CurrentLevel => _levels[_currentLevelIdx];

    public int LevelsToGenerate = 5;

    private Camera _cam;

    public int LevelResolution = 64;
    public int LevelMinRoomSize = 5;
    public int LevelMaxDepthOffset = 3;

    private Level[] _levels;
    private int _currentLevelIdx = 0;

    private GameObject _levelsParent;

    public Player CurrentPlayer;

    private void Start()
    {
        // temporary spawning code
        // should setup the whole game - e.g., 10 levels, including the first "tutorial" level and the "boss" level
        _cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        _levels = new Level[LevelsToGenerate];

        _levelsParent = new GameObject("Levels");

        for (int i = 0; i < LevelsToGenerate; i++)
        {
            _levels[i] = new Level();
            _levels[i].Generate(LevelResolution, LevelMinRoomSize, LevelMaxDepthOffset);

            var levelParent = new GameObject($"Level {i}");
            levelParent.transform.parent = _levelsParent.transform;
            _levels[i].WorldParent = levelParent.transform;

            _levels[i].WorldParent.localPosition = new Vector3(i * LevelResolution * 2, 0, 0);

            for (int x = 0; x < _levels[i].Size; x++)
            {
                for (int y = 0; y < _levels[i].Size; y++)
                {
                    var tile = WorldGenerator.Instance.SpawnStaticWorldBlock(_levels[i], new Vector2Int(x, y), _levels[i].Map[x, y]);
                    if (tile is EntranceTile enter)
                    {
                        enter.PreviousLevelIdx = i - 1;
                    }

                    if (tile is ExitTile exit)
                    {
                        exit.NextLevelIdx = i + 1 >= LevelsToGenerate ? -1 : i + 1;
                    }
                }
            }
        }

        _currentLevelIdx = 0;

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
                player.transform.position = new Vector3(player.CurrentPosition.x, 0, player.CurrentPosition.y);
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
