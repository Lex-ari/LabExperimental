using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LiquidType : MonoBehaviour 
{

    public float Fill;

    public virtual void AddVolume(float volume) {
        Fill += volume;
	}


}
