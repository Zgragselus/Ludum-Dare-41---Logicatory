using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool _isOpened;
    public bool _direction;
    public float _pickRange = 2.5f;
    public float _openingSpeed = 90.0f;

    public float _angle = 0.0f;
    public float _targetAngle = 0.0f;
    private bool ep = false;

    void FixedUpdate()
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
                                _targetAngle = 90.0f * (_direction ? 1.0f : -1.0f);
                            }
                            else
                            {
                                _targetAngle = 0.0f;
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

        if (_angle < _targetAngle)
        {
            _angle += _openingSpeed * Time.fixedDeltaTime;

            if (_angle >= _targetAngle)
            {
                _angle = _targetAngle;
            }
            else
            {
                transform.Rotate(new Vector3(0.0f, _openingSpeed * Time.fixedDeltaTime, 0.0f));
            }
        }
        else if (_angle > _targetAngle)
        {
            _angle -= _openingSpeed * Time.fixedDeltaTime;

            if (_angle <= _targetAngle)
            {
                _angle = _targetAngle;
            }
            else
            {
                transform.Rotate(new Vector3(0.0f, -_openingSpeed * Time.fixedDeltaTime, 0.0f));
            }
        }
    }
}
