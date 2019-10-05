using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiControllerUnit : Unit
{
    public override void Tick()
    {
        base.Tick();

        if (IsFacingObstacle())
        {
            TurnToAvailableDirection();
        }

        if (HasPlayerInLineOfSight(out Vector2Int playerPosition))
        {
            Intercept(playerPosition);
        }
        else
        {
            Wander();
        }
    }

    private void TurnToAvailableDirection()
    {
        Span<Vector2Int> neighbours = stackalloc Vector2Int[4];
        int count = GameManager.Instance.CurrentLevel.GetNeighbours(CurrentPosition, in neighbours);
        // turn to random non-blocked direction
        CurrentDirection = neighbours[Random.Range(0, count)] - CurrentPosition;
    }

    private bool IsFacingObstacle()
    {
        return !GameManager.Instance.CurrentLevel.IsWalkable(CurrentPosition + CurrentDirection) || !GameManager.Instance.CurrentLevel.IsWalkable(CurrentPosition + CurrentDirection * 2);
    }

    private bool HasPlayerInLineOfSight(out Vector2Int playerPosition)
    {
        playerPosition = default;
        return false;
    }

    private void Intercept(Vector2Int playerPosition)
    {
        Vector2Int diff = playerPosition - CurrentPosition;
        // x is vertical
        // y is horizontal
        // (0,0) = top left
        if (diff.x != 0 && diff.x > diff.y)
        {
            if (diff.x > 0)
            {
                TryMove(CurrentPosition);
            }
            else
            {
                TryMove(CurrentPosition);
            }
        }
        else if (diff.y != 0 && diff.y > diff.x)
        {
            if (diff.y > 0)
            {
                TryMove(CurrentPosition);
            }
            else
            {
                TryMove(CurrentPosition);
            }
        }
    }

    private void Wander()
    {
        TryMove(CurrentDirection);
    }
}
