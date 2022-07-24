using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{

	private LineRenderer lineRenderer = null;
	private Vector3 targetPosition = Vector3.zero;
	private ParticleSystem splashParticle = null;
	private Coroutine pourRoutine = null;

	private void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
		//splashParticle = GetComponentInChildren<ParticleSystem>();
	}

	private void Start() {
		MoveToPosition(0, transform.position);
		MoveToPosition(1, transform.position);
	}

	// Used for external functions, begins the pouring "animation"
	public void Begin() {
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
			yield return null;
		}
	}

	public void SetWidthMultiplier(float width, float radius) {
		float percentage = (width / (2 * radius)) * 0.1f;
		lineRenderer.startWidth = 0.0025f * 0.25f + (width - 0.0025f * 0.25f) * percentage;
		lineRenderer.endWidth = 0.0025f + (radius - 0.0025f) * percentage;
	}
}
