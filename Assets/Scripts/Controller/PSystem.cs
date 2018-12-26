using UnityEngine;
using System.Collections;

public class PSystem : MonoBehaviour {

    private ParticleSystem ps;
    
    void Start ()
    {
        ps = GetComponent<ParticleSystem>();
	}
	
	void Update ()
    {
	    if (!ps.IsAlive())
        {
            Destroy(gameObject);
        }
	}
}
