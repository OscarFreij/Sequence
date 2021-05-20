using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public bool RUNSM = false;
	public bool init = false;
	public bool init2 = false;
    public bool initDone { get; private set; }
    public bool showNextBox { get; set; }
	public bool sequenceDone { get; private set; }
	public bool stepDone { get; private set; }
	public bool offlineMode { get; private set; }

	public List<int> idSequence { get; private set; }
	public List<int> idSequenceCheck { get; private set; }
	public List<int> idSequenceCopy { get; private set; }
	public float overlayOnTime { get; private set; }
	public float overlayOnTimeDelay { get; private set; }

	private static GameManager _instance;
	public static GameManager Instance { get { return _instance; } }
	public GameObject gameOverMenu { get; set; }
	public SoundManager sm { get; private set; }
	public ScoreManager scoreManager { get; private set; }

	public GameObject UI_MainMenu { get; set; }
	public GameObject UI_AccountMenu { get; set; }
	public GameObject UI_RegisterMenu { get; set; }
	public GameObject UI_DifficultyMenu { get; set; }
	public GameObject UI_ConnectionMenu { get; set; }

	public int difficultyId { get; private set; }


	void Awake()
    {
		DontDestroyOnLoad(this.transform);

		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}
	}
	
    // Use this for initialization
    void Start ()
	{
		init = false;
		init2 = true;
		initDone = false;
		showNextBox = false;
		sequenceDone = false;
		stepDone = false;

		idSequence = new List<int>();
		idSequenceCheck = new List<int>();
		idSequenceCopy = new List<int>();

		this.sm = transform.GetComponent<SoundManager>();

		this.scoreManager = new ScoreManager();
		this.scoreManager.Init();

		this.offlineMode = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (init && SceneManager.GetActiveScene().name == "Game")
		{
			this.idSequence.Clear();
			this.idSequenceCopy.Clear();
			this.idSequenceCheck.Clear();


			NextStep();

			init = false;
            initDone = true;
			gameOverMenu = GameObject.Find("GameOverMenu");
			gameOverMenu.SetActive(false);
			Debug.Log("Init Done!");
		}

		if (init2 && SceneManager.GetActiveScene().name == "MainMenu")
        {
			if (this.UI_MainMenu == null)
            {
				this.UI_MainMenu = GameObject.Find("MainMenu");
			}

			if (this.UI_AccountMenu == null)
			{
				this.UI_AccountMenu = GameObject.Find("AccountMenu");
			}

			if (this.UI_RegisterMenu == null)
			{
				this.UI_RegisterMenu = GameObject.Find("RegisterMenu");
			}
			
			if (this.UI_DifficultyMenu == null)
			{
				this.UI_DifficultyMenu = GameObject.Find("DifficultyMenu");
			}

			if (this.UI_ConnectionMenu == null)
			{
				this.UI_ConnectionMenu = GameObject.Find("ConnectionMenu");
			}

			this.UI_RegisterMenu.SetActive(true);
			this.UI_DifficultyMenu.SetActive(true);

			GameObject.Find("ButtonPanel").transform.Find("Start").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(0); });
			GameObject.Find("ButtonPanel").transform.Find("Account").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(1); });
			GameObject.Find("ButtonPanel").transform.Find("Quit").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(2); });

			this.UI_RegisterMenu.transform.Find("RM").Find("Create").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(4); });
			this.UI_RegisterMenu.transform.Find("LM").transform.Find("Login").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(5); });
			this.UI_AccountMenu.transform.Find("Return").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(8); });
			this.UI_AccountMenu.transform.Find("Logout").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(6); });
			this.UI_AccountMenu.transform.Find("CopyKey").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(7); });
			
			this.UI_DifficultyMenu.transform.Find("Slow").GetComponent<Button>().onClick.AddListener(delegate { StartGameAction(0); });
			this.UI_DifficultyMenu.transform.Find("Normal").GetComponent<Button>().onClick.AddListener(delegate { StartGameAction(1); });
			this.UI_DifficultyMenu.transform.Find("Fast").GetComponent<Button>().onClick.AddListener(delegate { StartGameAction(2); });

			this.UI_DifficultyMenu.transform.Find("Fast").GetComponent<Button>().onClick.AddListener(delegate { StartGameAction(2); });
			this.UI_DifficultyMenu.transform.Find("Fast").GetComponent<Button>().onClick.AddListener(delegate { StartGameAction(2); });

			this.UI_ConnectionMenu.transform.Find("Continue").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(9); });
			this.UI_ConnectionMenu.transform.Find("Quit").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(2); });


			this.UI_AccountMenu.SetActive(false);
			this.UI_DifficultyMenu.SetActive(false);
			if (this.scoreManager.isRegisterd)
			{
				this.UI_RegisterMenu.SetActive(false);
				Debug.Log("User account has accessKey, closing register menu!");
			}

			if (!this.offlineMode)
			{
				this.UI_ConnectionMenu.SetActive(true);
				this.UI_ConnectionMenu.transform.Find("ConnectionText").GetComponent<Text>().text = "Connecting...";
				this.UI_ConnectionMenu.transform.Find("Continue").GetComponent<Button>().interactable = false;
				StartCoroutine(checkInternetConnection((isConnected) => {
					if (!isConnected)
					{
						Debug.LogWarning("Unable to connect to server! - Restart game to try connection again!");
						this.UI_ConnectionMenu.transform.Find("ConnectionText").GetComponent<Text>().text = "Could not connect to server!";
						this.UI_ConnectionMenu.transform.Find("Continue").GetComponent<Button>().interactable = true;
					}
					else
					{
						this.UI_ConnectionMenu.transform.Find("ConnectionText").GetComponent<Text>().text = "Connected";
						this.UI_ConnectionMenu.SetActive(false);
					}
				}));
			}
			else
            {
				GameObject.Find("ButtonPanel").transform.Find("Account").GetComponent<Button>().interactable = false;
				this.UI_ConnectionMenu.SetActive(false);
				this.UI_RegisterMenu.SetActive(false);
				Debug.Log("User playing in offline mode!");
			}
			

			init2 = false;

		}

        if (initDone)
        {
			if (showNextBox)
            {

				ToggleInput(0);
				Debug.Log("SequenceDisplay Running!");
				if (this.idSequenceCopy.Count == this.idSequence.Count)
                {
					int nextId = this.idSequenceCopy[0];
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayController>().isFirst1 = true;
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayController>().run = true;
					this.idSequenceCopy.RemoveAt(0);
				}
				else if (this.idSequenceCopy.Count < 1)
                {
					sequenceDone = true;
					ToggleInput(1);
					Debug.Log("SequenceDisplay Done!");
				}
				else
                {
					int nextId = this.idSequenceCopy[0];
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayController>().run = true;
					this.idSequenceCopy.RemoveAt(0);
				}
				showNextBox = false;

			}
        }

		if (!init && SceneManager.GetActiveScene().name == "Game")
        {
			this.KeypadToButton();

		}
    }

	/// <summary>
	/// Generate next id for sequence.1
	/// </summary>
	public void NextStep()
	{
		int temp = Random.Range(1, 9);
		this.idSequence.Add(temp);
		this.idSequenceCopy.Clear();
		this.idSequenceCopy.AddRange(this.idSequence);
		this.idSequenceCheck.Clear();
		this.showNextBox = true;
		this.StepScreen();
	}

	/// <summary>
	/// Check if current sequence is equal to pattern sequence.
	/// </summary>
	/// <param name="id">Button id.</param>
	/// <returns>Returns true if pattern is correct and otherwise returns false.</returns>
	public bool CheckClick(int id)
	{
		this.idSequenceCheck.Add(id);
		sm.PlaySound(id);
		for (int i = 0; i < this.idSequenceCheck.Count; i++)
		{
			if (this.idSequence[i] != this.idSequenceCheck[i])
			{
				sm.PlaySound(9);
				Debug.Log("GameOver!");
				this.ToggleInput(0);
				string localdofficultyScoreName = "";
                switch (this.difficultyId)
                {
					case 0:
						localdofficultyScoreName = "highscore_slow";
						break;
					case 1:
						localdofficultyScoreName = "highscore_norm";
						break;
					case 2:
						localdofficultyScoreName = "highscore_fast";
						break;
				}

                if (PlayerPrefs.GetInt(localdofficultyScoreName) < this.idSequence.Count && !this.offlineMode)
                {
					PlayerPrefs.SetInt(localdofficultyScoreName, this.idSequence.Count);
					this.scoreManager.UploadScore(this.idSequence.Count.ToString(), this.difficultyId);
				}

				gameOverMenu.SetActive(true);
				gameOverMenu.transform.Find("ButtonPanel").Find("MainMenu").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(3); });
				gameOverMenu.transform.Find("ButtonPanel").Find("Quit").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(2); });

				return false;
			}
		}

		if (this.idSequence.Count == this.idSequenceCheck.Count )
        {
			this.NextStep();
			Debug.Log("Good job! On to the next step!");
		}

		return true;
	}

	public void ToggleInput(int state = -1)
    {

		StartCoroutine(DelayAction((action) => {
			for (int i = 1; i <= 9; i++)
			{
				Button btn = GameObject.Find("ButtonPanel").transform.Find("Button (" + i + ")").GetComponent<Button>();
				if (state == -1)
				{
					btn.interactable = !btn.interactable;
				}
				else
				{
					btn.interactable = System.Convert.ToBoolean(state);
				}
			}
		}));



		
    }

	public IEnumerator DelayAction(System.Action<bool> action)
    {
		yield return new WaitForSeconds(0.1f);
		action(false);

	}

	public void StepScreen()
    {
		string temp = "";
        if (idSequence.Count.ToString().Length == 1)
        {
			temp = "00" + idSequence.Count.ToString();
		}
		else if (idSequence.Count.ToString().Length == 2)
        {
			temp = "0" + idSequence.Count.ToString();
		}
		else
        {
			temp = idSequence.Count.ToString();
		}

		GameObject.Find("ProgressPanel").transform.Find("BG").Find("Step").Find("Value").GetComponent<Text>().text = temp;
    }


	public void MainMenuAction(int id)
    {
        switch (id)
        {
			case 0:
				// Opens DifficultyMenu
				this.UI_DifficultyMenu.SetActive(true);
				break;
			case 1:
				// Opens AccountMenu
				this.UI_AccountMenu.SetActive(true);
				LoadAccountScreen();
				break;
			case 2:
				// Quits application
				Application.Quit();
				break;
			case 3:
				// Loads MainMenu Scean
				SceneManager.LoadScene(0);
				this.init2 = true;
				break;
			case 4:
				// Runs register user
				this.scoreManager.Register(this.UI_RegisterMenu.transform.Find("RM").Find("UsernameField").GetComponent<InputField>().text);
				this.UI_RegisterMenu.transform.Find("RM").Find("UsernameField").GetComponent<InputField>().text = "";
				break;
			case 5:
				// Runs login user
				this.scoreManager.Login(this.UI_RegisterMenu.transform.Find("LM").Find("AccessKeyField").GetComponent<InputField>().text);
				this.UI_RegisterMenu.transform.Find("LM").Find("AccessKeyField").GetComponent<InputField>().text = "";
				break;
			case 6:
				// Logs user out and removes local data
				PlayerPrefs.DeleteAll();
				this.scoreManager = new ScoreManager();
				this.scoreManager.Init();
				this.init2 = true;
				break;
			case 7:
				// Copy user access key to clipboard
				GUIUtility.systemCopyBuffer = PlayerPrefs.GetString("accessKey");
				break;
			case 8:
				// Closes AccountMenu
				this.UI_AccountMenu.SetActive(false);
				break;
			case 9:
				// Run game in offline mode!
				this.offlineMode = true;
				GameObject.Find("ButtonPanel").transform.Find("Account").GetComponent<Button>().interactable = false;
				this.UI_RegisterMenu.SetActive(false);
				this.UI_ConnectionMenu.SetActive(false);
				break;
		}
    }

	public void StartGameAction(int difficulty)
	{
		// Loads Game Scean
		switch (difficulty)
		{
			case 0:
				// Slow
				this.difficultyId = 0;
				break;
			case 1:
				// Normal
				this.difficultyId = 1;
				break;
			case 2:
				// Fast
				this.difficultyId = 2;
				break;
		}
		SetSequenceSpeed();
		SceneManager.LoadScene(1);
		this.init = true;
	}

	public void LoadAccountScreen()
    {
		this.UI_AccountMenu.transform.Find("Username").GetComponent<Text>().text = PlayerPrefs.GetString("username");
		this.UI_AccountMenu.transform.Find("ScorePanel").Find("Score_Slow_Value").GetComponent<Text>().text = HighScoreParse(PlayerPrefs.GetInt("highscore_slow"));
		this.UI_AccountMenu.transform.Find("ScorePanel").Find("Score_Norm_Value").GetComponent<Text>().text = HighScoreParse(PlayerPrefs.GetInt("highscore_norm"));
		this.UI_AccountMenu.transform.Find("ScorePanel").Find("Score_Fast_Value").GetComponent<Text>().text = HighScoreParse(PlayerPrefs.GetInt("highscore_fast"));
	}

	public string HighScoreParse(int input)
    {
		string temp = "";
		string temp2 = input.ToString();
		if (temp2.Length != 0)
		{
			if (temp2.Length == 1)
			{
				temp = "00" + temp2;
			}
			else if (temp2.Length == 2)
			{
				temp = "0" + temp2;
			}
			else
			{
				temp = temp2;
			}
		}
		else
		{
			temp = "000";
		}

		return temp;
	}

	public void SetSequenceSpeed()
    {
        switch (this.difficultyId)
        {
			case 0:
				this.overlayOnTime = 0.75f;
				break;
			case 1:
				this.overlayOnTime = 0.5f;
				break;
			case 2:
				this.overlayOnTime = 0.1f;
				break;
		}
        
		this.overlayOnTimeDelay = this.overlayOnTime * 0.2f;

	}

	IEnumerator checkInternetConnection(System.Action<bool> action)
	{
		UnityWebRequest webRequest = new UnityWebRequest();
		webRequest.timeout = 5;
		webRequest.url = "http://sequence.offthegridcg.me";

		yield return webRequest.SendWebRequest();
		if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.Log(0);
			action(false);
		}
		else
		{
			Debug.Log(1);
			action(true);
		}
		
	}

	private void KeypadToButton()
    {
		var pointer = new PointerEventData(EventSystem.current);
		if (Input.GetKeyDown(KeyCode.Keypad1))
        {
			ExecuteEvents.Execute(GameObject.Find("Button (1)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (2)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (3)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
			ExecuteEvents.Execute(GameObject.Find("Button (4)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (5)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad6))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (6)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad7))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (7)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad8))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (8)"), pointer, ExecuteEvents.submitHandler);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad9))
		{
			ExecuteEvents.Execute(GameObject.Find("Button (9)"), pointer, ExecuteEvents.submitHandler);
		}

        
	}
}
