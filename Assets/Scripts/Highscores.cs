using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;

public class Highscores : MonoBehaviour
{
    //public string username;
    //public short trackID;
    //public short kartSkinID;
    //public short charSkinID;
    //public short rank;
    //public Time time;
    //public Ghost ghostData;

    [SerializeField] InputField usernameInput;
    [SerializeField] InputField millisecondsInput;
    [SerializeField] Text scoreOutput;

    private void Awake()
    {
        DisplayScores();
    }

    public void SendScores()
    {
        StartCoroutine(DoPostScores(usernameInput.text, int.Parse(millisecondsInput.text)));
    }

    public void DisplayScores()
    {
        StartCoroutine(DoDisplayScores());
    }

    private const string highscoreURL = "https://quadrilateral-veter.000webhostapp.com/getsenddata.php";

    IEnumerator DoDisplayScores()
    {
        scoreOutput.text = "";

        List<Score> scores = new List<Score>();

        yield return StartCoroutine(DoRetrieveScores(scores));

        foreach (Score score in scores)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(score.score);
            scoreOutput.text += time.Minutes + "\'" + time.Seconds + "\"" + time.Milliseconds + " - " + score.name + "\n";
        }
    }

    IEnumerator DoRetrieveScores(List<Score> scores)
    {
        WWWForm form = new WWWForm();
        form.AddField("retrieve_leaderboard", "true");

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
                using (StringReader reader = new StringReader(contents))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Score entry = new Score();
                        entry.name = line;
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

    IEnumerator DoPostScores(string name, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("post_leaderboard", "true");
        form.AddField("username", name);
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
            }
        }
    }
}

public struct Score
{
    public string name;
    public int score;
}