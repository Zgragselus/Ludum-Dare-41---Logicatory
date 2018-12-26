using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public DecalManager _decalManager;

    public float _impactForce;
    public float _recoilTime = 0.1f;
    public float _bulletSpeed = 0.0f;

    private float _nextFire;

    public GameObject _psystem;

    void Start ()
    {
        _nextFire = 0.0f;
    }
	
	void FixedUpdate ()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > _nextFire)
        {
            RaycastHit hit;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                Debug.Log(hitObject.name);

                if (hitObject.GetComponent<ForceFieldController>())
                {
                    hitObject.GetComponent<ForceFieldController>().Disable();
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

                    if (tmp.GetComponent<TurretCollider>())
                    {
                        tmp.GetComponent<TurretCollider>().Damage(20.0f);
                    }
                }

                //_decalManager.SpawnDecal(hit);
                GameObject obj = Instantiate(_psystem);
                obj.transform.position = hit.point;
            }

            _nextFire = Time.time + _recoilTime;
        }
    }
}
