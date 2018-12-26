using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private bool fp = false;
    	
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
