using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;

public class AudioRecorder : MonoBehaviour
{
    private bool isRecording = false;
    private AudioClip recording;
    private string microphoneDevice = null;

    public Button recordButton;
    public TextMeshProUGUI buttonText;
    public AudioSource audioSource;
    public AudioSource audioSource2;
    //private ApiManager apiManager;

    // Start is called before the first frame update
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            Debug.Log("using microphone: " + microphoneDevice);
        }
        else
        {
            Debug.Log("No microphone devices found");
        }
    }

    public void ToggleRecording()
    {
        if (isRecording)
        {
            StartCoroutine(StopRecording());
        }
        else
        {
            StartRecording();
        }
    }

    IEnumerator StopRecording()
    {
        if (!isRecording) yield break;

        buttonText.text = "Start Recording";
        Microphone.End(microphoneDevice);
        isRecording = false;
        Debug.Log("recording stopped");

        //PlayRecording();

        byte[] wavData = AudioClipToWAV(recording);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", wavData, "recording.wav", "audio/wav")
        };

        // endpoint info
        string url = "https://easy-fly-cleanly.ngrok-free.app/processAnswer";
        Debug.Log("URL IS " + url + " before the req");

        //using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, formData))
        {
            webRequest.SetRequestHeader("ngrok-skip-browser-warning", "true");
            webRequest.certificateHandler = new BypassCertificate();

            Debug.Log("sent");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Status Code: " + webRequest.responseCode);
                Debug.Log("HERE BYRON");
                StartCoroutine(DownloadAudioClip("https://easy-fly-cleanly.ngrok-free.app/static/speech.mp3"));
                //Debug.Log("after the call");

                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response JSON: " + jsonResponse);

                // Deserialize JSON to LearningGoalsWrapper
                ProcessedResult res = null;
                try
                {
                    res = JsonUtility.FromJson<ProcessedResult>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error parsing JSON: " + ex.Message);
                }

                Debug.Log(res.correctness + " is correctness");
                if (res.correctness > 60)
                {
                    if (GlobalManager.Instance.questionCounter > 4)
                    {
                        GlobalManager.Instance.questionCounter = 0;
                        GlobalManager.Instance.learningGoalCounter++;
                    }
                    else
                    {
                        GlobalManager.Instance.questionCounter++;
                    }
                    GlobalManager.Instance.correct = true;
                }
            }
        }

    }

    // https://easy-fly-cleanly.ngrok-free.app/static/speech.mp3

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
                    audioSource2.clip = audioClip;
                    Debug.Log("the thing PLAYS");
                    audioSource2.Play();
                    Debug.Log("AFTER AFTER");
                    
                    //GlobalManager.Instance.apiManager.FetchQuestion();
                }
            }
        }
    }

    [System.Serializable]
    public class ProcessedResult
    {
        public int correctness;
    }

    // overriding our certificate class
    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // Simply accept all certificates
        }
    }

    void StartRecording()
    {
        if (microphoneDevice == null)
        {
            Debug.LogError("microphone device not found");
            return;
        }

        // max 10 second duration + sample rate of 44100 Hz
        buttonText.text = "Stop Recording";
        recording = Microphone.Start(microphoneDevice, true, 20, 44100);
        isRecording = true;
        Debug.Log("Recording started...");
    }

    void PlayRecording()
    {
        if (recording == null)
        {
            Debug.Log("no recording to play");
            return;
        }

        audioSource.clip = recording;
        audioSource.Play();
    }


    // Update is called once per frame
    void Update()
    {

    }

    // generate a wav file from an audio clip
    public static byte[] AudioClipToWAV(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        var sampleCount = samples.Length;
        var byteCount = sampleCount * 2;
        var headerSize = 44;

        var bytes = new byte[byteCount + headerSize];

        System.Buffer.BlockCopy(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, bytes, 0, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(byteCount + headerSize - 8), 0, bytes, 4, 4);
        System.Buffer.BlockCopy(System.Text.Encoding.UTF8.GetBytes("WAVEfmt "), 0, bytes, 8, 8);
        System.Buffer.BlockCopy(BitConverter.GetBytes(16), 0, bytes, 16, 4);  // PCM chunk size
        System.Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, bytes, 20, 2);  // format (PCM)
        System.Buffer.BlockCopy(BitConverter.GetBytes((short)clip.channels), 0, bytes, 22, 2);
        System.Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency), 0, bytes, 24, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, bytes, 28, 4);  // byte rate
        System.Buffer.BlockCopy(BitConverter.GetBytes((short)(clip.channels * 2)), 0, bytes, 32, 2);  // block align
        System.Buffer.BlockCopy(BitConverter.GetBytes((short)16), 0, bytes, 34, 2);  // bits per sample
        System.Buffer.BlockCopy(System.Text.Encoding.UTF8.GetBytes("data"), 0, bytes, 36, 4);
        System.Buffer.BlockCopy(BitConverter.GetBytes(byteCount), 0, bytes, 40, 4);

        // Convert float array to 16-bit PCM
        for (int i = 0, j = headerSize; i < sampleCount; i++, j += 2)
        {
            short value = (short)(samples[i] * short.MaxValue);
            System.Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, j, 2);
        }

        return bytes;
    }

}
