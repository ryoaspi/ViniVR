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
        _bonus.SetActive(true);
    }


    [SerializeField] private GameObject _bonus;
}
