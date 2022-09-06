using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : MonoBehaviour
{
    protected const float CUBICM_TO_ML = 1E6f;
    protected const float ML_TO_CUBICM = 1E-6f;
    private const float deltaHBurningReaction = 802.15f; // kJ/mol
    public bool isOn = true;
    public float flowRateVolume = 0.11f;  // Cubic M per hour
    public float flowRateBTU = 4400f; // BTU/Hour
    public LiquidType targetLiquidType;
    private ParticleSystem flame = null;

	private void Awake() {
        flame = GetComponent<ParticleSystem>();
        Begin();
	}

	public void Begin() {
        StartCoroutine(DetermineHit());
	}

    public void End() {
        StopCoroutine(DetermineHit());
	}

	public void Update() {
        HeatHit();

    }

	private IEnumerator DetermineHit() {
        while (gameObject.activeSelf) {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.up);
            if (Physics.Raycast(ray, out hit, 0.5f)) {
                if (hit.collider.CompareTag("Liquid")) {
                    targetLiquidType = hit.collider.GetComponentInParent<LiquidType>();
                } else {
                    targetLiquidType = null;
				}
			} else {
                targetLiquidType = null;
            }
            yield return null;
		}
	}

    // Found an equation https://highschoolenergy.acs.org/content/hsef/en/how-can-energy-change/energy-efficiency-of-heating-water/_jcr_content/articleContent/image_6.img.jpg/1382993887369.jpg
    // Energy released by burning natural gas (J) = time(s) * (vol gas (L) / time (s)) * (1 mol gas / 22.4 L) * (deltaH rxn (J) / 1 mol gas)
    // Using the reaction CH4(g) + 2O2(g) --> 2H2O(g) + CO2(g)
    // Delta Hf at 25C kj/mol for CH4 = -74.85, O2 = 0, H2O = -241.8, CO2 = -393.5
    // Delta Hrxn = Sum(nDeltaHf(Products)) - Sum(nDeltaHf(Reactants))
    // (2(-241.8) + -393.5) - (-74.95 + 2(0))
    // = -802.15 kj/mol
    // = 802.15 kj/mol released after reaction in the form of heat.
    private void HeatHit() {
        if (targetLiquidType != null) {
            // In Joules
            float energyReleased = Time.deltaTime * (flowRateVolume / 60 / 60 * CUBICM_TO_ML / 1000) * (1 / 22.4f) * deltaHBurningReaction * 1000;
            targetLiquidType.AddHeat(energyReleased); 
		}
	}
}
