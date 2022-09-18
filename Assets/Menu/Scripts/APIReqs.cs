using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Manager;
using System.Text;
using System;
using System.IO;
using TMPro;
namespace APIReq
{
    [System.Serializable]
    public class GestureInfo{
        string code;
        float gestureCount;
        float amountInStrikeZone;
        float duration;
        public GestureInfo(string code,float gestureCount, float amountInStrikeZone, float duration){
            this.gestureCount = gestureCount;
            this.amountInStrikeZone = amountInStrikeZone;
            this.duration = duration;

        }
    }
    [System.Serializable]
    public class AdHawkInfo
    {
        string code;
        Dictionary<string,float> data; 
        public AdHawkInfo(string code,Dictionary<string,float> data){
            this.code = code;
            this.data = data;
        }

    }
    [System.Serializable]
    public class CodeInfo
    {
        public string code;
    }
    [System.Serializable]
    public class AuthInfo
    {
        public bool success;
    }
    [System.Serializable]
    public class QuestionInfos
    {
        public List<QuestionInfo> questions;
    }
    [System.Serializable]
    public class QuestionInfo
    {
        public string text;
        public string audio;
        public string question_id;
    }
    [System.Serializable]
    public class AudioInfo
    {
        public string audio;
        public string text;
        public string code;
        public AudioInfo(string code, string text, byte[] audio){
            this.code = code;
            this.audio = Convert.ToBase64String(audio);
            this.text = text;
        }
    }
    [System.Serializable]
    public class APIReqs : MonoBehaviour
    {
        static string baseUrl = "http://206.189.188.227:5000/";

        
        public static IEnumerator GetCode(TMP_Text codeText)
        {
            string uri = APIReqs.baseUrl+"instance";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)){
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        CodeInfo codeInfo = JsonUtility.FromJson<CodeInfo>(webRequest.downloadHandler.text);
                        codeInfo.code = "1663486608";
                        codeText.text = "Pairing Code: " + codeInfo.code; 
                        Manager.DataManager.Instance.code = codeInfo;
                        break;
                }
            }

        }
        public static IEnumerator PollAuth(){
            if(Manager.DataManager.Instance.authenticated||Manager.DataManager.Instance.code==null){
                yield break;
            }
            


            string uri = APIReqs.baseUrl+"check_authenticated";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(Manager.DataManager.Instance.code));
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);

                        Manager.DataManager.Instance.authenticated = authInfo.success;
                        break;
                }
            }


        }
        public static IEnumerator SendGestures(float gestureCount, float amountInStrikeZone, float duration){
            string uri = APIReqs.baseUrl+"/send_gestures";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type", "application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new GestureInfo(Manager.DataManager.Instance.code.code,gestureCount,amountInStrikeZone,duration)));
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);
                        

                        break;
                }
            }

                

        }
        public static IEnumerator SendAudio(byte[] data, int idx){
            string uri = APIReqs.baseUrl+"/answer_audio";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new AudioInfo(Manager.DataManager.Instance.code.code,Manager.DataManager.Instance.questions[idx].text,data)));
                /* Debug.Log(JsonUtility.ToJson(new AudioInfo */
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);

                        break;
                }

                        
            }

        }
        public static IEnumerator SendAdHawk(Dictionary<string,float> data){
            string uri = APIReqs.baseUrl+"/send_adhawk";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type", "application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new AdHawkInfo(Manager.DataManager.Instance.code.code,data)));
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);
                        

                        break;
                }
            }


                        


        }

        public static IEnumerator QuestionAudio(){
            string uri = APIReqs.baseUrl+"/question_audio";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(Manager.DataManager.Instance.code));

                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        QuestionInfos questions = JsonUtility.FromJson<QuestionInfos>(webRequest.downloadHandler.text);

                        Manager.DataManager.Instance.questions = questions.questions;
                        
                         


                        break;
                }

                        
            }


        }
        public static string RetrieveAudio(APIReq.QuestionInfo q){
            byte[] receivedBytes = Convert.FromBase64String(q.audio);
            var filepath = Path.Combine(Application.persistentDataPath, "temp.wav");
            Debug.Log(filepath);
            File.WriteAllBytes(filepath, receivedBytes);
            return filepath;
        }
        



        // Update is called once per frame
        void Update()
        {
            
        }
    }
    
}
