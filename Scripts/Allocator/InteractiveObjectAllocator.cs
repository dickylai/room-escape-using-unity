using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	public class InteractiveObjectAllocator {
		public List<KeyValuePair<InteractiveObject, InteractiveObject>> relationList;
		public List<InteractiveObject> interactiveObjects;
		public List<Location> locations;
		public List<LockLocation> lockLocations;
		private System.Random prng;
		public List<Door> doors;
		public List<Lock> locks;
		public List<InteractiveObject> keys;

		public InteractiveObjectAllocator (System.Random prng) {
			relationList = new List<KeyValuePair<InteractiveObject, InteractiveObject>> ();
			interactiveObjects = new List<InteractiveObject> ();
			locations = new List<Location> ();
			lockLocations = new List<LockLocation> ();
			this.prng = prng;
			this.doors = new List<Door> ();
			this.locks = new List<Lock> ();
			this.keys = new List<InteractiveObject> ();
		}

		// function for allocating a door
		public void AllocateDoor (float originX, float originZ, float rotation, GameObject obj) {
			Door door = new Door ("Door" + (doors.Count + 1), originX, originZ, rotation, obj);
			doors.Add (door);
			interactiveObjects.Add (door);
			// TODO: add door location
		}

		// function for allocating a location (i.e. location without an object allocated)
		public void AllocateLocation (GameObject loc) {
			locations.Add (new Location (loc));
		}

		// function for allocating a lock location (i.e. location without an lock object allocated)
		public void AllocateLockLocation (GameObject loc) {
			LockLocation lockLocation = new LockLocation (loc);
			lockLocations.Add (lockLocation);
		}

		// function for allocating a lock object 
		public bool AllocateLock (GameObject obj, List<string> nextType) {
			for (int chooseLocation = 0; chooseLocation < lockLocations.Count; chooseLocation++) {
				if (lockLocations [chooseLocation].Occupy ()) {
					InteractiveObject intObj = new Lock (lockLocations [chooseLocation], obj, nextType);
					interactiveObjects.Add (intObj);
					locks.Add ((Lock)intObj);
					relationList.Add (
						new KeyValuePair<InteractiveObject,InteractiveObject> (
							intObj, 
							GetInteractiveObject (lockLocations [chooseLocation].GetGameObject ().transform.parent.gameObject)));
					return true;
				}
			}
				return false;
		}

		// function for allocating a object on a furniture
		public void AllocateFurnIntObj (GameObject obj) {
			InteractiveObject intObj = null;
			AllocateLocation (obj);
			int chooseLocation = locations.Count - 1;
			if (locations [chooseLocation].Occupy ()) {
				if (obj.transform.CompareTag ("nonStaticObjPull")) {
					intObj = new PullIntObj (locations [chooseLocation], obj);
				} else if (obj.transform.CompareTag ("nonStaticObjRotate")) {
					intObj = new RotateIntObj (locations [chooseLocation], obj);
				}
				interactiveObjects.Add (intObj);
			}
		}

		// function for allocating a picking object
		public bool AllocatePickIntObj (int chooseObj, GameObject obj, List<string> nextType, string thumbnail) {
			for (int chooseLocation = 0; chooseLocation < locations.Count; chooseLocation++) {
				if (locations [chooseLocation].Occupy ()) {
					InteractiveObject intObj = new PickIntObj (locations [chooseLocation], obj, nextType, thumbnail);
					interactiveObjects.Add (intObj);
					relationList.Add (
						new KeyValuePair<InteractiveObject,InteractiveObject> (
							intObj, 
							locks [chooseObj]));
					keys.Add (intObj);
					return true;
				}
			}
			return false;
		}

		// function for getting next object type
		public bool GetNextObjType (out int chooseObj, out string type) {
			type = "";
			for (chooseObj = 0; chooseObj < locks.Count; chooseObj++) {
				if ((type = locks [chooseObj].GetNextType (prng.Next (0, 10))) != "") {
					return true;
				}
			}
			return false;
		}

		// function for finding the specific object
		public InteractiveObject GetInteractiveObject (GameObject obj) {
			for (int i = 0; i < interactiveObjects.Count; i++) {
				if (obj == interactiveObjects [i].GetGameObject ()) {
					return interactiveObjects [i];
				}
			}
			return null;
		}

		// function for checking if the condition(s) of an obj is fulfilled
		public void GetLink (InteractiveObject intObj, PickIntObj selectedIntObj) {
			Debug.Log ("THIS IS: " + intObj.GetName ());
			if (selectedIntObj != null)
				selectedIntObj.Click ();
			for (int i = 0; i < relationList.Count; i++) {
				if (relationList [i].Value == intObj) {
					if (!relationList[i].Key.IsSolved ()) {
						if (selectedIntObj != null)
							selectedIntObj.Choose ();
						return;
					}
				} 
			}
			intObj.Click ();
		}

		// function for shifting keys in the final state of generation
		public void ShiftKeys () {
			Vector3 finalPosition = new Vector3 ();
			Transform finalParent = null;
			for (int i = keys.Count - 1; i > 0; i--) {
				if (i == keys.Count - 1) {
					finalPosition = keys [i].GetObjPosition ();
					finalParent = keys [i].GetObjParent ();
				}
				keys [i].SetObjPosition (keys [i - 1].GetObjPosition ());
				keys [i].SetObjParent (keys [i - 1].GetObjParent ());
			}
			keys [0].SetObjPosition (finalPosition);
			keys [0].SetObjParent (finalParent);
		}
	}
}