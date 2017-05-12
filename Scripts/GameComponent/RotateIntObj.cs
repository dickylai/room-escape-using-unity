using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {

	public class RotateIntObj : InteractiveObject {
		private bool isOpened;
		public RotateIntObj (Location location, GameObject prefab, int keyNo) : base (location, prefab, keyNo) {
			isOpened = false;
		}

		public override void Click () {
			if (!isSolved && !isOpened) {
				obj.gameObject.transform.GetComponent<Interaction> ().Rotation (-90);
				base.Click ();
				isOpened = true;
			} 
//			else if (isSolved && isOpened) {
//				obj.gameObject.transform.GetComponent<Interaction> ().Rotation (90);
//				isOpened = false;
//			}
		}
	}
}