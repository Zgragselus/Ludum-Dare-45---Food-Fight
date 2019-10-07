using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public int Health;

    public int Armor;

    public int Damage;

    public int Experience;

    public float AnimationJumpHeight = 0.5f;
    public float AnimationScaleFactor = 0.3f;

    public Vector2Int CurrentPosition;

    public Vector2Int CurrentDirection;

    public GameObject LeftHand = null;
    public GameObject RightHand = null;

    private bool _animationActive = false;
    private float _lerpPosition;
    private Vector3 _prevPosition;
    private Vector3 _nextPosition;

    protected float kAnimationsMaxLength = 0.25f;
    protected float kAnimationsInvMaxLength;

    public bool IsBoss;

    public bool IsDead => Health == 0;

    void Start()
    {
        kAnimationsInvMaxLength = 1.0f / kAnimationsMaxLength;

        transform.localScale = Vector3.one * 0.5f;
    }
    
    protected void SubmitMoveAction(Vector2Int direction)
    {
        var newPosition = CurrentPosition + direction;

        GameManager.Instance.CurrentLevel.RegisterAction(this, CurrentPosition, newPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>Returns true if the player died as a result</returns>
    public bool TakeDamage(int damage)
    {
        if (IsDead)
        {
            return false;
        }

        Health -= damage;
        Health = Math.Max(Health, 0);

        return IsDead;
    }

    public virtual void SubmitActions()
    {
    }

    public void PostTick()
    {
        if (IsDead)
        {
            GameManager.Instance.CurrentLevel.Kill(this);
        }
    }

    internal void Wait()
    {
    }

    internal void Move(Vector2Int newPos)
    {
        CurrentPosition = newPos;

        UpdateVisuals();
    }

    private void Update()
    {
        if (_animationActive == true)
        {
            Vector3 tmp = Vector3.Lerp(_prevPosition, _nextPosition, Easing.EaseInOutCubic(_lerpPosition));
            tmp.y = Mathf.Sin(_lerpPosition * Mathf.PI) * AnimationJumpHeight;

            transform.position = tmp;
            transform.localScale = Vector3.one * (0.5f + Mathf.Sin(Easing.EaseInOut(_lerpPosition) * Mathf.PI) * AnimationScaleFactor * 0.5f);

            _lerpPosition += Time.deltaTime * kAnimationsInvMaxLength;
            if (_lerpPosition > 1.0f)
            {
                _lerpPosition = 1.0f;
            }
        }
    }

    public void UpdateVisuals()
    {
        _animationActive = true;

        _prevPosition = transform.position;
        _nextPosition = GameManager.Instance.CurrentLevel.WorldParent.position + new Vector3(CurrentPosition.x, 0, CurrentPosition.y);
        _lerpPosition = 0.0f;
        //transform.position = GameManager.Instance.CurrentLevel.WorldParent.position + new Vector3(CurrentPosition.x, 0, CurrentPosition.y);


        if (CurrentDirection == Vector2Int.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (CurrentDirection == Vector2Int.down)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (CurrentDirection == Vector2Int.left)
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else if (CurrentDirection == Vector2Int.right)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    internal void TransferLevel(int levelIdx)
    {
        GameManager.Instance.MovePlayerToLevel(levelIdx, GameManager.Instance.CurrentLevel.Index < levelIdx);

        UpdateVisuals();
    }

    internal void Pickup(PickupObject obj)
    {
        obj.Consume();
    }

    internal void Attack()
    {
        if (LeftHand != null)
        {
            LeftHand.GetComponent<HandController>().Hit();
        }

        if (RightHand != null)
        {
            RightHand.GetComponent<HandController>().Hit();
        }
    }

    internal void Die()
    {
        if (IsBoss)
        {
            GameManager.Instance.Win();
        }
    }
}
