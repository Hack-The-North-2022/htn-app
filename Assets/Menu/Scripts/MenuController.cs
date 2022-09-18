using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using APIReq;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text codeText;
    float _time = 0f;
    bool _questionsRetrieved = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(APIReq.APIReqs.GetCode(codeText));
        
    }

    // Update is called once per frame
    void Update()
    {
        _time+=Time.deltaTime;
        if(_time >= 3) {
            StartCoroutine(APIReq.APIReqs.PollAuth());
            _time = 0;
        }
        if (Manager.DataManager.Instance.authenticated && !_questionsRetrieved) {
            StartCoroutine(APIReq.APIReqs.QuestionAudio());
            _questionsRetrieved = true;
        }
    }
}
