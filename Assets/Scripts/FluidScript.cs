using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidScript : MonoBehaviour
{

    private Renderer m_purpleLiquidRenderer;
    public float Fill;  
    public float centerOffset;
    public float dot;
    public float cylindricalFixVariable;

    // Start is called before the first frame update
    void Start()
    {
        m_purpleLiquidRenderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFill();
    }

    private void UpdateFill() {
        Vector3 upvector = gameObject.transform.up ;
        dot = Vector3.Dot(upvector, Vector3.up);
        float cylindricalFix = 1 + -cylindricalFixVariable * 0.5f * (Mathf.Cos(dot * Mathf.PI) + 1);
        float fillValue;
        if (Fill < 1E-9) {
            fillValue = -999;
		} else if (Fill - 1 > 1E-9){
            fillValue = 999;
		} else {
            fillValue = ((Fill * 2) - 1) * cylindricalFix + dot * centerOffset;
		}
        m_purpleLiquidRenderer.material.SetFloat("_Fill", fillValue);
	}

    private void SetFill(float fill) {
        
	}
}
