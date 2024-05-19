using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    // our global variables
    public int questionCounter;
    public int learningGoalCounter;
    public bool correct;
    public LearningGoals learningGoals;
    [SerializeField]
    public AudioSource voice;
    [SerializeField]
    public GameObject[] lights;
    public Button correctButton;

    public ApiManager apiManager;

    public void PlayQuestion()
    {
        apiManager.FetchQuestion();
    }

    private void Awake()
    {
        // see if there is already an instance of this class
        if (Instance == null)
        {
            // If not, set it to this instance
            Instance = this;
            // Make this instance persistent between scenes
            DontDestroyOnLoad(gameObject);
            this.questionCounter = 0;
            this.learningGoalCounter = 0;
            this.correct = false;
            SetActiveSpeaker(0);
            Debug.Log(voice);
            apiManager = gameObject.AddComponent<ApiManager>();
            apiManager.audioSource = voice;
            apiManager.FetchLearningGoals();
            //Debug.Log("right before the fetching");
            //apiManager.FetchFirstQuestion();
        }
        else
        {
            // If an instance already exists, destroy this new one
            Destroy(gameObject);
        }
    }

    public void SetActiveSpeaker(int index)
    {
        for (int i = 0; i<3; i++)
        {
            if (i == index)
            {
                lights[i].SetActive(true);
            } else
            {
                lights[i].SetActive(false);
            }
        }
    }

    public void SetCorrect(bool val)
    {
        Debug.Log("setting the val: " + val);
        this.correct = val;
    }

    public void PrintMessage()
    {
        if (learningGoals != null)
        {
            Debug.Log(JsonUtility.ToJson(learningGoals, true));
        }
        else
        {
            Debug.Log("Learning goals not loaded yet.");
        }
    }

    void Update()
    {
        if (questionCounter < 2)
        {
            SetActiveSpeaker(0);
        }
        else
        {
            SetActiveSpeaker(1);
        }

        if (this.correct)
        {
            correctButton.gameObject.SetActive(true);
        }
        else
        {
            correctButton.gameObject.SetActive(false);
        }
    }
}

[System.Serializable]
public class LearningGoals
{
    public List<LearningGoal> learning_goals;
}

[System.Serializable]
public class LearningGoal
{
    public string name;
    public List<string> introductoryQuestions;
    public List<string> hardQuestions;
}

[System.Serializable]
public class LearningGoalsWrapper
{
    public string[] names;
    public string[] intro;
    public string[] hard;
}
