using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance;
        public List<APIReq.QuestionInfo> questions;

        public APIReq.CodeInfo code;
        public bool authenticated = false;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
    }
}
