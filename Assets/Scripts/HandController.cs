using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public float Delay = 0.0f;

    private float _timer = -1.0f;

    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timer > 0.0f && _timer - Time.deltaTime < 0.0f)
        {
            _animator.Play("Hit", -1);
            _timer = -1.0f;
        }

        _timer -= Time.deltaTime;
        if (_timer < -1.0f)
        {
            _timer = -1.0f;
        }
    }

    public void Hit()
    {
        _timer = Delay;
    }
}
