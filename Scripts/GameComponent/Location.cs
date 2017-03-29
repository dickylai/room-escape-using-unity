using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// Location class defines a location for an interactive object
	public class Location : GameComponent {
		protected bool IsOccupied;
		public Location (GameObject point) {
			IsOccupied = false;
			name = point.name;
			obj = point;
			Debug.Log("Location for " + obj.transform.parent.name + " is added as " + name);
			this.originX = point.transform.position.x;
			this.originY = point.transform.position.y;
			this.originZ = point.transform.position.z;
		}

		public bool Occupy () {
			if (!IsOccupied) {
				IsOccupied = true;
				return true;
			} else {
				return false;
			}
		}
	}
}