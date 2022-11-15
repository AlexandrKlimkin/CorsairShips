using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct PhysicsUtils
    {
		public static Vector3 DetectFloor(Vector3 originPosition, LayerMask floorMask, float rayCastDistance = 100)
		{
			var hitDown = Physics.Raycast(
					originPosition + Vector3.up * 0.1f,
					Vector3.down,
					out var hitDownInfo,
					rayCastDistance,
					floorMask);
			var distanceDown = hitDown
				? hitDownInfo.distance
				: rayCastDistance;
			Debug.DrawRay(originPosition, Vector3.down * rayCastDistance, Color.gray, 3f);
			Debug.DrawRay(originPosition, Vector3.down * distanceDown, Color.green, 3f);

			var hitUp = Physics.Raycast(
					originPosition + Vector3.down * 0.1f,
					Vector3.up,
					out var hitUpInfo,
					rayCastDistance,
					floorMask);
			var distanceUp = hitUp
				? hitUpInfo.distance
				: rayCastDistance;
			Debug.DrawRay(originPosition, Vector3.up * rayCastDistance, Color.gray, 3f);
			Debug.DrawRay(originPosition, Vector3.up * distanceUp, Color.green, 3f);

			return hitDown || hitUp
				? distanceDown < distanceUp
					? originPosition + Vector3.down * distanceDown
					: originPosition + Vector3.up * distanceUp
				: originPosition;
		}
	}
}