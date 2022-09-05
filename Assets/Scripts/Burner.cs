using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : MonoBehaviour
{

    public bool isOn = true;
    private Object targetLiquidType;
    private ParticleSystem flame = null;

	private void Awake() {
        flame = GetComponent<ParticleSystem>();
	}

	public void Begin() {
        StartCoroutine(DetermineHit());
        StartCoroutine(HeatHit());
	}

    public void End() {
        StopCoroutine(DetermineHit());
        StopCoroutine(HeatHit());
	}

	private IEnumerator DetermineHit() {
        while (gameObject.activeSelf) {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.up);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.CompareTag("Liquid")) {
                    targetLiquidType = hit.collider.GetComponent<LiquidType>();
                } else {
                    targetLiquidType = null;
				}
			}
            yield return null;
		}
	}

    private IEnumerator HeatHit() {
        while (targetLiquidType != null) {



            yield return null;
		}
	}
}
