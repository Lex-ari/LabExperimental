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

    private float xzOffset;
    private float heightOffset;

    // Start is called before the first frame update
    void Start()
    {
        m_purpleLiquidRenderer = gameObject.GetComponent<Renderer>();
        xzOffset = origin.transform.localPosition.x;
        heightOffset = origin.transform.localPosition.y;
}

    // Update is called once per frame
    void Update()
    {
        UpdateFill();
        UpdateOrigin();
        UpdatePour();
    }

    // Used to "render" the fill, taking into consideration of offset of the mesh and rotation.
    // Not geometrically accurate, but suffices for testing.
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

    // Allows to set the fill of the beaker from 0 (empty) to 1 (full)
    public void SetFill(float fill) {
        
	}

    // Checks if the object is pouring.
    // If pouring, starts. If not pouring anymore, stops.
    private void UpdatePour() {
        bool pourCheck = CalculatePourEnabled();
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

    // Calculates the "lowest point" of the top of the cylinder and sets the origin to that point.
    // Simulates pouring at a weird angle.
    private void UpdateOrigin() {
        float xValue = transform.eulerAngles.x; // z direction
        float zValue = transform.eulerAngles.z; // x direction
        float angle;
        if (zValue % 359.99 < 1E-3) {
            if (xValue > 0) {
                angle = 90;
			} else {
                angle = -90;
			}
		} else {
            angle = Mathf.Atan(Mathf.Sin(xValue * Mathf.Deg2Rad) / Mathf.Sin(zValue * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
            if (zValue > 180) {
                angle += 180;
			}
        }
        
        Debug.Log("xValue: " + xValue + " zValue: " + zValue + " angle: " + angle);
        origin.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * xzOffset, heightOffset, -Mathf.Sin(angle * Mathf.Deg2Rad) * xzOffset);
        //origin.transform.localPosition = new Vector3(Mathf.Cos(zValue * Mathf.Deg2Rad), heightOffset, 0);
	}

    // Determines if the angle of the cylinder is enough to pour out liquid.
    // Uses the fill calculations to determinte output.
    private bool CalculatePourEnabled() {
        return origin.transform.position.y < transform.position.y;
	}

    private Stream CreateStream() {
        GameObject streamObject = Instantiate(streamPrefab, origin.position, Quaternion.identity, origin.transform);
        return streamObject.GetComponent<Stream>();
	}
}
