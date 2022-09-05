using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beaker : ContainerType {

	// Used to "render" the fill, taking into consideration of offset of the mesh and rotation.
	// Not geometrically accurate, but suffices for testing.
	// Will remake in the future to account for cylindrical and cone shaped glassware
	protected override void UpdateFillRender() {
		Vector3 upvector = gameObject.transform.up;
		float dot = Vector3.Dot(upvector, Vector3.up);
		float cylindricalFix = 1 + -cylindricalFixVariable * 0.5f * (Mathf.Cos(dot * Mathf.PI) + 1);
		float fillValue;
		float Fill = liquidVolume / containerVolume;
		if (Fill < 1E-9) {
			fillValue = -999;
		} else if (Fill - 1 > 1E-9) {
			fillValue = 999;
		} else {
			fillValue = ((Fill * 2) - 1) * cylindricalFix + dot * centerOffset;
		}
		m_purpleLiquidRenderer.material.SetFloat("_Fill", fillValue);
		float valueForCollider = ((Fill * 2) - 1) * cylindricalFix + dot * centerOffset;
		liquidSurfaceCollider.center = new Vector3(liquidSurfaceCollider.center.x, valueForCollider, liquidSurfaceCollider.center.z);
	}

	protected override float GetLossyRadius() {
		return Mathf.Abs(xzOffset) * transform.lossyScale.x;
	}

}
