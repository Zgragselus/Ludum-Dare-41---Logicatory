using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPicker : MonoBehaviour
{
    private bool isHoldingBody = false;
    private GameObject _heldBody;
    private bool ep = false;

    public float _correctionForce = 100.0f;
    public float _stabilizationFactor = 0.5f;
    public float _pointDistance = 1.0f;
    public float _pickRange = 2.5f;
    public float _dropRange = 3.0f;
    public float _throwForce = 250.0f;

    void Start()
    {

    }

    void FixedUpdate()
    {
        PhysicsPickUp();

        if (isHoldingBody)
        {
            Rigidbody rb = _heldBody.GetComponent<Rigidbody>();

            if (rb.constraints != RigidbodyConstraints.FreezeRotation)
                rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (rb.useGravity)
                rb.useGravity = false;

            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            targetPoint += Camera.main.transform.forward * _pointDistance;
            Vector3 force = targetPoint - _heldBody.transform.position;

            rb.velocity = force.normalized * rb.velocity.magnitude;
            rb.AddForce(force * _correctionForce);

            rb.velocity *= Mathf.Min(1.0f, force.magnitude / 2);

            if (Vector3.Distance(_heldBody.transform.position, Camera.main.transform.position) > _dropRange)
            {
                PhysicThrow(0.0f);
            }
        }
    }

    public void PhysicsPickUp()
    {
        if (Input.GetKey(KeyCode.E) && !isHoldingBody && !ep)
        {
            ep = true;

            RaycastHit hit;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out hit))
            {
                if (hit.distance < _pickRange)
                {
                    GameObject g = hit.collider.gameObject;
                    Rigidbody rb = g.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        _heldBody = hit.collider.gameObject;
                        isHoldingBody = true;
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.E) && isHoldingBody && !ep)
        {
            ep = true;
            PhysicThrow(0.0f);
        }

        if (Input.GetKey(KeyCode.E) == false)
        {
            ep = false;
        }

        if (Input.GetMouseButton(0) && isHoldingBody)
        {
            PhysicThrow(_throwForce);
        }
    }

    private void PhysicThrow(float force = 0)
    {
        if (isHoldingBody)
        {
            Rigidbody rb = _heldBody.GetComponent<Rigidbody>();

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.AddForce(Camera.main.transform.forward * force);

            _heldBody = null;
            isHoldingBody = false;
        }
    }
}
