using System.Net.Http;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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

    [System.Serializable]
    public class LoginResponseObject
    {
        public string responseCode;
        public string msg;
        public string username;
        public string score;

        public static LoginResponseObject CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<LoginResponseObject>(jsonString);
        }
    }

    private HttpClient client { get; set; }
    public bool initiated { get; private set; } = false;
    private string accessKey { get; set; } = "";
    public bool isRegisterd { get; set; } = false;
    public string serverUrl { get; private set; } = "http://sequence.offthegridcg.me/callback.php";

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

    public async void UploadScore(string score, int difficulty)
    {
        string requestUrl = serverUrl + "?action=update";

        if (this.isRegisterd)
        {
            FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
{
            new KeyValuePair<string, string>("uniqueKey", accessKey),
            new KeyValuePair<string, string>("score", score),
            new KeyValuePair<string, string>("diff", difficulty.ToString()),
            });
            
            HttpResponseMessage httpResponse = await client.PostAsync(requestUrl, postData);
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
        if (!this.isRegisterd)
        {
            if (username.Length < 1)
            {
                GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = "Username can not be empty!";
                return;
            }

            FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("startupKey", accessKey),
                new KeyValuePair<string, string>("username", username.ToString()),
            }); ;

            HttpResponseMessage httpResponse = await client.PostAsync(serverUrl + "?action=reg", postData);
            Debug.Log(await httpResponse.Content.ReadAsStringAsync());
            ResponseObject ro = ResponseObject.CreateFromJSON(await httpResponse.Content.ReadAsStringAsync());

            if (ro.uniqueKey != null)
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("accessKey", ro.uniqueKey);
                this.accessKey = ro.uniqueKey;
                this.isRegisterd = true;
                PlayerPrefs.SetInt("highscore_slow", 0);
                PlayerPrefs.SetInt("highscore_norm", 0);
                PlayerPrefs.SetInt("highscore_fast", 0);
                GameObject.Find("RegisterMenu").SetActive(false);
            }
            else
            {
                GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = ro.responseCode + " : " + ro.msg;
                Debug.LogError(ro.responseCode + " : " + ro.msg);
            }
        }
        else
        {
            Debug.LogError("User not registerd!");
        }
    }


    public async void Login(string accessKey = "")
    {
        if (accessKey.Length < 1 && !this.isRegisterd)
        {
            GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = "Accesskey can not be empty!";
            return;
        }
        else if (this.isRegisterd)
        {
            accessKey = PlayerPrefs.GetString("accessKey");
        }

        FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("startupKey", accessKey),
            new KeyValuePair<string, string>("uniqueKey", accessKey),
        });

        HttpResponseMessage httpResponse = await client.PostAsync(serverUrl + "?action=login", postData);
        LoginResponseObject ro = LoginResponseObject.CreateFromJSON(await httpResponse.Content.ReadAsStringAsync());

        if (ro.username != null)
        {
            PlayerPrefs.SetString("username", ro.username);
            PlayerPrefs.SetString("accessKey", accessKey);
            this.accessKey = accessKey;
            string[] scoreStrings = ro.score.Split(' ');
            PlayerPrefs.SetInt("highscore_slow", int.Parse(scoreStrings[0]));
            PlayerPrefs.SetInt("highscore_norm", int.Parse(scoreStrings[1]));
            PlayerPrefs.SetInt("highscore_fast", int.Parse(scoreStrings[2]));
            this.isRegisterd = true;
            GameObject.Find("RegisterMenu").SetActive(false);
        }
        else
        {
            GameObject.Find("RegisterMenu").transform.Find("ErrorMessage").GetComponent<UnityEngine.UI.Text>().text = ro.responseCode + " : " + ro.msg;
            Debug.LogError(ro.responseCode + " : " + ro.msg);
        }
    }
}
