using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	public class RoomAllocator : Allocator {
		public List<Room> rooms;

		float x, z, originX, originZ, doorOriginX, doorOriginZ, doorPosition, doorRotation, localX, localZ;
		int linkedRoomNo, getSide;
		Vector2[] getRoom;

		public RoomAllocator (System.Random prng) : base (prng) {
			rooms = new List<Room> ();
			linkedRoomNo = -1;
		}

		public bool AllocateRoom (out float outDoorOriginX, out float outDoorOriginZ, out float outDoorRotation) {
			int trial = 0;
			outDoorOriginX = 0;
			outDoorOriginZ = 0;
			outDoorRotation = 0;
			x = prng.Next (7, 10);
			z = prng.Next (7, 10);
			if (allocatedSpace.Count == 0) {
				originX = 0;
				originZ = 0;
				doorOriginX = originX;
				doorOriginZ = originZ + z / 2f;
				doorPosition = originX;
				doorRotation = 180;
				localX = doorOriginX;
				localZ = doorOriginZ - 1;
				doorOriginX = doorOriginX + 0.5f;
			} else {
				do {
					linkedRoomNo = prng.Next (0, allocatedSpace.Count);
					getRoom = allocatedSpace [linkedRoomNo];
					getSide = prng.Next (0, 3);
					if (getSide == 0) {
						originX = getRoom [0].x - x / 2f;
						originZ = prng.Next (((int)getRoom [2].y + 1) * 10, ((int)getRoom [0].y - 1) * 10) / 10f + 0.5f;
						doorOriginX = originX + x / 2f;
						doorOriginZ = originZ;
						doorPosition = originZ;
						doorRotation = 90;
						localX = doorOriginX + 1;
						localZ = doorOriginZ;
						doorOriginZ = doorOriginZ + 0.5f;
					} else if (getSide == 1) {
						originX = prng.Next (((int)getRoom [0].x + 1) * 10, ((int)getRoom [1].x - 1) * 10) / 10f + 0.5f;
						originZ = getRoom [2].y - z / 2f;
						doorOriginX = originX;
						doorOriginZ = originZ + z / 2f;
						doorPosition = originX;
						doorRotation = 0;
						localX = doorOriginX;
						localZ = doorOriginZ + 1;
						doorOriginX = doorOriginX - 0.5f;
					} else if (getSide == 2) {
						originX = getRoom [1].x + x / 2f;
						originZ = prng.Next (((int)getRoom [2].y + 1) * 10, ((int)getRoom [0].y - 1) * 10) / 10f + 0.5f;
						doorOriginX = originX - x / 2f;
						doorOriginZ = originZ;
						doorPosition = originZ;
						doorRotation = 270;
						localX = doorOriginX - 1;
						localZ = doorOriginZ;
						doorOriginZ = doorOriginZ - 0.5f;
					} 
					//				else if (getSide == 3) {
					//					originX = prng.Next (((int)getRoom [0].x + 1) * 10, ((int)getRoom [1].x - 1) * 10) / 10f + 0.5f;
					//					originZ = getRoom [1].y + z / 2f;
					//					doorOriginX = originX;
					//					doorOriginZ = originZ + z / 2f;
					//					doorPosition = originX;
					//					doorRotation = 180;
					//					localX = doorOriginX;
					//					localZ = doorOriginZ - 1;
					//					doorOriginX = doorOriginX + 0.5f;
					//				}
					trial++;
				} while ((IsCollidedByValues (x, z, originX, originZ) || !rooms [linkedRoomNo].fa.IsInsideFloorByValues (1.5f, 1.5f, localX, localZ) || rooms [linkedRoomNo].fa.IsCollidedByValues (1.5f, 1.5f, localX, localZ)) && trial < 5);
			}
			if (linkedRoomNo == -1 || (!IsCollidedByValues (x, z, originX, originZ) && rooms [linkedRoomNo].fa.IsInsideFloorByValues (1.5f, 1.5f, localX, localZ) && !rooms [linkedRoomNo].fa.IsCollidedByValues (1.5f, 1.5f, localX, localZ))) {
				rooms.Add (new Room ("Room" + (rooms.Count + 1), x, z, originX, originZ, prng));
				allocatedSpace.Add (getPointsByValues (x, z, originX, originZ));

				// adding door hole
				if (linkedRoomNo == -1) {
					rooms [rooms.Count - 1].MakeDoorWall (3, doorPosition);
					rooms [rooms.Count - 1].fa.AllocateDoor (getSide, doorOriginX, doorOriginZ);
				} else {
					rooms [linkedRoomNo].MakeDoorWall (getSide, doorPosition);
					rooms [rooms.Count - 1].MakeDoorWall ((getSide + 2) % 4, doorPosition);
					rooms [linkedRoomNo].fa.AllocateDoor (getSide, doorOriginX, doorOriginZ);
					rooms [rooms.Count - 1].fa.AllocateDoor ((getSide + 2) % 4, doorOriginX, doorOriginZ);
				}
				outDoorOriginX = doorOriginX;
				outDoorOriginZ = doorOriginZ;
				outDoorRotation = doorRotation;
				return true;
			} else {
				return false;
			}
		}
	}
}