using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryDoorController : MonoBehaviour {

    public bool _isOpened;
    public bool _direction;
    public float _pickRange = 2.5f;
    public float _openingSpeed = 1.0f;
    public float _openingOffset = 3.0f;
    public GameObject _door;

    private float _position = 0.0f;
    private float _targetPosition = 0.0f;
    private bool ep = false;
    private Vector3 _baseTransform;

    void Start()
    {
        _baseTransform = _door.transform.position;
    }

	void FixedUpdate ()
    {
        if (Input.GetKey(KeyCode.E) && !ep)
        {
            ep = true;

            RaycastHit hit;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out hit))
            {
                if (hit.distance < _pickRange)
                {
                    GameObject g = hit.collider.gameObject;

                    while (g != null)
                    {
                        if (g == gameObject)
                        {
                            _isOpened = !_isOpened;

                            if (_isOpened)
                            {
                                _targetPosition = _openingOffset;
                            }
                            else
                            {
                                _targetPosition = 0.0f;
                            }

                            break;
                        }

                        if (g.transform.parent)
                        {
                            g = g.transform.parent.gameObject;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.E) == false)
        {
            ep = false;
        }

        if (_position < _targetPosition)
        {
            _position += _openingSpeed * Time.fixedDeltaTime;

            if (_position >= _targetPosition)
            {
                _position = _targetPosition;
            }
            else
            {
                _door.transform.position = _baseTransform + new Vector3(0.0f, _position, 0.0f);
            }
        }
        else if (_position > _targetPosition)
        {
            _position -= _openingSpeed * Time.fixedDeltaTime;

            if (_position <= _targetPosition)
            {
                _position = _targetPosition;
            }
            else
            {
                _door.transform.position = _baseTransform + new Vector3(0.0f, _position, 0.0f);
            }
        }
    }
}
