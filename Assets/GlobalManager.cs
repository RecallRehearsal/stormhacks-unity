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
    public AudioSource[] voices;
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
            apiManager = gameObject.AddComponent<ApiManager>();
            apiManager.audioSource = voices[0];
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
