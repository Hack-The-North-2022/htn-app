using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdhawkApi;

public class PersistentToggleDot : MonoBehaviour
{

    private static PersistentToggleDot _instance;
    private KeyCode toggleKey = KeyCode.Tab;
    private bool on = true;

    private Dictionary<string, float> eyeTrackingHits;
    private string currentHit;
    private float timeHit = 0f;
    private bool recordingEyeContact = false;

    public void StartRecording() { recordingEyeContact = true; eyeTrackingHits.Clear(); }
    public Dictionary<string, float> StopRecording() { recordingEyeContact = false; return eyeTrackingHits; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        eyeTrackingHits = new Dictionary<string, float>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(toggleKey))
        {
            on = !on;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            foreach (KeyValuePair<string, float> kvp in eyeTrackingHits) {
                Debug.Log(kvp.Key + " " + kvp.Value);
            }
        }

        if (on)
        {
            if (Player.Instance == null)
            {
                transform.position = Camera.main.transform.position + (Camera.main.transform.rotation * (EyeTrackerAPI.Instance.GazeVector.normalized * 1f));
            }
            else
            {

                //find a better position.
                RaycastHit hit;
                Ray ray = new Ray(Player.Instance.EyeCenter.position, EyeTrackerAPI.Instance.GazeVector.normalized);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    transform.position = hit.point;
                }
                else
                {
                    transform.position = Player.Instance.Cam.transform.position + (Player.Instance.Cam.transform.rotation * (EyeTrackerAPI.Instance.GazeVector.normalized * 1f));
                }
                if (recordingEyeContact && Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    if (hit.transform.tag != "Untagged") {
                        if (hit.transform.tag != currentHit) {
                            if (timeHit > 0.5f) {
                                if (eyeTrackingHits.ContainsKey(currentHit)) {
                                    eyeTrackingHits[currentHit] += timeHit;
                                } else {
                                    eyeTrackingHits.Add(currentHit, timeHit);
                                }
                            }
                            currentHit = hit.transform.tag;
                            timeHit = Time.deltaTime;
                        } else {
                            timeHit += Time.deltaTime;
                        }
                    }
                }

            }

        }
        else
        {
            transform.position = Vector3.down * 1000;
        }

    }
}
