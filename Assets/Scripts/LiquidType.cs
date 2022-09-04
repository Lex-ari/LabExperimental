using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LiquidType : MonoBehaviour 
{
    protected const float CUBICM_TO_ML = 1E6f;
    protected const float ML_TO_CUBICM = 1E-6f;

    protected float ContainerVolume; // In Mililiters (mL)
    public float LiquidVolume; // In Mililiters (mL)

    // Adds volume to the container in Mililiters
    public virtual void AddVolume(float volume) {
        LiquidVolume += volume;
    }


}
