using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {

	// Subclass of InteractiveObject for lock
	public class Lock : InteractiveObject {

		public Lock (Location location, GameObject prefab, List<string> nextType, int keyNo) : base (location, prefab, nextType, keyNo) {}

	}

}