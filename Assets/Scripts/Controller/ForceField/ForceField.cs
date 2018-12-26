using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    private float mPhase;
    private Material mMaterial;
    public GameObject mLight0;
    public GameObject mLight1;
    public GameObject mLight2;
    public GameObject mLight3;
    private float _alpha;
    private bool _enabled = true;

    private void Start()
    {
        mMaterial = GetComponent<Renderer>().material;
        mPhase = 0.0f;
        mLight0.GetComponent<Light>().color = new Color(0.0f, 1.0f, 1.0f);
        mLight1.GetComponent<Light>().color = new Color(0.0f, 1.0f, 1.0f);
        mLight2.GetComponent<Light>().color = new Color(0.0f, 1.0f, 1.0f);
        mLight3.GetComponent<Light>().color = new Color(0.0f, 1.0f, 1.0f);
        _alpha = 1.0f;
    }

    public void Update()
    {
        if (_enabled == false)
        {
            _alpha -= 1.0f * Time.deltaTime;

            if (_alpha <= 0.0f)
            {
                _alpha = 0.0f;
            }
        }

        mLight0.GetComponent<Light>().intensity = _alpha;
        mLight1.GetComponent<Light>().intensity = _alpha;
        mLight2.GetComponent<Light>().intensity = _alpha;
        mLight3.GetComponent<Light>().intensity = _alpha;

        mMaterial.SetColor("_Color", new Color(0.0f, 1.0f, 1.0f, _alpha));
        mMaterial.SetFloat("_Offset", mPhase);
        mPhase += Time.deltaTime;
    }

    public void Disable()
    {
        _enabled = false;
    }
}