using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidScript : MonoBehaviour
{

    private Renderer m_purpleLiquidRenderer;
    public float Fill;  
    public float centerOffset;
    public float cylindricalFixVariable;

    public float pourThreshold = 45;
    public GameObject streamPrefab;
    public Transform origin;
    private bool isPouring = false;
    private Stream currentStream = null;

    // Start is called before the first frame update
    void Start()
    {
        m_purpleLiquidRenderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFill();
        UpdatePour();
    }

    private void UpdateFill() {
        Vector3 upvector = gameObject.transform.up ;
        float dot = Vector3.Dot(upvector, Vector3.up);
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

    public void SetFill(float fill) {
        
	}

    private void UpdatePour() {
        bool pourCheck = CalculatePourAngle() > pourThreshold;
        if (isPouring != pourCheck) {
            isPouring = pourCheck;
            if (isPouring) {
                currentStream = CreateStream();
                currentStream.Begin();
			} else {
                currentStream.End();
                currentStream = null;
			}
		}
	}


    private float CalculatePourAngle() {
        return (1 - Vector3.Dot(transform.up, Vector3.up)) * 180;
	}

    private Stream CreateStream() {
        GameObject streamObject = Instantiate(streamPrefab, origin.position, Quaternion.identity, transform);
        return streamObject.GetComponent<Stream>();
	}
}
