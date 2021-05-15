using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
	[Range(0, 50)]
	public float viewRadius;

	[Range(0, 360)]
	public float viewAngle;

	[Range(0, 1)]
	public float findTargetDelay = 0.2f;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<GameObject> visibleTargets = new List<GameObject>();

	public Action<GameObject> OnObjectExit;
	public Action<GameObject> OnObjectEnter;

	void Start()
	{
		StartCoroutine(FindTargetsCoroutine(findTargetDelay));
	}

    IEnumerator FindTargetsCoroutine(float delay)
	{
		while (true)
		{
			var oldVisibleTargets = visibleTargets;
			var newVisibleTargets = FindVisibleTargets();

			visibleTargets = newVisibleTargets;

			EventCalls(oldVisibleTargets, newVisibleTargets);			

			yield return new WaitForSeconds(delay);
		}
	}

	List<GameObject> FindVisibleTargets()
	{
		List<GameObject> targets = new List<GameObject>();

		// get all objects with the target mask inside of a sphere from the current position.
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);		

        foreach (var target in targetsInViewRadius)
        {
			Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

			// if the target is inside of the view angle
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

				// if there are no obstacles between the two points
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
				{
					// add the object to the visibles targets
					targets.Add(target.gameObject);
				}
			}
		}

		return targets;
	}

	void EventCalls(List<GameObject> oldVisibleTargets, List<GameObject> newVisibleTargets)
	{
        foreach (var target in oldVisibleTargets)
        {
			if (!newVisibleTargets.Exists(x => x == target))
			{
				OnObjectExit?.Invoke(target);							
			}
		}

		foreach (var target in newVisibleTargets)
		{
			if (!oldVisibleTargets.Exists(x => x == target))
			{
				OnObjectEnter?.Invoke(target);
			}
		}
	}

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}

		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}
