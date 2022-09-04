using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : LiquidType
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public override void AddVolume(float volume) {
        LiquidVolume += volume;
        float radius = CalculateRadius();
        transform.localScale = new Vector3(2f * radius, transform.lossyScale.y, 2f * radius);
        //Debug.Log("m_volume:" + m_volume + " radius:" + radius);
	}

    private float CalculateRadius() {
        // V = pi r^2 * h
        // r = sqrt(V / (pi * h))
        return Mathf.Sqrt(LiquidVolume * ML_TO_CUBICM/ (Mathf.PI * transform.lossyScale.y));
	}
}
