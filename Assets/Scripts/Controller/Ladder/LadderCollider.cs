using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Whenever player collides with collider assigned to ladder, set its status to be on ladder,
/// which disables gravity for player (therefore he can go up and down).
/// 
/// This is a simple Ladder implementation (similar to what original GoldSrc engine used)
/// </summary>
public class LadderCollider : MonoBehaviour
{
    /// <summary>
    /// Player controller
    /// </summary>
    public FpsController _player;

    /// <summary>
    /// Ladder collider
    /// </summary>
    private Collider _collider;

    /// <summary>
    /// Is player on ladder?
    /// 
    /// This variable is used to prevent enable/disable binding to ladder
    /// </summary>
    private bool _onLadder;

    /// <summary>
    /// Just get components, and set that player is not bound to ladder
    /// </summary>
    void Start()
    {
        _collider = GetComponent<Collider>();
        _onLadder = false;
    }

    /// <summary>
    /// During update, test whether player is inside ladder collider, if so - bind him to ladder,
    /// otherwise unbind him from ladder. This is a hacky solution, worthy of game jam.
    /// 
    /// This could've been improved with trigger collider F.e.
    /// </summary>
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
