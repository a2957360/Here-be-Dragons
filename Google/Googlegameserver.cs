using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class Googlegameserver : MonoBehaviour {


    void Start ()
    {
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate ();
        LogIn();
        //Addacheivement(GPGSIds.achievement_start_the_game);
    }

    public void updateacheviement()
    {

    }

    public void LogIn ()
    {
        Social.localUser.Authenticate ((bool success) =>
        {
            if (success) {
                Debug.Log ("Login Sucess");
            } else {
                Debug.Log ("Login failed");
            }
        });
    }

    public void OnShowLeaderBoard ()
    {
        //Social.ShowLeaderboardUI(); // Show all leaderboard
        ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI (GPGSIds.leaderboard_hero_scoreboard); // Show current (Active) leaderboard
    }

    public static void OnAddScoreToLeaderBorad (int score)
    {
        if (Social.localUser.authenticated) {
            Social.ReportScore (score, GPGSIds.leaderboard_hero_scoreboard, (bool success) =>
            {
                if (success) {
                    Debug.Log ("Update Score Success");
                    
                } else {
                    Debug.Log ("Update Score Fail");
                }
            });
        }
    }

    public static void Addacheivement(string name)
    {
        if (Social.localUser.authenticated)
        {
            Social.ReportProgress(name, 100.0f, (bool success) =>
            {
                if (success)
                {
                    Debug.Log("Update Achement Success");

                }
                else
                {
                    Debug.Log("Update Achement Fail");
                }
            });
        }
    }

    public void OnShowAcheivement()
    {
        //Social.ShowLeaderboardUI(); // Show all leaderboard
        if (Social.localUser.authenticated)
        {
            Social.ShowAchievementsUI();
        }
    }

}
