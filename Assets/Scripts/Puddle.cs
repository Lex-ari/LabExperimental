using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : LiquidType
{
    public override void AddVolume(float volume) {
        liquidVolume += volume;
        float radius = CalculateRadius();
        transform.localScale = new Vector3(2f * radius, transform.lossyScale.y, 2f * radius);
        //Debug.Log("m_volume:" + m_volume + " radius:" + radius);
	}

    private float CalculateRadius() {
        // V = pi r^2 * h
        // r = sqrt(V / (pi * h))
        return Mathf.Sqrt(liquidVolume * ML_TO_CUBICM/ (Mathf.PI * transform.lossyScale.y));
	}
}
