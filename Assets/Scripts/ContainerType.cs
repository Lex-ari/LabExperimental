using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContainerType : LiquidType
{
	protected Renderer m_purpleLiquidRenderer;
	public float centerOffset;
	public float cylindricalFixVariable;

	public GameObject streamPrefab;
	public Transform origin;
	public GameObject liquidGameObject;
	protected bool isPouring = false;
	protected Stream currentStream = null;
	protected BoxCollider liquidSurfaceCollider = null;

	protected float xzOffset;
	protected float heightOffset;
	// Start is called before the first frame update

	//Debugging
	public bool streamEnabled = false;

	void Start() {
		m_purpleLiquidRenderer = liquidGameObject.GetComponent<Renderer>();
		containerVolume = Mathf.PI * Mathf.Pow(m_purpleLiquidRenderer.bounds.size.z / 2, 2) * m_purpleLiquidRenderer.bounds.size.y * CUBICM_TO_ML; // PI * r^2  *h 
		xzOffset = origin.transform.localPosition.x;
		heightOffset = origin.transform.localPosition.y;
		liquidSurfaceCollider = liquidGameObject.GetComponent<BoxCollider>();
		StartCoroutine(UpdateOrigin());
		StartCoroutine(UpdateStreamWidth()); // Adjusts to the angle of pouring / how much is being poured out.
	}

	// Update is called once per frame
	void Update() {
		UpdateFillRender(); // Render Fill into beaker
		UpdatePour(); // Determine angle of pouring and enables
		if (isPouring) {
			float removedVolume = UpdateFillValue();
			AddVolumeToStream(removedVolume);
		}
	}

	// Used to "render" the fill, taking into consideration of offset of the mesh and rotation.
	// Not geometrically accurate, but suffices for testing.
	// Will remake in the future to account for cylindrical and cone shaped glassware
	protected virtual void UpdateFillRender() {

	}

	// Checks if the object is pouring.
	// If pouring, starts. If not pouring anymore, stops.
	protected void UpdatePour() {
		bool pourCheck = CalculatePourEnabled();
		if (isPouring != pourCheck) {
			isPouring = pourCheck;
			liquidSurfaceCollider.enabled = !isPouring;
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
	protected IEnumerator UpdateOrigin() {
		while (gameObject.activeSelf) {
			float xValue = liquidGameObject.transform.eulerAngles.x; // z direction
			float zValue = liquidGameObject.transform.eulerAngles.z; // x direction
			float angle;
			if (zValue % 359.99 < 1E-3) { // used to prevent a 1/0 = infinity calculation, saving FPS.
				if (xValue < 180) {
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
			//Debug.Log("xvalue:" + xValue + " zValue:" + zValue + " angle:" + angle + " local position:" + origin.transform.localPosition);
			yield return null;
		}

	}

	// Determines if the angle of the cylinder is enough to pour out liquid.
	// If the origin (lip of beaker) is less than the fill, it spills.
	protected bool CalculatePourEnabled() {
		//Debug.Log("origin - transform:" + (origin.transform.position.y - transform.position.y) + " liquidRenderer" + (m_purpleLiquidRenderer.material.GetFloat("_Fill") * transform.lossyScale.y));
		return origin.transform.position.y - liquidGameObject.transform.position.y < m_purpleLiquidRenderer.material.GetFloat("_Fill") * liquidGameObject.transform.lossyScale.y;
	}

	protected Stream CreateStream() {
		GameObject streamObject = Instantiate(streamPrefab, origin.position, Quaternion.identity, origin.transform);
		return streamObject.GetComponent<Stream>();
	}

	protected IEnumerator UpdateStreamWidth() {
		while (gameObject.activeSelf) {
			float radius = GetLossyRadius();
			float centerToFluidAngled = GetDistanceFromCenterToFluid();
			float dotProduct = Vector3.Dot(transform.up, Vector3.up);
			float chord = 2 * radius;
			if (true) {
				float cSquaredMinusASquared = (radius * radius) - (centerToFluidAngled * centerToFluidAngled);
				if (cSquaredMinusASquared < 0) {
					cSquaredMinusASquared = 0;
				}
				chord = 2 * Mathf.Sqrt(cSquaredMinusASquared);
			}
			if (currentStream != null) {
				currentStream.SetWidthMultiplier(chord, radius);
			}
			//Debug.Log("chord: " + chord + " centerToFluidAngled:" + centerToFluidAngled);
			yield return null;
		}

	}

	protected float GetDistanceFromCenterToFluid() {
		float dotProduct = Vector3.Dot(liquidGameObject.transform.up, Vector3.up);
		float angle = Mathf.Acos(dotProduct) + 10f * Mathf.Deg2Rad;
		float centerHeight = 0.5f * dotProduct * m_purpleLiquidRenderer.bounds.size.y;
		float fluidHeight = centerHeight - m_purpleLiquidRenderer.material.GetFloat("_Fill") * liquidGameObject.transform.lossyScale.y;
		//float fluidHeight = centerHeight - (m_purpleLiquidRenderer.material.GetFloat("_Fill") / (1 + -cylindricalFixVariable * 0.5f * (Mathf.Cos(dotProduct * Mathf.PI) + 1)) - dotProduct * centerOffset) * transform.lossyScale.y;
		float radiusToFluidAngled = fluidHeight / Mathf.Sin(angle);
		if (radiusToFluidAngled < 0) {
			return 0;
		}
		//Debug.Log("dotProduct: " + dotProduct + " angle:" + angle * Mathf.Rad2Deg + " fluidHeight:" + fluidHeight + " centerHeight:" + centerHeight + " radiusToFluidAngled:" + radiusToFluidAngled + " chord:" + chord);
		return radiusToFluidAngled;
	}

	protected virtual float GetLossyRadius() {
		return Mathf.Abs(xzOffset) * liquidGameObject.transform.lossyScale.z;
	}

	// Calculates the amount of fluid "spilled" when tilted.
	// Uses Bernoulli's Principle
	// Returns the amount of volume removed in mililiters.
	protected float UpdateFillValue() {
		float radius = GetLossyRadius();
		float centerToFluidDistance = GetDistanceFromCenterToFluid();
		float spillHeight = (m_purpleLiquidRenderer.material.GetFloat("_Fill") * liquidGameObject.transform.lossyScale.y) - (origin.transform.position.y - liquidGameObject.transform.position.y);
		// Bernoulli's Equation: Pressure1 + 0.5*fluidDensity*velocity^2 + fluidDensity*gravity*height1 = Pressure2 + 0.5*fluidDensity*volume^2 + pressureDensity*gravity*height2;
		// Pressure 1 = pressure 2, 
		// gravity*height1 = 0.5*velocity^2
		if (spillHeight < 0) {
			spillHeight = 0;
		}
		float velocity = Mathf.Sqrt(9.8f * spillHeight / 0.5f);
		float theta = 0;
		if (centerToFluidDistance < radius) {
			theta = 2f * Mathf.Acos(centerToFluidDistance / radius);
		}
		float segmentArea = 0.5f * radius * radius * (theta - Mathf.Sin(theta));
		float rateVolumeDepleted = segmentArea * velocity; // Meters ^ 3 / s
		float miliLitersVolumeDepleted = rateVolumeDepleted * Time.deltaTime * CUBICM_TO_ML;

		if (liquidVolume < miliLitersVolumeDepleted) {
			miliLitersVolumeDepleted = liquidVolume;
		}
		liquidVolume -= miliLitersVolumeDepleted;
		return miliLitersVolumeDepleted; // In Mililiters
	}
	protected void AddVolumeToStream(float volume) {
		currentStream.AddVolume(volume);
	}
}
