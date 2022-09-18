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
    public Animator joeAnimator;
    public GameObject finalMessage;

    ControllerInputHandler cInput;
    string _state = "";
    private int _questionIndex = 0;
    private AudioClip _completeInterview;
    // Start is called before the first frame update
    void Start()
    {
        cInput = ControllerInputHandler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_state);
        if (Manager.DataManager.Instance.questions != null && Manager.DataManager.Instance.questions.Count != 0) {
            if (_state == "") {
                if (_questionIndex == Manager.DataManager.Instance.questions.Count) {
                    finalMessage.SetActive(true);
                    Debug.Log("Final Statement");
                    gestureScript.StopRecording();
                    eyeContactScript.StopRecording();                    
                } else {
                    if (_questionIndex == 0) {
                        gestureScript.StartRecording();
                        eyeContactScript.StartRecording();
                    }
                    joeAnimator.SetBool("Talking", true);
                    _state = "saying";
                    StartCoroutine(SayQuestion(Manager.DataManager.Instance.questions[_questionIndex]));
                }
            }
            if (_state == "response") {
                if (!interviewRecorder.recording  && cInput.LeftCon.X.wasButtonPressedLastFrame) {
                    interviewRecorder.StartRecording();
                    RecordingUI.SetActive(true);
                    StartCoroutine(ListenToSpeaker());
                } else if (cInput.LeftCon.X.wasButtonPressedLastFrame) {
                    interviewRecorder.StopRecording(_questionIndex);
                }
            }
        }
    }

    IEnumerator SayQuestion(APIReq.QuestionInfo audio) {
        string ac = APIReq.APIReqs.RetrieveAudio(audio);
        WWW loader = new WWW("file://" + ac);
        yield return loader;
        
        AudioClip clip = loader.GetAudioClip(false, false, AudioType.MPEG);
        Debug.Log(clip);
        interviewerAudioSource.clip = clip;
        interviewerAudioSource.Play();
        yield return new WaitUntil(() => interviewerAudioSource.isPlaying == false);
        joeAnimator.SetBool("Talking", false);
        _state = "response";
    }

    IEnumerator ListenToSpeaker() {
        yield return new WaitUntil(() => interviewRecorder.recording == false);
        RecordingUI.SetActive(false);
        _questionIndex += 1;
        _state = "";
    }
    
}
