using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldController : MonoBehaviour
{
    public GameObject _forceField;
    public GameObject _forceFieldDrop;

    public void Disable()
    {
        _forceField.GetComponent<Collider>().enabled = false;
        _forceField.GetComponent<ForceField>().Disable();
        _forceFieldDrop.GetComponent<Rigidbody>().isKinematic = false;
    }
}
