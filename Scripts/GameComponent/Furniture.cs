using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// Furniture class defines a furniture in a room
	public class Furniture : GameComponent {
		protected float rotation;
		protected Vector3 location;

		public Furniture (string name, float originX, float originZ, GameObject prefab) {
			obj = prefab;
			width = obj.transform.localScale.x;
			depth = obj.transform.localScale.z;
			height = obj.transform.localScale.y;
			obj.name = name;
			obj.transform.position = new Vector3 (originX, height / 2, originZ);
			rotation = obj.transform.localRotation.y;
		}

		public float GetRotation () {
			return rotation;
		}

		public Vector3 GetLocation () {
			return location;
		}
	}
}