using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class InterviewRecorder : MonoBehaviour
{
    bool ready=false;
    public bool recording = false;
    string microphone = "MacBook Pro Microphone";
    AudioSource audioSource;
    void Start()
    {
        /* Application.RequestUserAuthorization(UserAuthorization.Microphone); */
        /* AudioSource audioSource = GetComponent<AudioSource>(); */
        /* audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100); */
        /* audioSource.Play(); */
        StartCoroutine(rua());
        //StartCoroutine(APIReq.APIReqs.QuestionAudio());
        audioSource = GetComponent<AudioSource>();


        
    }
    public void StartRecording(){
        if(ready&&!recording){
            recording = true;
            audioSource.clip = Microphone.Start(microphone, true, 10, 44100);
        }
        
    }
    public void StopRecording(){
        recording = false;

        Microphone.End(microphone);

        /* GetAudioBytes(audioSource.clip); */


        SavWav.Save("temp.wav",audioSource.clip);
        var filepath = Path.Combine(Application.persistentDataPath, "temp.wav");

        StartCoroutine(APIReq.APIReqs.SendAudio(File.ReadAllBytes(filepath)));
    }
    /* public byte[] GetAudioBytes(AudioClip audioClip) */
    /* { */
    /*     var samples = new float[audioClip.samples]; */

    /*     audioClip.GetData(samples, 0); */

    /*     Int16[] intData = new Int16[samples.Length]; */

    /*     Byte[] bytesData = new Byte[samples.Length * 2]; */

    /*     int rescaleFactor = 32767; */

    /*     for (int i = 0; i < samples.Length; i++) */
    /*     { */
    /*         intData[i] = (short)(samples[i] * rescaleFactor); */
    /*         Byte[] byteArr = new Byte[2]; */
    /*         byteArr = BitConverter.GetBytes(intData[i]); */
    /*         byteArr.CopyTo(bytesData, i * 2); */
    /*     } */

    /*     return bytesData; */

    /* } */

    void Update()
    {


        
        
    }
    IEnumerator rua(){
        FindMicrophones();
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Microphone found");
            ready = true;
        }
        else
        {
            Debug.Log("Microphone not found");
        }
    }
    void FindMicrophones()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}
