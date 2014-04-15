using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	void OnGUI () {
		GUI.backgroundColor = new Color(253,74,0);
		if (GUI.Button (new Rect (280,240,400,100), "Start Game")) {
			Application.LoadLevel("Ingame");
		}
		if (GUI.Button (new Rect (280,380,400,100), "Exit Game")) {
			Application.Quit();
		}
	}
}