using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace RoomEscape {
	// The RoomGenerator class generates all the rooms, furnitures and relations between different objects.
	public class RoomGenerator : MonoBehaviour {
		// variables for the databases
		string furnitureDbFilePath;
		XmlDocument furnitureDb;
		FileStream furnitureDbFile;

		string furnitureSetDbFilePath;
		XmlDocument furnitureSetDb;
		FileStream furnitureSetDbFile;

		string intObjDbFilePath;
		XmlDocument intObjDb;
		FileStream intObjDbFile;

		string lockDbFilePath;
		XmlDocument lockDb;
		FileStream lockDbFile;


		// the seed using for procedural generation
		private System.Random prng;

		// list for storing different prefabs
		public static List<Object> doorPrefab;

		// Classes contain methods for assigning different objects
		public RoomAllocator ra;
		public InteractiveObjectAllocator ia;

		// Class for getting information from menu
		private Menu menu;
		private int difficulty;

		bool gen = false;

		// Room generation starts
		void Start () {
			menu = GetComponent<Menu> ();
			doorPrefab = new List<Object> ();

			furnitureDbFilePath = "./Assets/Database/furnitures.xml";
			furnitureSetDbFilePath = "./Assets/Database/furnituresets.xml";
			intObjDbFilePath = "./Assets/Database/intobjs.xml";
			lockDbFilePath = "./Assets/Database/locks.xml";

			furnitureDb = new XmlDocument();
			furnitureDbFile = new FileStream(furnitureDbFilePath, FileMode.Open);
			furnitureDb.Load (furnitureDbFile);

			furnitureSetDb = new XmlDocument();
			furnitureSetDbFile = new FileStream(furnitureSetDbFilePath, FileMode.Open);
			furnitureSetDb.Load (furnitureSetDbFile);

			intObjDb = new XmlDocument();
			intObjDbFile = new FileStream(intObjDbFilePath, FileMode.Open);
			intObjDb.Load (intObjDbFile);

			lockDb = new XmlDocument();
			lockDbFile = new FileStream(lockDbFilePath, FileMode.Open);
			lockDb.Load (lockDbFile);

			int maxNumOfRoom = 0;
			int maxNumOfFurn = 0;
			int roomNo = -1;
			if (!gen) {
				// get the seed and difficulty
				prng = new System.Random (menu.GetSeed ().GetHashCode ());
				difficulty = menu.GetDifficulty ();
				// set the room difficulty parameters
				if (difficulty == 0) {
					maxNumOfRoom = prng.Next (1, 3);
					maxNumOfFurn = prng.Next (3, 5);
				} else if (difficulty == 1) {
					maxNumOfRoom = prng.Next (3, 5);
					maxNumOfFurn = prng.Next (3, 5);
				} else if (difficulty == 2) {
					maxNumOfRoom = prng.Next (4, 6);
					maxNumOfFurn = prng.Next (4, 6);
				} 
				Debug.Log ("maxNumOfRoom = " + maxNumOfRoom + "\nmaxNumOfFurn = " + maxNumOfFurn);

				// initialize the allocators
				ra = new RoomAllocator (prng);
				ia = new InteractiveObjectAllocator (prng);

				// generation algorithm as follows
				int counter = 0;
				int openRoomFactor = 2 * maxNumOfRoom;
				while (true) {
					if (ra.rooms.Count == maxNumOfRoom && ra.IsAllRoomsFull ()) { // end the generation
						if (ia.IsEmptyList ()) {
							generateKeyForLockDecision ();
							if (!ia.IsEmptyList ()) 
								generateFurnitureDecision (ra.rooms [roomNo]);
						} 
						if (!ia.IsEmptyList ()) {
							generateSplitKey ();
							if (!ia.IsEmptyList ()) {
								do {
									generateFurnitureDecision (ra.rooms [roomNo]); // generate furniture with no lock(s)
								} while (!ia.IsEmptyLocationExisted ());
								generateSplitKey ();
							}
						}
						break;
					}
					if (!getRoomDecision (ref roomNo, ref openRoomFactor, maxNumOfRoom)) // no room selected successfully
						continue;

					if (ia.IsEmptyLocationExisted ()) {
						if (ia.IsEmptyList ()) {
							if (generateKeyForLockDecision() == -1) { // no lock
								generateFurnitureDecision (ra.rooms [roomNo]); // generate furniture with lock(s)
							}
						} else {
							if (generateSplitKey ()) {
								if (!ia.IsEmptyList ()) generateFurnitureDecision (ra.rooms [roomNo]); // generate furniture with no lock(s)
							}
						}
					} else {
						generateFurnitureDecision (ra.rooms [roomNo]); // generate furniture without lock
					}

					if (ra.rooms [roomNo].fa.IsEnoughFurniture (maxNumOfFurn)) {
						ra.roomsIsFull [roomNo] = true;
					}

					if (++counter > 400) {
						Debug.Log ("Too many loops.");
						break;
					}
				}
				ia.ShiftKeys ();

				// reset the position of the carmera
				this.transform.position = new Vector3 (0, 0, 0);

				// print the game info in console
				printInfo ();
				gen = true;
			}
		}

		// function for loading furniture set prefab
		Object getFurnitureSetPrefab (int needAgainstWall, int needLocks, out int againstWall, out int locations, out string lockTypes, out List<KeyValuePair<string, string>> pathList) {
			int getNum;
			do {
				getNum = prng.Next (0, furnitureSetDb.GetElementsByTagName("FurnitureSet").Count);
				againstWall = int.Parse (furnitureSetDb.GetElementsByTagName ("AgainstWall") [getNum].InnerText);
				locations = int.Parse (furnitureSetDb.GetElementsByTagName ("Locations") [getNum].InnerText);
				lockTypes = furnitureSetDb.GetElementsByTagName ("Locks") [getNum].InnerText;
			} while ((needLocks == 0 && lockTypes != "") || (needLocks == 1 && lockTypes == "") || (needAgainstWall != againstWall && needAgainstWall != -1));
			pathList = new List<KeyValuePair<string, string>> ();
			foreach (XmlNode node in furnitureSetDb.GetElementsByTagName ("Anchors") [getNum].ChildNodes) {
				pathList.Add (new KeyValuePair<string, string> (node.Attributes ["id"].Value, node.ChildNodes.Item(prng.Next(0, node.ChildNodes.Count)).InnerText));
			}
			return Resources.Load (furnitureSetDb.GetElementsByTagName ("Path") [getNum].InnerText);
		}

		// function for loading furniture prefab
		Object getFurniturePrefab (int needAgainstWall, int needLocks, out int againstWall, out int locations, out int lockLocations) {
			int getNum;
			do {
				getNum = prng.Next (0, furnitureDb.GetElementsByTagName("Furniture").Count);
				againstWall = int.Parse (furnitureDb.GetElementsByTagName ("AgainstWall") [getNum].InnerText);
				locations = int.Parse (furnitureDb.GetElementsByTagName ("Locations") [getNum].InnerText);
				lockLocations = int.Parse (furnitureDb.GetElementsByTagName ("Locks") [getNum].InnerText);
			} while ((needLocks == 0 && lockLocations != 0) || (needLocks == 1 && lockLocations == 0) || (needAgainstWall != againstWall && needAgainstWall != -1));
			return Resources.Load (furnitureDb.GetElementsByTagName ("Path") [getNum].InnerText);
		}

		// function for loading and storing door prefabs
		Object getDoorPrefab () {
			if (doorPrefab.Count == 0) {
				doorPrefab.AddRange (Resources.LoadAll ("Doors"));
			}
			return doorPrefab [prng.Next (0, doorPrefab.Count)];
		}

		// function for loading lock prefab
		Object getLockPrefab (string type, out List<string> nextType) {
			int getNum;
			bool match = false;
			do {
				getNum = prng.Next (0, lockDb.GetElementsByTagName ("Lock").Count);
				nextType = new List<string> ();
				foreach (XmlNode node in lockDb.GetElementsByTagName ("NextTypes") [getNum]) {
					nextType.Add(node.InnerText);
				}
				foreach (XmlNode node in lockDb.GetElementsByTagName ("Types") [getNum]) {
					if (type == node.InnerText) {
						match = true;
						break;
					}
				}
			} while ((type != "") && (match == false));
			return Resources.Load (lockDb.GetElementsByTagName ("Path") [getNum].InnerText);
		}

		// function for loading int obj prefab
		Object getIntObjPrefab (string type, out List<string> nextType, out string thumbnail) {
			int getNum;
			do {
				getNum = prng.Next (0, intObjDb.GetElementsByTagName ("Obj").Count);
				nextType = new List<string> ();
				foreach (XmlNode node in intObjDb.GetElementsByTagName ("NextTypes") [getNum]) {
					nextType.Add (node.InnerText);
				}
				thumbnail = intObjDb.GetElementsByTagName ("Thumbnail") [getNum].InnerText;
			} while ((type != "") && (type != intObjDb.GetElementsByTagName ("Type") [getNum].InnerText) || ((nextType.Count == 1) && nextType [0] == "NULL"));
			ia.priorityList.AddRange (nextType);
			return Resources.Load (intObjDb.GetElementsByTagName ("Path") [getNum].InnerText);
		}

		Object getSplitIntObjPrefab (string name, out List<string> nextType, out string thumbnail) {
			nextType = new List<string> ();
			thumbnail = "";
			for (int i = 0; i < intObjDb.GetElementsByTagName ("Obj").Count; i++) {
				if (intObjDb.GetElementsByTagName ("Name") [i].InnerText == name) {
					thumbnail = intObjDb.GetElementsByTagName ("Thumbnail") [i].InnerText;
					return Resources.Load (intObjDb.GetElementsByTagName ("Path") [i].InnerText);
				}
			}
			return null;
		}

		// function for generating a new room
		void generateRoom () {
			float doorOriginX, doorOriginZ, doorRotation;
			if (ra.AllocateRoom (out doorOriginX, out doorOriginZ, out doorRotation)) {
				generateDoor (doorOriginX, doorOriginZ, doorRotation);
				generateLock ("Door");
			}
			ia.RoomIsAdded ();
		}

		// function called by generateRoom for generating a new door
		void generateDoor (float doorOriginX, float doorOriginZ, float doorRotation) {
			GameObject door = (GameObject)Instantiate (getDoorPrefab ());
			ia.AllocateDoor (doorOriginX, doorOriginZ, doorRotation, door);
			registerInteractiveObject (door);
		}

		// function for generating a set of furnitures
		void generateFurnitureSet (Room room, int needLocks, out string lockTypes) {
			int againstWall, locations;
			List<KeyValuePair<string, string>> pathList = new List<KeyValuePair<string, string>> ();
			GameObject furnitureSet = (GameObject)Instantiate (getFurnitureSetPrefab (-1, needLocks, out againstWall, out locations, out lockTypes, out pathList));
			if (room.fa.AllocateFurnitureSet (furnitureSet, againstWall)) {
				for (int i = 0; i < furnitureSet.transform.childCount; i++) {
					foreach (KeyValuePair<string, string> path in pathList) {
						if (path.Key == furnitureSet.transform.GetChild (i).name) {
							generateFurniture (room, path.Value, furnitureSet.transform.GetChild (i).gameObject);
						}
					}
				}
			} else {
				Destroy (furnitureSet);
			}
		}

		// function for generating a new furniture
		void generateFurniture (Room room, int needLocks) {
			int againstWall, locations, lockLocations;
			GameObject furniture = (GameObject)Instantiate (getFurniturePrefab (-1, needLocks, out againstWall, out locations, out lockLocations));
			if (room.fa.AllocateFurniture (furniture, againstWall)) {
				registerInteractiveObject (furniture);
			} else {
				Destroy (furniture);
			}
		}
		void generateFurniture (Room room, string path, GameObject parent) {
			GameObject furniture = (GameObject)Instantiate (Resources.Load (path));
			furniture.transform.SetParent (parent.transform);
			if (room.fa.AllocateFurniture (furniture)) {
				registerInteractiveObject (furniture);
			} else {
				Destroy (furniture);
			}
		}

		bool getRoomDecision(ref int roomNo, ref int openRoomFactor, int maxNumOfRoom) {
			if (roomNo != -1 && (ra.rooms [roomNo].fa.IsNoFurniture () || !ia.IsEmptyList ()))
				return true;
			int prngTry = prng.Next (0, openRoomFactor);
			if (ra.rooms.Count == 0 || (prngTry == 0 && ra.rooms.Count < maxNumOfRoom)) {
				generateRoom ();
				roomNo = ra.rooms.Count - 1;
				generateFurnitureDecision (ra.rooms [roomNo]);
			} else {
				roomNo = prng.Next (0, ra.rooms.Count);
				int countLoop = 0;
				while (ra.roomsIsFull [roomNo]) {
					roomNo = (roomNo + 1) % ra.rooms.Count;
					countLoop++;
					if (countLoop > ra.rooms.Count)
						break;
				}
				if (countLoop > ra.rooms.Count) {
					if (openRoomFactor > 1) openRoomFactor--;
					return false;
				}
			}
			return true;
		}

		int generateKeyForLockDecision () {
			// for all locks
			for (int i = 0; i < ia.locks.Count; i++) {
				// check if any locks without a key
				if (!ia.locks [i].HasNext ()) {
					if (!generateIntObj ()) {
						return 0;
					}
					return 1;
				}
			}
			// lock not enough
			return -1;
		}

		// function for generating a new furniture decision
		void generateFurnitureDecision (Room room) {
			string lockType;
			if (ia.IsEmptyList ()) {
				if (room.fa.IsNoFurniture ()) {
					generateFurnitureSet (room, 0, out lockType);
				} else {
					generateFurnitureSet (room, 1, out lockType);
					generateLock (lockType);
				}
			} else {
				generateFurnitureSet (room, 0, out lockType);
			}
		}

		// function for generating a new relation (lock)
		void generateLock (string type) {
			List<string> nextType;
			if ((ia.lockLocations.Count - ia.locks.Count) != 0) {
				GameObject obj = (GameObject)Instantiate (getLockPrefab (type, out nextType));
				if (!ia.AllocateLock (obj, nextType)) Destroy (obj);
			}
		}

		// function for generating a new relation (non-lock)
		bool generateIntObj () {
			List<string> nextType;
			int chooseObj;
			string thumbnail;
			string type;
			if (ia.IsEmptyLocationExisted ()) {
				if (ia.GetNextObjType (out chooseObj, out type)) {
					GameObject obj = (GameObject)Instantiate (getIntObjPrefab (type, out nextType, out thumbnail));
					if (ia.IsEmptyList ()) {
						if (ia.AllocatePickIntObj (chooseObj, obj, nextType, thumbnail)) {
							return true;
						} else {
							Destroy (obj);
							return false;
						}
					} else {
						// need to split
						if (ia.AllocateInterIntObj (chooseObj, obj, nextType, thumbnail)) {
							return true;
						} else {
							Destroy (obj);
							ia.priorityList.Clear ();
							return false;
						}
					}
				}
			}
			return false;
		}

		bool generateSplitKey () {
			List<string> nextType;
			string thumbnail;
			GameObject obj = (GameObject)Instantiate (getSplitIntObjPrefab (ia.priorityList [0], out nextType, out thumbnail));
			if (ia.AllocateSplitIntObj (obj, nextType, thumbnail)) {
				ia.priorityList.RemoveAt (0);
				return true;
			} else {
				Destroy (obj);
				return false;
			}
		}

		// function for registering interactive objects/ location under a furniture
		void registerInteractiveObject (GameObject obj) {
			for (int i = 0; i < obj.transform.childCount; i++) {
				if (obj.transform.GetChild (i).CompareTag ("lockLocation")) {
					ia.AllocateLockLocation (obj.transform.GetChild (i).gameObject);
				}
				else if (obj.transform.GetChild (i).CompareTag ("location")) {
					ia.AllocateLocation (obj.transform.GetChild (i).gameObject);
				}
				else if (obj.transform.GetChild (i).CompareTag ("nonStaticObjPull") || obj.transform.GetChild (i).CompareTag ("nonStaticObjRotate") || obj.transform.GetChild (i).CompareTag ("nonStaticObjSwitch"))
					ia.AllocateFurnIntObj (obj.transform.GetChild (i).gameObject);
				if (obj.transform.GetChild (i).childCount != 0)
					registerInteractiveObject (obj.transform.GetChild (i).gameObject);
			}
		}

		// function for printing game info
		void printInfo () {
			int noOfRooms, noOfDoors, noOfFurnitures, noOfLocks;
			noOfRooms = ra.rooms.Count;
			noOfDoors = ia.doors.Count;
			noOfFurnitures = 0;
			for (int i = 0; i < ra.rooms.Count; i++) {
				noOfFurnitures = noOfFurnitures + ra.rooms [i].fa.furnitures.Count;
			}
			noOfLocks = ia.locks.Count;

			Debug.Log ("Number of room(s) in total: " + noOfRooms
				+ "\nNumber of door(s) in total: " + noOfDoors
				+ "\nNumber of furniture(s) in total: " + noOfFurnitures
				+ "\nNumber of Lock(s) in total: " + noOfLocks
				+ "\n==========End==========");

//			 Debug.Log ("locations:" + ia.locations.Count 
//				+ "\nlockLocations:" + ia.lockLocations.Count 
//				+ "\ndoors:" + ia.doors.Count 
//				+ "\ninteractiveObjects:" + ia.interactiveObjects.Count 
//				+ "\nno of key:" + ia.keys.Count 
//				+ "\nTOTAL:" + (ia.locations.Count + ia.locks.Count + ia.doors.Count - ia.interactiveObjects.Count));
		}
	}
}