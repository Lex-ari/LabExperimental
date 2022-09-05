using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : MonoBehaviour
{

    public bool isOn = true;
    private Object targetLiquidType;

    private IEnumerator DetermineHit() {
        while (gameObject.activeSelf) {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.up);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.CompareTag("Container")) {
                    targetLiquidType = hit.collider.GetComponent<ContainerType>();
                }
			}
            yield return null;
		}
	}
}
