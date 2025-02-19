using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableDisableEvents : MonoBehaviour
{
    public UnityEvent On;
    public UnityEvent Off;

    void OnEnable()
    {
        On?.Invoke();
    }

    void OnDisable()
    {
        Off?.Invoke();
    }
}
