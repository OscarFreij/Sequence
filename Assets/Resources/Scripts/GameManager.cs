using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public bool init = false;
	public bool init2 = false;
    public bool initDone { get; private set; }
    public bool showNextBox { get; set; }
	public bool sequenceDone { get; private set; }
	public bool stepDone { get; private set; }


	public List<int> idSequence { get; private set; }
	public List<int> idSequenceCheck { get; private set; }
	public List<int> idSequenceCopy { get; private set; }
	public float overlayOnTime { get; private set; }
	public float overlayOnTimeDelay { get; private set; }

	private static GameManager _instance;
	public static GameManager Instance { get { return _instance; } }
	public GameObject gameOverMenu { get; set; }
	public SoundManager sm { get; private set; }

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
		LoadHighscoreScreen();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (init && SceneManager.GetActiveScene().name == "Game")
		{
			this.overlayOnTime = 1.0f;
			this.overlayOnTimeDelay = overlayOnTime * 0.2f;

			this.idSequence.Clear();
			this.idSequenceCopy.Clear();
			this.idSequenceCheck.Clear();


			NextStep();

			init = false;
            initDone = true;
			gameOverMenu = GameObject.Find("GameOverMenu");
			gameOverMenu.SetActive(false);

			GameObject.Find("SpeedPanel").transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate { SetSequenceSpeed(); });
			Debug.Log("Init Done!");
		}

		if (init2 && SceneManager.GetActiveScene().name == "MainMenu")
        {
			LoadHighscoreScreen();
			GameObject.Find("ButtonPanel").transform.Find("Start").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(0); });
			GameObject.Find("ButtonPanel").transform.Find("Reset").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(1); });
			GameObject.Find("ButtonPanel").transform.Find("Quit").GetComponent<Button>().onClick.AddListener(delegate { MainMenuAction(2); });
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
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayControler>().isFirst1 = true;
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayControler>().run = true;
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
					GameObject.Find("ButtonPanel").transform.Find("Button (" + nextId + ")").Find("Overlay").GetComponent<OverlayControler>().run = true;
					this.idSequenceCopy.RemoveAt(0);
				}
				showNextBox = false;

			}
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

				if (PlayerPrefs.GetInt("Highscore") < this.idSequence.Count)
                {
					PlayerPrefs.SetInt("Highscore", this.idSequence.Count);
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
				SceneManager.LoadScene(1);
				this.init = true;
				break;
			case 1:
				PlayerPrefs.DeleteAll();
				LoadHighscoreScreen();
				break;
			case 2:
				Application.Quit();
				break;
			case 3:
				SceneManager.LoadScene(0);
				this.init2 = true;
				break;
		}
    }
	
	public void LoadHighscoreScreen()
    {
		string temp = "";
		string temp2 = PlayerPrefs.GetInt("Highscore").ToString();

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

		GameObject.Find("HighScoreValue").GetComponent<Text>().text = temp;
	}
	public void SetSequenceSpeed()
    {
		this.overlayOnTime = GameObject.Find("SpeedPanel").transform.Find("Slider").GetComponent<Slider>().value;
		this.overlayOnTimeDelay = this.overlayOnTime * 0.2f;

	}
}
