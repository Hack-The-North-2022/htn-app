using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using APIReq;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Text codeText;
    float _time = 0f;
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

        
    }
}
