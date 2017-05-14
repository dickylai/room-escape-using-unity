using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace RoomEscape {
	public class Menu : MonoBehaviour {
		static string seed;
		static int difficulty;
		static bool success;
		public Text congrat;

		void Start () {
			if (SceneManager.GetActiveScene ().name == "Menu") {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				seed = "";
				difficulty = 0;
				if (success)
					congrat.text = "Congratulations!";
				else
					congrat.text = "";
				success = false;
			}
		}

		public void SetSeed(string input) {
			seed = input;
		} 

		public void SetDifficulty(int input) {
			difficulty = input;
		}

		public void GoToGame() {
			SceneManager.LoadScene ("Generating");
		}

		public string GetSeed () {
			return seed;
		}

		public int GetDifficulty () {
			return difficulty;
		}

		public void PlayWin () {
			success = true;
		}
	}
}