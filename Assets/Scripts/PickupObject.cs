using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public int HealthAdd;
    public int DamageAdd;

    private Animation _anim;

    private void Awake()
    {
        _anim = GetComponent<Animation>();
    }

    public void Consume()
    {
        _anim.Play("PowerupPickedup");
        GameObject.Destroy(gameObject, 3f);
    }
}
