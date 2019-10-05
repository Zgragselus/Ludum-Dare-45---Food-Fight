using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiControllerUnit : Unit
{
    public override void SubmitActions()
    {
        base.SubmitActions();

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
        playerPosition = GameManager.Instance.CurrentPlayer.CurrentPosition;

        Vector2Int checkedPosition = CurrentPosition;

        while (true)
        {
            if (playerPosition == checkedPosition)
            {
                Debug.Log("has line of sight");

                return true;
            }

            if (!GameManager.Instance.CurrentLevel.IsWalkable(checkedPosition))
            {
                Debug.Log("does not have line of sight");

                return false;
            }

            Vector2Int diff = playerPosition - checkedPosition;

            var deltaX = Mathf.Abs(diff.x);
            var deltaY = Mathf.Abs(diff.y);

            var moveX = checkedPosition.x >= playerPosition.x ? -1 : 1;
            var moveY = checkedPosition.y >= playerPosition.y ? -1 : 1;

            if (deltaX > deltaY)
            {
                checkedPosition += new Vector2Int(moveX, 0);
            }
            else
            {
                checkedPosition += new Vector2Int(0, moveY);
            }
        }

    }

    private void Intercept(Vector2Int playerPosition)
    {
        Vector2Int diff = playerPosition - CurrentPosition;

        var deltaX = Mathf.Abs(diff.x);
        var deltaY = Mathf.Abs(diff.y);

        var moveX = CurrentPosition.x >= playerPosition.x ? -1 : 1;
        var moveY = CurrentPosition.y >= playerPosition.y ? -1 : 1;

        if (deltaX > deltaY)
        {
            SubmitMoveAction(new Vector2Int(moveX, 0));
        }
        else
        {
            SubmitMoveAction(new Vector2Int(0, moveY));
        }
    }

    private void Wander()
    {
        SubmitMoveAction(CurrentDirection);
    }
}
