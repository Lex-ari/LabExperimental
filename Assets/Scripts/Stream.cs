using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{

	private LineRenderer lineRenderer = null;
	private Vector3 targetPosition = Vector3.zero;
	private ParticleSystem splashParticle = null;
	private Coroutine pourRoutine = null;

	private LiquidType targetLiquidType = null;
	public GameObject puddlePrefab;

	public float bufferVolume = 0f; // meters ^3

	private void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
		splashParticle = GetComponentInChildren<ParticleSystem>();
	}

	private void Start() {
		MoveToPosition(0, transform.position);
		MoveToPosition(1, transform.position);
	}

	// Used for external functions, begins the pouring "animation"
	public void Begin() {
		StartCoroutine(UpdateParticle());
		StartCoroutine(DetermineHit());
		pourRoutine = StartCoroutine(BeginPour());
	}

	// Used for external functions, ends the current "animation"
    public void End() {
		StopCoroutine(pourRoutine);
		pourRoutine = StartCoroutine(EndPour());
	}

	// Coroutine that moves the begining to the deepest point, simulating the shut off of the fluid.
	// Gameobject is destroyed once the beginning is at the end.
	private IEnumerator EndPour() {
		while (!HasReachedPosition(0, targetPosition)) {
			AnimateToPosition(0, targetPosition);
			AnimateToPosition(1, targetPosition);
			yield return null;
		}
		Destroy(gameObject);
	}

	// Coroutine that continously moves the end position of the line renderer to the deepest point.
	private IEnumerator BeginPour() {
		while (gameObject.activeSelf) {
			targetPosition = FindEndPoint();
			MoveToPosition(0, transform.position);
			AnimateToPosition(1, targetPosition);
			AddBufferToHit();
			yield return null;
		}
	}

	// Creates a raycast and returns the position of the first object it intersects.
	// If it does not interact with an object, the default max distance is used.
	private Vector3 FindEndPoint() {
		RaycastHit hit;
		Ray ray = new Ray(transform.position, Vector3.down);
		Physics.Raycast(ray, out hit, 2.0f);
		Vector3 endPoint = hit.collider ? hit.point : ray.GetPoint(2.0f);
		return endPoint;
	}

	// Uses MoveTowards to slowly move the beginning or ending position to target position
	private void AnimateToPosition(int index, Vector3 targetPosition) {
		Vector3 currentPoint = lineRenderer.GetPosition(index);
		Vector3 newPosition;
		if (currentPoint.y < targetPosition.y) {
			newPosition = targetPosition;
		} else {
			newPosition = Vector3.MoveTowards(currentPoint, targetPosition, Time.deltaTime * 1.75f);
		}
		lineRenderer.SetPosition(index, newPosition);
	}

	// Simple boolean if the beginning or ending position is the target position
	private bool HasReachedPosition(int index, Vector3 targetPosition) {
		return lineRenderer.GetPosition(index) == targetPosition;
	}

	// Allows setting the beginning and ending position of the line renderer to a target position.
	private void MoveToPosition(int index, Vector3 targetPosition) {
		lineRenderer.SetPosition(index, targetPosition);
	}

	// Placeholder - Used for splash effects
	private IEnumerator UpdateParticle() {
		while (gameObject.activeSelf) {
			splashParticle.gameObject.transform.position = targetPosition;
			bool isHitting = HasReachedPosition(1, targetPosition);
			splashParticle.gameObject.SetActive(isHitting);
			yield return null;
		}
	}

	private IEnumerator DetermineHit() {
		while (gameObject.activeSelf) {
			if (HasReachedPosition(1, targetPosition)) {
				RaycastHit hit;
				Ray ray = new Ray(lineRenderer.GetPosition(1) + Vector3.up * 0.1f, Vector3.down);
				if(Physics.Raycast(ray, out hit)) {
					if (hit.collider.CompareTag("Liquid")) {
						targetLiquidType = hit.collider.GetComponent<LiquidType>();
					} else {
						targetLiquidType = CreatePuddle();
					}
				}
			}
			yield return null;
		}
	}


	public void SetWidthMultiplier(float width, float radius) {
		float percentage = (width / (2 * radius)) * 0.25f;
		float startWidth = 0.025f * 0.25f + width * percentage;
		float endWidth = 0.025f + 2 * radius *  percentage;

		lineRenderer.startWidth = startWidth;
		lineRenderer.endWidth = endWidth;
		var shape = splashParticle.shape;
		shape.radius = endWidth / 2;
		//Debug.Log("startwidth:" + startWidth + " endWidth:" + endWidth + " percentage:" + percentage);
	}

	private Puddle CreatePuddle() {
		GameObject puddleObject = Instantiate(puddlePrefab, targetPosition, Quaternion.identity);
		return puddleObject.GetComponent<Puddle>();
	}

	public void AddVolumeToBuffer(float volume) {
		bufferVolume += volume;
	}

	private void AddBufferToHit() {
		if (targetLiquidType != null) {
			float fallDistance = lineRenderer.GetPosition(0).y - lineRenderer.GetPosition(1).y;
			//Using kinematics, v^2 = v^2,0 + 2aDeltaX
			float velocity = Mathf.Sqrt(2f * 9.8f * fallDistance);
			float endWidth = lineRenderer.endWidth;
			float area = Mathf.Pow(endWidth / 2, 2) * Mathf.PI;
			float rateVolumeDepleted = area * velocity; // Meters ^3 / t
			float volumeDepletedFrame = rateVolumeDepleted * Time.deltaTime; // Meters ^3
			if (bufferVolume > 0) {
				bufferVolume -= volumeDepletedFrame;
				targetLiquidType.AddVolume(volumeDepletedFrame);
			}
		}
	}
}
