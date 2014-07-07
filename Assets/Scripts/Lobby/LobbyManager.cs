﻿using UnityEngine;
using System.Linq;
using System.Collections;

public class LobbyManager : MonoBehaviour {

	public ChatTextList textList;

	GameObject[] existingPlayers = new GameObject[]{};

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject p in players) {
			if (!(existingPlayers.Contains (p))) {
			
				if (Network.isServer) {
					Player player = (Player)p.GetComponent(typeof(Player));
					player.SetPlayerName("test player");
					textList.Add ("A Player has joined");
				}

			}
		}

		existingPlayers = players;
	}
}