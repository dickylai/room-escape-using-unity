using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// Door class defines a door of a room
	public class Door : InteractiveObject {
		private bool isOpened;

		public Door (string name, float originX, float originZ, float rotation, GameObject prefab) {
			obj = prefab;
			width = obj.transform.localScale.x;
			depth = obj.transform.localScale.z;
			height = obj.transform.localScale.y;
			obj.name = name;
			location = new Vector3 (originX, height / 2, originZ);
			obj.transform.position = location;

			this.name = name;
			obj.transform.localRotation = Quaternion.AngleAxis (rotation, obj.transform.up);
			isSolved = false;
			isOpened = false;
		}

		public bool IsOpened () {
			return isOpened;
		}

		public override void Click () {
			if (!isSolved && !isOpened) {
				base.Click ();
				obj.gameObject.transform.GetComponent<Interaction> ().Rotation (90.0f);
				isOpened = true;
			} 
		}
	}
}