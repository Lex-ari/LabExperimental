using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LiquidType : MonoBehaviour 
{

    public float Fill; // Currently in 0-1
    public float Volume;

    public virtual void AddVolume(float volume) {
        Fill += volume;
	}


}
