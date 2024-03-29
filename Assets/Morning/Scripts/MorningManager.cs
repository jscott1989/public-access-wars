﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MorningManager : SceneManager {
	NetworkManager mNetworkManager;
	DialogueManager mDialogueManager;
	QuestionPanel mQuestionPanel;
	Manager mManager;

	string[] mGoodDialogues = new string[]{
		"Wow! That was one of our best ever nights. Keep it up!",
		"That was a really strong showing!",
		"We're showing the others how it's done!",
		"Keep doing that and I think we can renew the show for a full season!",
		"Brilliant!"
	};

	string[] mBadDialogues = new string[]{
		"That was a little bit worse than expected.",
		"You're really going to have to do something to turn this around.",
		"We can't accept many more results like this.",
		"This is just as bad as the results the last show was getting.",
		"This is embarrasing.",
		"I can't believe so few people tuned in. And after all huge publicity push!"
	};

	string[] mMiddleDialogues = new string[]{
		"That was okay.",
		"That's in line with what we expected.",
		"That's about right for a station of our size.",
		"I think we can do better, but I can accept this.",
		"Let's aim bigger next time.",
		"We're in the middle of the pack, but I want to be in front!"
	};

	public string uCurrentDay {
		get {
			if (mNetworkManager == null || mNetworkManager.myPlayer == null) {
				return "";
			}
			if (mNetworkManager.myPlayer.uDay > Game.NUMBER_OF_DAYS) {
				return "";
			}
			return "Day " + mNetworkManager.myPlayer.uDay.ToString () + " of " + Game.NUMBER_OF_DAYS.ToString ();
		}
	}

	public Texture uStationLogo {
		get {
			if (mNetworkManager == null || mNetworkManager.myPlayer == null) {
				return null;
			}
			return mNetworkManager.myPlayer.uStationLogo;
		}
	}

	public string uWelcomeMessage {
		get {
			if (mNetworkManager == null || mNetworkManager.myPlayer == null) {
				return "";
			}
			return "Welcome to " + mNetworkManager.myPlayer.uStationName;
		}
	}

	void Awake() {
		mNetworkManager = FindObjectOfType<NetworkManager>();
		mDialogueManager = FindObjectOfType<DialogueManager>();
		mQuestionPanel = FindObjectOfType<QuestionPanel>();
		mManager = FindObjectOfType<Manager>();
	}

	void Start () {
		// First we need to set everyone to "Not Ready"
		if (Network.isServer) {
			foreach (Player player in mNetworkManager.players) {
				player.networkView.RPC ("SetReady", RPCMode.All, false);
			}
		}

		if (mNetworkManager.myPlayer.uDay == 1) {
			FirstDay();
		} else if(mNetworkManager.myPlayer.uDay < Game.NUMBER_OF_DAYS) {
			MiddleDay();
		} else if(mNetworkManager.myPlayer.uDay > Game.NUMBER_OF_DAYS) {
			GameOver ();
		} else {
			LastDay();
		}
	}

	void FirstDay() {
		string[] day1FirstDialogue = new string[]{
			"This is " + mNetworkManager.myPlayer.uSelectedStation.uName + ". The public access TV station you've worked at for several years",
			"Here comes your boss, " + mNetworkManager.myPlayer.uBossName
		};

		Action day1FirstDialogueFinished =
		() => {

			Action managerArrived =
				() => {
					print ("ARRIVED");
					Action day1DialogueFinished =
					() => {
						Action<string> showNameSelected =
						(string pShowName) => {
							mNetworkManager.myPlayer.networkView.RPC ("SetShowName", RPCMode.All, pShowName);
							
							string[] dialogue = new string[] {
								"Great. " + pShowName + "! I like it!",
								"You have " + Game.NUMBER_OF_DAYS.ToString () + " days to save your job and win the ratings war. Don't let me down!"
							};
							
							
							Action dialogueFinished =
							() => {
								mDialogueManager.WaitForReady();
							};
							
							mDialogueManager.StartDialogue(dialogue, dialogueFinished);
						};
						
						mQuestionPanel.AskQuestion ("The show will be \"" + mNetworkManager.myPlayer.uTheme + "\"\n\nWhat shall we call the new show?", showNameSelected);
						
					};
					
					System.Random rnd = new System.Random();
					
					string otherStation = mNetworkManager.players.Where (p => p.uID != mNetworkManager.mMyClientID).Select (p => p.uStationName).OrderBy(x => rnd.Next()).FirstOrDefault();
					if (otherStation == null) {
						// Should only happen when debugging with one player
						otherStation = "The other stations";
					}
					
					string[] day1SecondDialogue = new string[]{
						"Good morning, " + mNetworkManager.myPlayer.uName + "!",
						"Listen... I've been meaning to talk to you",
						"You see, things aren't going so great at " + mNetworkManager.myPlayer.uSelectedStation.uName,
						otherStation + " are killing us in the ratings",
						"The bosses... they wanted to let you go... " + mNetworkManager.myPlayer.uOldTheme + " just isn't doing it any more",
						"We're trying to recreate " + mNetworkManager.myPlayer.uSelectedStation.uName + " to be new, exciting, vibrant",
						"You've got one week to turn it around - and we're giving you a new show to run",
						"Our market research suggests that " + mNetworkManager.myPlayer.uTheme + " would really be a big hit with modern audiences",
					};
					
					mDialogueManager.StartDialogue(day1SecondDialogue, day1DialogueFinished);
			};
			mManager.Show (managerArrived);
		};

		if (Game.DEBUG_MODE) {
			mNetworkManager.myPlayer.networkView.RPC ("SetShowName", RPCMode.All, "TempShowName");
			mDialogueManager.WaitForReady();
		} else {
			mDialogueManager.StartDialogue (day1FirstDialogue, day1FirstDialogueFinished);
		}
	}

	void MiddleDay() {
		Action dialogueFinished =
			() => {
			mDialogueManager.WaitForReady();
		};

		if (Game.DEBUG_MODE) {
			mDialogueManager.WaitForReady();
		} else {
			Action s =
				() => {
					Action feedbackGiven =
						() => {
							string[] dialogue = new string[] {
								"That's " + (mNetworkManager.myPlayer.uDay - 1).ToString () + " days gone. " + (Game.NUMBER_OF_DAYS - (mNetworkManager.myPlayer.uDay - 1)).ToString() + " to go."
							};

							mDialogueManager.StartDialogue (dialogue, dialogueFinished);
					};
					GiveFeedback(feedbackGiven);
			};
			mManager.Show (s);
		}
	}

	void LastDay() {
		Action s =
		() => {
			Action firstDialogueFinished =
				() => {
					Action dialogueFinished =
						() => {
					mDialogueManager.WaitForReady();
					};

					string[] lastDialogue = new string[] {
						"This is the last day. One last chance to prove to " + mNetworkManager.myPlayer.uStationName + " that you can make shows that the public want to see.",
						"Don't let us down."
					};

					mDialogueManager.StartDialogue(lastDialogue, dialogueFinished);
			};

			if (Game.DEBUG_MODE) {
				mDialogueManager.WaitForReady();
			} else {
				GiveFeedback(firstDialogueFinished);
			}
		};
		mManager.Show (s);
	}

	void GameOver() {
		// Calculate the final day's scores
		int[] data = mNetworkManager.myPlayer.GenerateLatestViewerData();
		// Add today's total viewer seconds to the score record
		// TODO: This isn't a very good scoring mechanism - replace this with something more accurate
		mNetworkManager.myPlayer.networkView.RPC ("AddDailyCreatorScore", RPCMode.All, (data.Sum () * Game.CREATOR_SCORE_MULTIPLIER) .ToString ());

		List<Player> positions = mNetworkManager.players.OrderBy (p => p.uOverallScore).Reverse().ToList();
		int position = positions.IndexOf (mNetworkManager.myPlayer);

		string[] positionStrings = new string[]{"1st","2nd","3rd","4th","5th","6th","7th","8th","9th","10th"};

		string[] dialogue;

		if (position == 0) {
			dialogue = new string[]{
				"We won the ratings war! Brilliant!",
				"Well done. I think we can renew " + mNetworkManager.myPlayer.uShowName + " now!"
			};
		} else if (position == (mNetworkManager.players.Length - 1)) {
			dialogue = new string[]{
				"LAST? I put my neck on the line for you and this is how you repay me?",
				mNetworkManager.myPlayer.uShowName + " is cancelled. And you're fired.",
				"You'll never work in public access again!"
			};
		} else if (position >= (mNetworkManager.players.Length/2)) {
			dialogue = new string[] {
				"We came " + positionStrings[position] + ". That was poor.",
				"I'm sure you've guessed we won't be renewing your contract.",
				"Why you ever thought " + mNetworkManager.myPlayer.uTheme + " could work, I'll never know."
			};
		} else {
			dialogue = new string[] {
				"We came " + positionStrings[position] + ". It's not bad, but it's not good enough.",
				"We're going to take " + mNetworkManager.myPlayer.uShowName + " forward under a new showrunner. We're hoping to land " + positions[0].uName + ".",
				"Best of luck for the future though. If you need a reference, just ask."
			};
		}

		Action dialogueComplete =
			() => {
				mDialogueManager.WaitForReady();
		};

		mDialogueManager.StartDialogue (dialogue, dialogueComplete);
	}

	void GiveFeedback(Action callback) {
		int myViewers = 0;
		int otherViewers = 0;

		foreach(Player p in mNetworkManager.players) {
			foreach(WatchedStationAction a in p.uWatchedStationActions) {
				float endTime = a.uEndTime;
				if (endTime == -1) {
					endTime = 31;
				}
				// Check each second if it falls inside the watched time
				for(int i = 0; i < 30; i++) {
					if ((i >= a.uStartTime && i <= endTime) && (i + 1 >= a.uStartTime && i <= endTime)) {
						if (a.uPlayer == mNetworkManager.myPlayer) {
							myViewers += 1;
						} else {
							otherViewers += 1;
						}
					}
				}
			}
		}

		float myShare = ((float)myViewers)/(float)(myViewers + otherViewers);
		float expectedShare = 1f/((float)mNetworkManager.players.Length);

		string[] possibleDialogues;

		if (myShare <= expectedShare - 0.1) {
			// We did poorly
			possibleDialogues = mBadDialogues;
		} else if (myShare >= expectedShare + 0.1) {
			// We did well
			possibleDialogues = mGoodDialogues;
		} else {
			// We did okay
			possibleDialogues = mMiddleDialogues;
		}

		System.Random rnd = new System.Random();
		string[] dialogue = new string[]{
			"Last night, we got " + (myShare * 100).ToString ("0.00") + "% of the audience share.",
			possibleDialogues[rnd.Next (possibleDialogues.Length - 1)]
		};
		mDialogueManager.StartDialogue (dialogue, callback);
	}

	public override void AllReady() {
		networkView.RPC ("MoveToNextScene", RPCMode.All);
	}

	/**
	 * Move to the prop selection scene
	 */
	[RPC] void MoveToNextScene() {
		mDialogueManager.EndDialogue();
		if (mNetworkManager.myPlayer.uDay == 1) {
			mNetworkManager.LoadLevel ("PropSelection");
		} else if (mNetworkManager.myPlayer.uDay > Game.NUMBER_OF_DAYS) {
			mNetworkManager.LoadLevel("EndOfGame");
		} else {
			mNetworkManager.LoadLevel("Feedback");
		}
	}
}
