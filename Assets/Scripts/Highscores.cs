using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;

public class Highscores : MonoBehaviour
{
    private const string highscoreURL = "https://quadrilateral-veter.000webhostapp.com/getsenddata.php";

    [SerializeField] InputField usernameInput = null;
    [SerializeField] InputField millisecondsInput = null;
    [SerializeField] Text scoreOutput = null;

    private void Awake()
    {
        DisplayScores("0");
    }

    public void SendScores(string trackID)
    {
        StartCoroutine(DoPostScores(trackID, usernameInput.text, int.Parse(millisecondsInput.text)));
    }

    public void DisplayScores(string trackID)
    {
        StartCoroutine(DoDisplayScores(trackID));
    }

    IEnumerator DoDisplayScores(string trackID)
    {
        scoreOutput.text = "";

        List<Score> scores = new List<Score>();

        yield return StartCoroutine(DoRetrieveScores(trackID, scores));

        foreach (Score score in scores)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(score.score);
            scoreOutput.text += time.Minutes + "\'" + time.Seconds + "\"" + time.Milliseconds + " - " + score.name + "\n";
        }
    }

    IEnumerator DoRetrieveScores(string trackID, List<Score> scores)
    {
        WWWForm form = new WWWForm();
        form.AddField("retrieve_leaderboard_" + trackID, "true");

        using (UnityWebRequest www = UnityWebRequest.Post(highscoreURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Successfully retrieved scores!");
                string contents = www.downloadHandler.text;
                Debug.Log(contents);

                using (StringReader reader = new StringReader(contents))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Score entry = new Score();
                        entry.name = line;

                        // skip IDs
                        reader.ReadLine();
                        reader.ReadLine();

                        try
                        {
                            entry.score = Int32.Parse(reader.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Invalid score: " + e);
                            continue;
                        }

                        scores.Add(entry);
                    }
                }
            }
        }
    }

    IEnumerator DoPostScores(string trackID, string name, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("post_leaderboard_" + trackID, "true");
        form.AddField("username", name);
        form.AddField("kartSkinID", 0);
        form.AddField("charSkinID", 0);
        form.AddField("time", score);

        using (UnityWebRequest www = UnityWebRequest.Post(highscoreURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Successfully posted score!");

                //debug stuff
                //string contents = www.downloadHandler.text;
                //Debug.Log(contents);
            }
        }
    }
}

public struct Score
{
    public string name;
    public int score;
}