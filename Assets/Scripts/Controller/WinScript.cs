using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScript : MonoBehaviour
{
    public GameObject _player;
    public bool _win = false;
    public float _cooldown = 5.0f;

    void Start ()
    {
		
	}

    void Update()
    {
        if (GetComponent<Collider>().bounds.Contains(_player.transform.position))
        {
            _win = true;
        }

        if (_win)
        {
            _cooldown -= Time.deltaTime;
        }

        if (_cooldown < 0.0f)
        {
            Application.Quit();
        }
    }
}
