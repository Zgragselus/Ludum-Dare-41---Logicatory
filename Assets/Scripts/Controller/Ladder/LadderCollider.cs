using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderCollider : MonoBehaviour
{
    public FpsController _player;

    private Collider _collider;
    private bool _onLadder;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _onLadder = false;
    }

    void Update()
    {
        if (_collider.bounds.Contains(_player.transform.position))
        {
            if (_onLadder == false)
            {
                _onLadder = true;
                _player.SetLadder(true);
            }
        }
        else
        {
            if (_onLadder == true)
            {
                _onLadder = false;
                _player.SetLadder(false);
            }
        }
    }
}
