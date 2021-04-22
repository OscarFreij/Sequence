using System.Net.Http;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{

    [System.Serializable]
    public class ResponseObject
    {
        public string responseCode;
        public string msg;
        public string uniqueKey;

        public static ResponseObject CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<ResponseObject>(jsonString);
        }
    }

    private HttpClient client { get; set; }
    public bool initiated { get; private set; } = false;
    private string accessKey { get; set; } = "jkvA4YRMuNrCvrr7x5bqPFffGYXRVvHL";
    public bool isRegisterd { get; private set; } = false;
    public string serverUrl { get; private set; } = "http://sequence_server.local/";

    public void Init()
    {
        this.client = new HttpClient();
        
        if (PlayerPrefs.HasKey("accessKey"))
        {
            this.accessKey = PlayerPrefs.GetString("accessKey");
            this.isRegisterd = true;
        }

        this.initiated = true;
    }

    public async void UploadScore(string score, string difficulty)
    {
        if (this.isRegisterd)
        {
            FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
{
            new KeyValuePair<string, string>("uniqueKey", accessKey),
            new KeyValuePair<string, string>("score", score),
            new KeyValuePair<string, string>("diff", difficulty),
            });

            HttpResponseMessage httpResponse = await client.PostAsync(serverUrl, postData);
            ResponseObject ro = ResponseObject.CreateFromJSON(await httpResponse.Content.ReadAsStringAsync());

            if (ro.responseCode.Contains("e"))
            {
                Debug.LogError(ro.responseCode + " : " + ro.msg);
            }
            else
            {
                return;
            }
        }
        else
        {
            Debug.LogError("User not registerd!");
        }
    }

    public async void Register(string username)
    {
        if (username.Length < 1)
        {
            GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = "Username can not be empty!";
            return;
        }

        FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("startupKey", accessKey),
            new KeyValuePair<string, string>("username", username),
        });

        HttpResponseMessage httpResponse = await client.PostAsync(serverUrl, postData);
        ResponseObject ro = ResponseObject.CreateFromJSON(await httpResponse.Content.ReadAsStringAsync());

        if (ro.uniqueKey != null)
        {
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("accessKey", ro.uniqueKey);
            this.accessKey = ro.uniqueKey;
            this.isRegisterd = true;
            GameObject.Find("RegisterMenu").SetActive(false);
            GameObject.Find("MainMenu").transform.Find("SignedInUser").transform.GetComponent<UnityEngine.UI.Text>().text = "Playing as : " + PlayerPrefs.GetString("username");
        }
        else
        {
            GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = ro.responseCode + " : " + ro.msg;
        }
    }
}
