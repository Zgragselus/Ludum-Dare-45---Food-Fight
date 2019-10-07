using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform Target;

    public float Factor = 2.0f;
    public float Distance = 0.75f;

    private Vector3 _diffPosition;
    private Vector3 _nextPosition;
    
    void Start()
    {
        transform.position = new Vector3(0.0f, 10.0f, -4.0f) * Distance;
    }

    void Update()
    {
        _nextPosition = Target.position + new Vector3(0, 10, -4) * Distance;
        _nextPosition.y = 7.0f * Distance;
        _diffPosition = _nextPosition - transform.position;

        if (_diffPosition.magnitude > 10.0f)
        {
            transform.position = transform.position + _diffPosition;
        }
        else
        {
            transform.position = transform.position + _diffPosition * Time.deltaTime * Factor;
        }
    }
}
