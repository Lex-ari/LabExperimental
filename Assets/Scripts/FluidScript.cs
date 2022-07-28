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
        UpdateFillRender(); // Render Fill into beaker
        UpdateOrigin(); // Update lowest position of beaker for pouring
        UpdatePour(); // Determine angle of pouring and enables
        if (isPouring) {
            UpdateStreamWidth(); // Adjusts to the angle of pouring / how much is being poured out.
            UpdateFillValue();

        }
        
    }

    // Used to "render" the fill, taking into consideration of offset of the mesh and rotation.
    // Not geometrically accurate, but suffices for testing.
    // Will remake in the future to account for cylindrical and cone shaped glassware
    private void UpdateFillRender() {
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
        if (zValue % 359.99 < 1E-3) { // used to prevent a 1/0 = infinity calculation, saving FPS.
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
        
        origin.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * xzOffset, heightOffset, -Mathf.Sin(angle * Mathf.Deg2Rad) * xzOffset);
	}

    // Determines if the angle of the cylinder is enough to pour out liquid.
    // If the origin (lip of beaker) is less than the fill, it spills.
    private bool CalculatePourEnabled() {
        //Debug.Log("origin - transform:" + (origin.transform.position.y - transform.position.y) + " liquidRenderer" + (m_purpleLiquidRenderer.material.GetFloat("_Fill") * transform.lossyScale.y));
        return origin.transform.position.y - transform.position.y < m_purpleLiquidRenderer.material.GetFloat("_Fill") * transform.lossyScale.y;
	}

    private Stream CreateStream() {
        GameObject streamObject = Instantiate(streamPrefab, origin.position, Quaternion.identity, origin.transform);
        return streamObject.GetComponent<Stream>();
	}

    private void UpdateStreamWidth() {
        float radius = GetLossyRadius();
        float centerToFluidAngled = GetDistanceFromCenterToFluid();
        float dotProduct = Vector3.Dot(transform.up, Vector3.up);
        float chord = 2 * radius;
        if (dotProduct > 0) {
            chord = 2 * Mathf.Sqrt((radius * radius) - (centerToFluidAngled * centerToFluidAngled));
		}
        if (currentStream != null) {
            currentStream.SetWidthMultiplier(chord, radius);
		}
        Debug.Log("chord: " + chord + " centerToFluidAngled:" + centerToFluidAngled);
    }

    private float GetDistanceFromCenterToFluid() {
        float dotProduct = Vector3.Dot(transform.up, Vector3.up);
        float angle = Mathf.Acos(dotProduct) + 10f * Mathf.Deg2Rad;
        float centerHeight = 0.5f * dotProduct * m_purpleLiquidRenderer.bounds.size.y;
        float fluidHeight = centerHeight - m_purpleLiquidRenderer.material.GetFloat("_Fill") * transform.lossyScale.y;
        //float fluidHeight = centerHeight - (m_purpleLiquidRenderer.material.GetFloat("_Fill") / (1 + -cylindricalFixVariable * 0.5f * (Mathf.Cos(dotProduct * Mathf.PI) + 1)) - dotProduct * centerOffset) * transform.lossyScale.y;
        float radiusToFluidAngled = fluidHeight / Mathf.Sin(angle);
        if (radiusToFluidAngled < 0) {
            return 0;
		}
        //Debug.Log("dotProduct: " + dotProduct + " angle:" + angle * Mathf.Rad2Deg + " fluidHeight:" + fluidHeight + " centerHeight:" + centerHeight + " radiusToFluidAngled:" + radiusToFluidAngled + " chord:" + chord);
        return radiusToFluidAngled;
    }

    private float GetLossyRadius() {
        return Mathf.Abs(xzOffset) * transform.lossyScale.x;
	}

    // Calculates the amount of fluid "spilled" when tilted.
    // Uses Bernoulli's Principle
    private void UpdateFillValue() {
        float radius = GetLossyRadius();
        float centerToFluidDistance = GetDistanceFromCenterToFluid();
        float volume = radius * radius * Mathf.PI * GetComponent<Renderer>().bounds.size.y; // Meters ^3
        float spillHeight = (m_purpleLiquidRenderer.material.GetFloat("_Fill") * transform.lossyScale.y) - (origin.transform.position.y - transform.position.y);
        // Bernoulli's Equation: Pressure1 + 0.5*fluidDensity*velocity^2 + fluidDensity*gravity*height1 = Pressure2 + 0.5*fluidDensity*volume^2 + pressureDensity*gravity*height2;
        // Pressure 1 = pressure 2, 
        // gravity*height1 = 0.5*velocity^2
        float velocity = Mathf.Sqrt(9.8f * spillHeight / 0.5f);
        float theta = 0;
        if (centerToFluidDistance < radius) {
            theta = 2f * Mathf.Acos(centerToFluidDistance / radius);
		}
        float segmentArea = 0.5f * radius * radius * (theta - Mathf.Sin(theta));
        float rateVolumeDepleted = segmentArea * velocity; // Meters ^ 3 / s
        float percentageVolumeDepleted = rateVolumeDepleted / volume; // Percentage removed / s
        Fill -= percentageVolumeDepleted * Time.deltaTime;
    }


}
