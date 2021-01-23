using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    private const string leaderboardURL = "https://quadrilateral-veter.000webhostapp.com/getsenddata.php";

    [SerializeField] InputField usernameInput = null;
    [SerializeField] InputField millisecondsInput = null;
    [SerializeField] Text scoreOutput = null;

    string ghostFilePath = "GhostData\\dummy.gvg";

    private void Awake()
    {
        DisplayScores(0);
    }

    //------Testing button functions------//

    public void SendScores(int trackID)
    {
        StartCoroutine(DoPostScores(trackID, usernameInput.text, 0, 0, int.Parse(millisecondsInput.text)));
    }

    public void DisplayScores(int trackID)
    {
        StartCoroutine(DoDisplayScores(trackID));
    }

    public void DownloadTestGhost()
    {
        StartCoroutine(DoRetrieveGhost(0, 30));
    }

    //------Accessing server coroutines------//

    IEnumerator DoDisplayScores(int trackID)
    {
        scoreOutput.text = "";
        List<LeaderboardEntry> players = new List<LeaderboardEntry>();

        // wait for data to be retrieved from server
        yield return StartCoroutine(DoRetrieveScores(trackID, players));

        // display data
        int count = players.Count < 10 ? players.Count : 10; // limit display to top 10, or players.Count if less than 10 entries exist
        for (int i = 0; i < count; ++i)
        {
            // convert milliseconds to timespan
            TimeSpan time = TimeSpan.FromMilliseconds(players[i].time);

            // display: 0'00"000 (time) - username
            scoreOutput.text += time.Minutes + "\'" + time.Seconds + "\"" + time.Milliseconds + " - " + players[i].username + "\n";
        }
    }

    IEnumerator DoRetrieveScores(int trackID, List<LeaderboardEntry> data)
    {
        WWWForm form = new WWWForm();
        form.AddField("retrieve_leaderboard", "true");
        form.AddField("trackID", trackID);

        using (UnityWebRequest www = UnityWebRequest.Post(leaderboardURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Successfully accessed database");

                string contents = www.downloadHandler.text;
                Debug.Log(contents);

                using (StringReader reader = new StringReader(contents))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        LeaderboardEntry entry = new LeaderboardEntry();

                        try
                        {
                            entry.ID = Int32.Parse(line);
                            entry.username = reader.ReadLine();
                            entry.kartSkinID = Int32.Parse(reader.ReadLine());
                            entry.ID = Int32.Parse(reader.ReadLine());
                            entry.time = Int32.Parse(reader.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Invalid data: " + e);
                        }

                        data.Add(entry);
                    }
                }
            }
        }
    }

    IEnumerator DoPostScores(int trackID, string name, int kartID, int charID, int score)
    {
        string ghostData = File.ReadAllText(ghostFilePath);

        WWWForm form = new WWWForm();
        form.AddField("post_leaderboard", "true");
        form.AddField("trackID", trackID);
        form.AddField("username", name);
        form.AddField("kartSkinID", kartID);
        form.AddField("charSkinID", charID);
        form.AddField("time", score);
        form.AddField("ghostData", ghostData);

        using (UnityWebRequest www = UnityWebRequest.Post(leaderboardURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Successfully accessed database");

                //string debugInfo = www.downloadHandler.text;
                //Debug.Log(debugInfo);
            }
        }
    }

    IEnumerator DoRetrieveGhost(int trackID, int entryID)
    {
        WWWForm form = new WWWForm();
        form.AddField("retrieve_ghostfile", "true");
        form.AddField("trackID", trackID);
        form.AddField("ID", entryID);

        using (UnityWebRequest www = UnityWebRequest.Post(leaderboardURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Successfully accessed database");
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
}

// For temporary local storage of data
public struct LeaderboardEntry
{
    public int ID;
    public string username;
    public int kartSkinID;
    public int charSkinID;
    public int time;
    public string ghostFilePath;
}