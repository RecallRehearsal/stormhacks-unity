using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    private string apiUrl = "https://easy-fly-cleanly.ngrok-free.app/initialize";
    public AudioSource audioSource;

    public void FetchLearningGoals()
    {
        StartCoroutine(GetLearningGoals());
    }

    public void FetchQuestion()
    {
        StartCoroutine(FetchNextQuestion());
    }

    private IEnumerator FetchNextQuestion()
    {
        string url = "https://easy-fly-cleanly.ngrok-free.app/generateQuestionAudio";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("ngrok-skip-browser-warning", "true");
            //webRequest.certificateHandler = new BypassCertificate();

            Debug.Log("sent");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Status Code of GET FIRST: " + webRequest.responseCode);
                StartCoroutine(DownloadAudioClip("https://easy-fly-cleanly.ngrok-free.app/static/speech.mp3"));
            }
        }
    }

    IEnumerator DownloadAudioClip(string url)
    {
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result == UnityWebRequest.Result.ConnectionError || audioRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + audioRequest.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                if (audioClip == null)
                {
                    Debug.LogError("Failed to download audio clip.");
                }
                else
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }
        }
    }

    public void StartGame()
    {
        FetchQuestion();
    }

    private IEnumerator GetLearningGoals()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("GOT A RESPONSE FROM: " + apiUrl);

                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                //Debug.Log("Response JSON: " + jsonResponse);

                // Deserialize JSON to LearningGoalsWrapper
                LearningGoalsWrapper wrapper = null;
                try
                {
                    wrapper = JsonUtility.FromJson<LearningGoalsWrapper>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error parsing JSON: " + ex.Message);
                }

                // Check for null values in the wrapper
                if (wrapper == null)
                {
                    Debug.LogError("Error: Wrapper is null.");
                    yield break;
                }

                if (wrapper.names == null)
                {
                    Debug.LogError("Error: Wrapper names are null.");
                    yield break;
                }
                else
                {
                    Debug.Log("wrapper stuff: " + wrapper.names[0]);
                }

                if (wrapper.intro == null)
                {
                    Debug.LogError("Error: Wrapper intro is null.");
                    yield break;
                }
                else
                {
                    Debug.Log("second one" + wrapper.intro[0]);
                }

                if (wrapper.hard == null)
                {
                    Debug.LogError("Error: Wrapper hard is null.");
                    yield break;
                }

                // Check for array length consistency
                if (wrapper.names.Length != wrapper.intro.Length || wrapper.names.Length != wrapper.hard.Length)
                {
                    Debug.LogError("Error: The lengths of names, intro, and hard arrays do not match.");
                    Debug.Log($"Names length: {wrapper.names.Length}, Intro length: {wrapper.intro.Length}, Hard length: {wrapper.hard.Length}");
                    yield break;
                }

                Debug.Log("Wrapper parsed successfully and contains data.");
                //Debug.Log($"Names length: {wrapper.names.Length}");
                //Debug.Log($"Intro length: {wrapper.intro.Length}");
                //Debug.Log($"Hard length: {wrapper.hard.Length}");

                // Create LearningGoals object
                LearningGoals learningGoals = new LearningGoals
                {
                    learning_goals = new List<LearningGoal>()
                };

                // Map data from wrapper to learningGoals
                for (int i = 0; i < wrapper.names.Length; i++)
                {
                    LearningGoal learningGoal = new LearningGoal
                    {
                        name = wrapper.names[i],
                        introductoryQuestions = new List<string>(wrapper.intro[i].Split("$")),
                        hardQuestions = new List<string>(wrapper.hard[i].Split("$"))
                    };
                    learningGoals.learning_goals.Add(learningGoal);
                }

                // Update the GlobalManager with the fetched learning goals
                GlobalManager.Instance.learningGoals = learningGoals;

                // Optionally print the learning goals to verify
                //GlobalManager.Instance.PrintMessage();
            }
        }
    }
}