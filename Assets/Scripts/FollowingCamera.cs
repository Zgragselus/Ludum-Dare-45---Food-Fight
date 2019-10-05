using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform Target;

    void Update()
    {
        transform.position = Target.position + new Vector3(0, 10, -4);
    }
}
