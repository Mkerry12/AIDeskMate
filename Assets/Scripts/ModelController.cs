using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();



    }

    void Update()
    {
        WhetherThinking();
    }

    void WhetherThinking()
    {
            _animator.SetBool("ISThinking", ChatBox.ISThinking6);
    }
}
