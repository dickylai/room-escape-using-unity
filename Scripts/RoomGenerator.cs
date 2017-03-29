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
		public string furnitureDbFilepath;
		XmlDocument furnitureDb;
		FileStream furnitureDbFile;

		public string intObjDbFilepath;
		XmlDocument intObjDb;
		FileStream intObjDbFile;

		public string lockDbFilepath;
		XmlDocument lockDb;
		FileStream lockDbFile;

		// set the threshold
		public int keyThreshold;

		// the seed using for procedural generation
		public string seed;
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

			furnitureDb = new XmlDocument();
			furnitureDbFile = new FileStream(furnitureDbFilepath, FileMode.Open);
			furnitureDb.Load (furnitureDbFile);

			intObjDb = new XmlDocument();
			intObjDbFile = new FileStream(intObjDbFilepath, FileMode.Open);
			intObjDb.Load (intObjDbFile);

			lockDb = new XmlDocument();
			lockDbFile = new FileStream(lockDbFilepath, FileMode.Open);
			lockDb.Load (lockDbFile);

			int maxNumOfRoom = 0;
			int maxNumOfFurn = 0;
			int roomNo = 0;
			if (!gen) {
				// get the seed and difficulty
				prng = new System.Random (menu.GetSeed ().GetHashCode ());
				// prng = new System.Random (seed.GetHashCode ());
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

				// initialize the allocators
				ra = new RoomAllocator (prng);
				ia = new InteractiveObjectAllocator (prng);

				// generation algorithm as follows:
				generateRoom ();
				generateFurniture (ra.rooms [ra.rooms.Count - 1], 0);
				while (ia.keys.Count < keyThreshold) {
					// printInfo ();
					roomNo = prng.Next(0, ra.rooms.Count);

					// 
					if ((ra.rooms [ra.rooms.Count - 1].fa.furnitures.Count) % maxNumOfFurn == 0 && 
						ra.rooms [ra.rooms.Count - 1].fa.furnitures.Count != 0) {
						if (ia.keys.Count != keyThreshold - 1) {
							generateRoom ();
							generateFurniture (ra.rooms [ra.rooms.Count - 1], 0);
						}
					}
					if ((ia.locations.Count + ia.locks.Count + ia.doors.Count - ia.interactiveObjects.Count) != 0) {
						if (generateKeyForLockDecision() == -1) {
							generateFurnitureDecision (ra.rooms [ra.rooms.Count - 1]);
						}
					} else {
						generateFurnitureDecision (ra.rooms [ra.rooms.Count - 1]);
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
					nextType.Add(node.InnerText);
				}
				thumbnail = intObjDb.GetElementsByTagName ("Thumbnail") [getNum].InnerText;
			} while ((type != "") && (type != intObjDb.GetElementsByTagName ("Type") [getNum].InnerText));
			return Resources.Load (intObjDb.GetElementsByTagName ("Path") [getNum].InnerText);
		}

		// function for generating a new room
		void generateRoom () {
			float doorOriginX, doorOriginZ, doorRotation;
			if (ra.AllocateRoom (out doorOriginX, out doorOriginZ, out doorRotation)) {
				generateDoor (doorOriginX, doorOriginZ, doorRotation);
				generateLock ("Door");
			}
		}

		// function called by generateRoom for generating a new door
		void generateDoor (float doorOriginX, float doorOriginZ, float doorRotation) {
			GameObject door = (GameObject)Instantiate (getDoorPrefab ());
			ia.AllocateDoor (doorOriginX, doorOriginZ, doorRotation, door);
			registerInteractiveObject (door);
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

		int generateKeyForLockDecision () {
			// for all locks
			for (int i = 0; i < ia.locks.Count; i++) {
				// check if any locks without a key
				if (!ia.locks [i].HasNext ()) {
					if (!generateIntObj ()) {
						return 1;
					}
					return 0;
				}
			}
			// lock not enough
			return -1;
		}

		// function for generating a new furniture decision
		void generateFurnitureDecision (Room room) {
			if (ra.rooms [ra.rooms.Count - 1].fa.furnitures.Count != 0) {
				generateFurniture (ra.rooms [ra.rooms.Count - 1], 1);
				generateLock ("Drawer");
			} else {
				generateFurniture (ra.rooms [ra.rooms.Count - 1], 0);
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
			if ((ia.locations.Count + ia.lockLocations.Count + ia.doors.Count - ia.interactiveObjects.Count) != 0) {
				if (ia.GetNextObjType (out chooseObj, out type)) {
					GameObject obj = (GameObject)Instantiate (getIntObjPrefab (type, out nextType, out thumbnail));
					if (ia.AllocatePickIntObj (chooseObj, obj, nextType, thumbnail)) {
						return true;
					} else {
						Destroy (obj);
						return false;
					}
				}
			}
			return false;
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
				else if (obj.transform.GetChild (i).CompareTag ("nonStaticObjPull") || obj.transform.GetChild (i).CompareTag ("nonStaticObjRotate"))
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