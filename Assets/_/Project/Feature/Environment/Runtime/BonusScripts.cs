using System;
using UnityEngine;

public class BonusScripts : MonoBehaviour
{
    private void Awake()
    {
        if (_bonus == null) return;
        _bonus.SetActive(false);
    }

    public void BonusEnable()
    {
        if (_firstCall = false)
        {
            _bonus.SetActive(true);
            _firstCall = true;
        }
    }


    [SerializeField] private GameObject _bonus;
    private bool _firstCall;
}
