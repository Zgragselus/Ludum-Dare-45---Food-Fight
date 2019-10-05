using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiControllerUnit : Unit
{
    private Vector2Int CurrentDirection;

    private void Update()
    {
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
        CurrentDirection = CurrentPosition - neighbours[Random.Range(0, count)];
    }

    private bool IsFacingObstacle()
    {
        return false;
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
                TryMove(CurrentPosition + Vector2Int.down);
            }
            else
            {
                TryMove(CurrentPosition + Vector2Int.up);
            }
        }
        else if (diff.y != 0 && diff.y > diff.x)
        {
            if (diff.y > 0)
            {
                TryMove(CurrentPosition + Vector2Int.right);
            }
            else
            {
                TryMove(CurrentPosition + Vector2Int.left);
            }
        }
    }

    private void Wander()
    {
        TryMove(CurrentDirection);
    }
}
