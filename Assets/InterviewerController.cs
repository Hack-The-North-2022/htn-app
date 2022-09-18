using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterviewerController : MonoBehaviour
{
    public AudioSource interviewerAudioSource;
    public InterviewRecorder interviewRecorder;
    public GestureScript gestureScript;
    public PersistentToggleDot eyeContactScript;
    public GameObject RecordingUI;

    ControllerInputHandler cInput;
    string _state = "";
    private int _questionIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        cInput = ControllerInputHandler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.DataManager.Instance.questions != null && Manager.DataManager.Instance.questions.Count != 0) {
            if (_state == "") {
                if (_questionIndex == Manager.DataManager.Instance.questions.Count) {
                    Debug.Log("Completed");
                    gestureScript.StopRecording();
                    eyeContactScript.StopRecording();                    
                } else {
                    if (_questionIndex == 0) {
                        gestureScript.StartRecording();
                        eyeContactScript.StartRecording();
                    }
                    _state = "saying";
                    StartCoroutine(SayQuestion(Manager.DataManager.Instance.questions[_questionIndex]));
                }
            }
            if (_state == "response") {
                interviewRecorder.StartRecording();
                RecordingUI.SetActive(true);
                StartCoroutine(ListenToSpeaker());
            }
        }
        if (interviewRecorder.recording && cInput.LeftCon.X.wasButtonPressedLastFrame) {
            interviewRecorder.StopRecording();
        }
    }

    IEnumerator SayQuestion(APIReq.QuestionInfo audio) {
        AudioClip ac = APIReq.APIReqs.RetrieveAudio(audio);
        interviewerAudioSource.clip = ac;
        interviewerAudioSource.Play();
        yield return new WaitUntil(() => interviewerAudioSource.isPlaying == false);
        _state = "response";
    }

    IEnumerator ListenToSpeaker() {
        yield return new WaitUntil(() => interviewRecorder.recording == false);
        RecordingUI.SetActive(false);
        _questionIndex += 1;
        _state = "";
    }
    
}
