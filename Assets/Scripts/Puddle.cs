using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : LiquidType
{

    private float m_volume;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddVolume(float volume) {
        m_volume += volume;
        float radius = CalculateRadius();
        transform.localScale.Set(2 * radius, transform.localScale.y, 2 * radius);
	}

    private float CalculateRadius() {
        return Mathf.Sqrt(m_volume / Mathf.PI * transform.localScale.y);
	}
}
