using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LiquidType : MonoBehaviour 
{
    protected const float CUBICM_TO_ML = 1E6f;
    protected const float ML_TO_CUBICM = 1E-6f;

    protected float containerVolume; // In Mililiters (mL)
    public float liquidVolume; // In Mililiters (mL)
    public bool isHeating;
    public float volumeTemperature = 0f;
    public ParticleSystem steamParticle;

    // Adds volume to the container in Mililiters
    public virtual void AddVolume(float volume) {
        liquidVolume += volume;
    }

    public virtual void AddHeat(float heat) { // heat in joules
        if (!steamParticle.gameObject.activeSelf && volumeTemperature > 330000) {
            steamParticle.gameObject.SetActive(true);
		}
        volumeTemperature += heat;
        Debug.Log(volumeTemperature);
    }

}
