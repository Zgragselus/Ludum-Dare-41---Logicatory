using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flashlight controls
/// </summary>
public class Flashlight : MonoBehaviour
{
    /// <summary>
    /// Prevents multiple invocations of enabling-disabling flashlight
    /// </summary>
    private bool fp = false;
    
    /// <summary>
    /// Basically just enable/disable light component whenever 'F' key is pressed
    /// </summary>
	void Update ()
    {
        if (Input.GetKey(KeyCode.F) && !fp)
        {
            fp = true;

            GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
        }

        if (Input.GetKey(KeyCode.F) == false)
        {
            fp = false;
        }
    }
}
