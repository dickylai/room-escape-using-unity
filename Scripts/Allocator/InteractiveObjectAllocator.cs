using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	public class InteractiveObjectAllocator {
		public List<KeyValuePair<InteractiveObject, InteractiveObject>> relationList;
		public List<InteractiveObject> interactiveObjects;
		public List<InterIntObj> nonShownInteractiveObjects;
		public List<Location> locations;
		public List<LockLocation> lockLocations;
		private InterIntObj lastIntObj;
		public List<string> priorityList;
		private System.Random prng;
		public List<Door> doors;
		public List<Lock> locks;
		public List<InteractiveObject> keys;
		List<InteractiveObject> tempKeys;
		List<KeyValuePair<InteractiveObject, int>> tempSplitKeys;
		private List<int> roomOpenPosition;

		public InteractiveObjectAllocator (System.Random prng) {
			relationList = new List<KeyValuePair<InteractiveObject, InteractiveObject>> ();
			interactiveObjects = new List<InteractiveObject> ();
			nonShownInteractiveObjects = new List<InterIntObj> ();
			locations = new List<Location> ();
			lockLocations = new List<LockLocation> ();
			lastIntObj = null;
			priorityList = new List<string> ();
			this.prng = prng;
			this.doors = new List<Door> ();
			this.locks = new List<Lock> ();
			this.keys = new List<InteractiveObject> ();
			this.tempKeys = new List<InteractiveObject> ();
			this.tempSplitKeys = new List<KeyValuePair<InteractiveObject, int>> ();
			roomOpenPosition = new List<int> (); // record room open moment with key
		}

		// function for checking if a location is empty 
		public bool IsEmptyLocationExisted () {
			return (locations.Count + lockLocations.Count + doors.Count - interactiveObjects.Count + nonShownInteractiveObjects.Count > 0) ? true : false;
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
					InteractiveObject linkedObj = GetInteractiveObject (lockLocations [chooseLocation].GetGameObject ().transform.parent.gameObject);
					InteractiveObject intObj = new Lock (lockLocations [chooseLocation], obj, nextType);
					interactiveObjects.Add (intObj);
					locks.Add ((Lock)intObj);
					relationList.Add (
						new KeyValuePair<InteractiveObject,InteractiveObject> (
							intObj, 
							linkedObj));
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
				} else if (obj.transform.CompareTag ("nonStaticObjSwitch")) {
					intObj = new SwitchIntObj (locations [chooseLocation], obj);
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

		// function for allocating a intermediate object
		public bool AllocateInterIntObj (int chooseObj, GameObject obj, List<string> nextType, string thumbnail) {
			InteractiveObject intObj = new InterIntObj (null, obj, nextType, thumbnail);
			interactiveObjects.Add (intObj);
			nonShownInteractiveObjects.Add ((InterIntObj) intObj);
			relationList.Add (
				new KeyValuePair<InteractiveObject,InteractiveObject> (
					intObj, 
					locks [chooseObj]));
			keys.Add (intObj);
			lastIntObj = (InterIntObj) intObj;
			return true;
		}

		public bool AllocateSplitIntObj (GameObject obj, List<string> nextType, string thumbnail) {
			for (int chooseLocation = 0; chooseLocation < locations.Count; chooseLocation++) {
				if (locations [chooseLocation].Occupy ()) {
					InteractiveObject intObj = new PickIntObj (locations [chooseLocation], obj, nextType, thumbnail);
					interactiveObjects.Add (intObj);
					relationList.Add (
						new KeyValuePair<InteractiveObject,InteractiveObject> (
							intObj, 
							lastIntObj));
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
			if (intObj == null) {
				selectedIntObj.Choose ();
				return;
			}
			if (selectedIntObj != null)
				selectedIntObj.Click ();
			bool related = false;
			for (int i = 0; i < relationList.Count; i++) {
				if (relationList [i].Value == intObj) {
					if (relationList [i].Key == selectedIntObj)
						related = true;
					if (!relationList[i].Key.IsSolved ()) {
						if (selectedIntObj != null)
							selectedIntObj.Choose ();
						return;
					}
				} 
			}
			if (!related && selectedIntObj != null) {
				selectedIntObj.Choose ();
				return;
			}
			intObj.Click ();
		}

		// function for checking if objects can be combined together
		public void CheckCombine (PickIntObj objA, PickIntObj objB) {
			if (objA != null)
				objA.Click ();
			if (objB != null)
				objB.Click ();
			InteractiveObject temp = null;
			for (int i = 0; i < relationList.Count; i++) {
				if (relationList [i].Key == objA) {
					temp = relationList [i].Value;
					break;
				} 
			}
			for (int i = 0; i < relationList.Count; i++) {
				if (relationList [i].Value == temp && relationList [i].Key == objB) {
					temp.Click ();
					return;
				} 
			}
			objA.Choose ();
			objB.Choose ();
		}

		// function for shifting keys in the final state of generation
		public void ShiftKeys () {
			shiftAllKeysForOnePosition ();
			// shiftSplitKey ();
		}

		private void shiftAllKeysForOnePosition () {
			Vector3 finalPosition = new Vector3 ();
			Transform finalParent = null;
			tempKeys.AddRange (keys);
			if (tempKeys.Count > 1) {
				int tempKeysLength = tempKeys.Count;
				int limit = roomOpenPosition[0];
				for (int i = 0; i < tempKeysLength; i++) {
					if (roomOpenPosition.Count > 1)
						if (i >= roomOpenPosition[1]) {
							roomOpenPosition.RemoveAt (0);
							limit = roomOpenPosition[0];
						}
					if (tempKeys [i].GetType ().Name == "InterIntObj") {
						tempSplitKeys.Add (new KeyValuePair<InteractiveObject, int> (tempKeys [i + 2], System.Math.Max(i - 1, limit)));
						tempKeys.RemoveAt (i + 2);
						tempKeys.RemoveAt (i);
						tempKeysLength -= 2;
						for (int j = 0; j < roomOpenPosition.Count; j++) {
							if (i + 2 < roomOpenPosition[j])
								roomOpenPosition[j]--;
							if (i < roomOpenPosition[j])
								roomOpenPosition[j]--;
						}
					}
				}

				for (int i = tempKeys.Count - 1; i > 0; i--) {
					if (i == tempKeys.Count - 1) {
						finalPosition = tempKeys [i].GetObjPosition ();
						finalParent = tempKeys [i].GetObjParent ();
					}
					tempKeys [i].SetObjPosition (tempKeys [i - 1].GetObjPosition ());
					tempKeys [i].SetObjParent (tempKeys [i - 1].GetObjParent ());
				}
				tempKeys [0].SetObjPosition (finalPosition);
				tempKeys [0].SetObjParent (finalParent);
			}
		}

		private void shiftSplitKey () {
			foreach (KeyValuePair<InteractiveObject, int> pair in tempSplitKeys) {
				Vector3 tempPosition = tempKeys [pair.Value].GetObjPosition ();
				Transform tempParent = tempKeys [pair.Value].GetObjParent ();
				tempKeys [pair.Value].SetObjPosition (pair.Key.GetObjPosition ());
				tempKeys [pair.Value].SetObjParent (pair.Key.GetObjParent ());
				pair.Key.SetObjPosition (tempPosition);
				pair.Key.SetObjParent (tempParent);
			}
		}

		public void RoomIsAdded () {
			roomOpenPosition.Add (keys.Count);
		}

		public bool IsGameEnd () {
			return doors [0].IsOpened ();
		}

		public bool IsEmptyList() {
			return (priorityList.Count == 0) ? true : false;
		}
	}
}