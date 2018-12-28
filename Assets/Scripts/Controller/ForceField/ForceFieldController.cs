using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to disable ForceField, and drop down current game object for effect.
/// </summary>
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
