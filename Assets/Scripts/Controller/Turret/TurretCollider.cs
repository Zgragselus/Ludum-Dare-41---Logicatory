using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretCollider : MonoBehaviour
{
    public GameObject _psystem;
    public GameObject _psystemdie;

    public GameObject _cannon;
    public GameObject _trigger;
    public GameObject _player;
    public float _rotationSpeed;
    public float _wakeupTime;

    public float _recoilTime = 0.1f;
    private float _nextFire = 0.0f;
    public float _impactForce = 250.0f;
    public DecalManager _decalManager;
    private int _fireCount = 0;

    private float _timer;
    private bool _target = false;

    private float _health = 100.0f;
    private bool _alive = true;
    private bool _dieanim = false;

    // Use this for initialization
    void Start () {
		
	}

    public void Damage(float hp)
    {
        _health -= hp;
        if (_health <= 0.0f)
        {
            _alive = false;

            if (!_dieanim)
            {
                _dieanim = true;

                GameObject obj = Instantiate(_psystemdie);
                obj.transform.position = _cannon.transform.position;

            }
        }
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (_alive)
        {
            if (_trigger.GetComponent<Collider>().bounds.Contains(_player.transform.position))
            {
                if (_target == false)
                {
                    _target = true;
                    _timer = _wakeupTime;
                }

                if (_wakeupTime > 0.0f)
                {
                    _wakeupTime -= Time.fixedDeltaTime;
                }
                else
                {
                    Vector3 directionTarget = _player.transform.position - transform.position;
                    directionTarget.y = 0.0f;
                    directionTarget.Normalize();

                    float angleTarget = Mathf.Atan2(directionTarget.z, directionTarget.x) * Mathf.Rad2Deg;

                    Vector3 directionTurret = _cannon.transform.forward;
                    directionTurret.y = 0.0f;
                    directionTurret.Normalize();

                    float angleTurret = Mathf.Atan2(directionTurret.z, directionTurret.x) * Mathf.Rad2Deg;

                    if (Mathf.Abs(angleTurret - angleTarget) > 0.01f)
                    {
                        Vector3 crossDir = Vector3.Cross(directionTarget, directionTurret);

                        if (Vector3.Dot(crossDir, new Vector3(0.0f, 1.0f, 0.0f)) > 0.0f)
                        {
                            _cannon.transform.Rotate(new Vector3(0.0f, 1.0f * _rotationSpeed * Time.fixedDeltaTime, 0.0f));
                        }
                        else
                        {
                            _cannon.transform.Rotate(new Vector3(0.0f, 1.0f * -_rotationSpeed * Time.fixedDeltaTime, 0.0f));
                        }
                    }

                    if (Mathf.Abs(angleTurret - angleTarget) > 1.0f)
                    {
                        if (Time.time > _nextFire)
                        {
                            RaycastHit hit;
                            float distance = Vector3.Distance(_player.transform.position, _cannon.transform.position);
                            Ray r = new Ray(_cannon.transform.position, _player.transform.position + new Vector3(Random.value, Random.value, Random.value) * (_fireCount + 1) * 0.25f * distance * 0.1f - _cannon.transform.position);
                            if (Physics.Raycast(r, out hit))
                            {
                                GameObject hitObject = hit.collider.gameObject;

                                if (hitObject.GetComponent<ForceFieldController>())
                                {
                                    hitObject.GetComponent<ForceFieldController>().Disable();
                                }

                                if (hitObject && hitObject == _player.gameObject)
                                {
                                    _player.GetComponent<FpsController>().Damage(Random.value * 5.0f + 5.0f);
                                }

                                if (hitObject && hitObject.transform.parent && hitObject.transform.parent.gameObject == _player.gameObject)
                                {
                                    _player.GetComponent<FpsController>().Damage(Random.value * 5.0f + 5.0f);
                                }

                                GameObject tmp = hitObject;
                                while (tmp && tmp.GetComponent<Rigidbody>() == null)
                                {
                                    if (tmp.transform.parent)
                                    {
                                        tmp = tmp.transform.parent.gameObject;
                                    }
                                    else
                                    {
                                        tmp = null;
                                    }
                                }

                                if (tmp)
                                {
                                    tmp.GetComponent<Rigidbody>().AddForceAtPosition(r.direction * _impactForce, hit.point);
                                }

                                //_decalManager.SpawnDecal(hit);
                                GameObject obj = Instantiate(_psystem);
                                obj.transform.position = hit.point;
                            }

                            _fireCount++;
                            if (_fireCount == 5)
                            {
                                _fireCount = 0;
                                _nextFire = Time.time + _recoilTime * 5.0f;
                            }
                            else
                            {
                                _nextFire = Time.time + _recoilTime;
                            }
                        }
                    }
                }
            }
            else
            {
                _target = false;
            }
        }
	}
}
