using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class UIController : MonoBehaviour {

    public GameObject player;

    // Menu Parents
    public GameObject mainMenuParent;
    public GameObject deathMenuParent;

    // Game UI
    public GameObject lblGameScore;

    // Menu UI
    public GameObject playButton;
    public GameObject replayButton;
    public GameObject lblMenuBestScoreNum;
    public GameObject easyButton;
    public GameObject hardButton;
    public Animator animChar;
    public GameObject headerUI;
     
    // Death UI
    public GameObject lblDeathScoreNum;
    public GameObject lblDeathBestScoreNum;

    // Sprites
    public Sprite spriteBtnEasy;
    public Sprite spriteBtnEasyPressed;
    public Sprite spriteBtnHard;
    public Sprite spriteBtnHardPressed;

    // Variables
    private bool fadeMainMenuOut = false;
    private bool fadeMainMenuIn = false;
    private bool fadeDeathMenuOut = false;
    private bool fadeDeathMenuIn = false;
    private bool restarting = false;

    private int score = 0;
    private int bestScore = 0;

    public static EventHandler Restarting;
    public static EventHandler StartGame;

    public enum GameMode { Easy, Hard };
    public static GameMode currentMode;

    void Start()
    {
		// Application Settings
		Application.targetFrameRate = 60;

        currentMode = GameMode.Easy;

        bestScore = PlayerPrefs.GetInt("Score");

        // Disable button if best score is under 30
        if (bestScore < 30)
        {
            hardButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        else
        {
            easyButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(EasyClicked);
            hardButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(HardClicked);
            hardButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteBtnHard;
        }

        iTween.ScaleTo(playButton, iTween.Hash("scale", new Vector3(1.06f, 1.06f, 0), "time", 0.5f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.pingPong));
        iTween.ScaleTo(replayButton, iTween.Hash("scale", new Vector3(1.06f, 1.06f, 0), "time", 0.5f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.pingPong));

        // UI Settings
        PlayerController.PlayerDead += IsDead;
        GameController.LevelComplete += LevelComplete;
        replayButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Restart);
        playButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlayGame);
        lblMenuBestScoreNum.GetComponent<UnityEngine.UI.Text>().text = bestScore.ToString();
    }

    void Update()
    {
        if (fadeMainMenuOut)
        {
            SetMainMenu(false, 2);
        }
        else if (fadeMainMenuIn)
        {
            SetMainMenu(true, 3);
        }

        if (fadeDeathMenuOut)
        {
            SetDeathMenu(false, 2);             
        }     
        else if (fadeDeathMenuIn)
        {
            SetDeathMenu(true, 1);
        }    
        
        if (restarting)
        {
            if (!fadeDeathMenuOut && !fadeMainMenuIn && !fadeMainMenuOut)
            {
                // RELOAD EVERYTHING
                SetTransparancyOfObjectInstantly(deathMenuParent, 1f);
                if (Restarting != null) Restarting(null, null);
                restarting = false;
                fadeDeathMenuOut = true;
                score = 0;
                lblGameScore.GetComponent<UnityEngine.UI.Text>().text = score.ToString();
            }
        }
    }

    void IsDead(System.Object obj, EventArgs args)
    {
        if (!restarting)
        {
            fadeDeathMenuOut = false;
            fadeMainMenuIn = false;
            fadeMainMenuOut = false;
            fadeDeathMenuIn = true;
        }

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("Score", score);
        }
            
        animChar.SetTrigger("die");
        lblGameScore.SetActive(false);
        lblDeathScoreNum.GetComponent<UnityEngine.UI.Text>().text = score.ToString();
        lblDeathBestScoreNum.GetComponent<UnityEngine.UI.Text>().text = bestScore.ToString();
    }

    void LevelComplete(System.Object obj, EventArgs args)
    {
        // Add 1 to the score
        score++;

        // Update the score UI
        lblGameScore.GetComponent<UnityEngine.UI.Text>().text = score.ToString();
        iTween.ScaleFrom(lblGameScore, iTween.Hash("scale", new Vector3(1.1f, 1.1f, 0), "time", 0.3f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none));
    }

    public void Restart()
    {
        // These line prevents a null ref exception in the IsDead function when playing for the second time
        //GameController.LevelComplete -= LevelComplete;
        //PlayerController.PlayerDead -= IsDead;
        //SceneManager.LoadScene(0);
        if (!fadeDeathMenuOut)
            restarting = true;
        lblGameScore.SetActive(true);
        lblGameScore.GetComponent<Animator>().SetTrigger("slidein");
    }

    public void PlayGame()
    {
        if (!fadeMainMenuOut)
        {
            if (StartGame != null) StartGame(null, null);
            fadeMainMenuOut = true;
            lblGameScore.GetComponent<Animator>().SetTrigger("slidein");
        }
    }

    public void SetMainMenu(bool visible, float speed)
    {
        if (!visible)
        {
            mainMenuParent.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.r, mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.g, mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.b, 0), Time.deltaTime * speed);

            foreach (UnityEngine.UI.Image img in mainMenuParent.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.color = Color.Lerp(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(img.color.r, img.color.g, img.color.b, 0), Time.deltaTime * speed);
            }

            foreach (UnityEngine.UI.Text txt in mainMenuParent.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                txt.color = Color.Lerp(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(txt.color.r, txt.color.g, txt.color.b, 0), Time.deltaTime * speed);
            }

            if (mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.a == 0.0f)
            {
                fadeMainMenuOut = false;
                mainMenuParent.SetActive(false);
            }
        }
        else
        {
            if (!mainMenuParent.activeSelf)
            {
                SetTransparancyOfObjectInstantly(mainMenuParent, 0f);
                mainMenuParent.SetActive(true);
            }               

            mainMenuParent.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.r, mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.g, mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.b, 1), Time.deltaTime * speed);

            foreach (UnityEngine.UI.Image img in mainMenuParent.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.color = Color.Lerp(mainMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(img.color.r, img.color.g, img.color.b, 0), Time.deltaTime * speed);
            }

            if (mainMenuParent.GetComponent<UnityEngine.UI.Image>().color.a == 1.0f)
            {
                SetTransparancyOfObjectInstantly(mainMenuParent, 1f);
                fadeMainMenuIn = false;
            }
        }
    }

    public void SetDeathMenu(bool visible, float speed)
    {
        if (!visible)
        {
            deathMenuParent.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(deathMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.r, deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.g, deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.b, 0), Time.deltaTime * speed);

            foreach (UnityEngine.UI.Image img in deathMenuParent.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.color = Color.Lerp(img.color, new Color(img.color.r, img.color.g, img.color.b, 0), Time.deltaTime * speed);
            }

            foreach (UnityEngine.UI.Text txt in deathMenuParent.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                txt.color = Color.Lerp(txt.color, new Color(txt.color.r, txt.color.g, txt.color.b, 0), Time.deltaTime * speed);
            }

            if (deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.a == 0.0f)
            {
                fadeDeathMenuOut = false;
                deathMenuParent.SetActive(false);
            }
        }
        else
        {
            if (!deathMenuParent.activeSelf)
            {
                SetTransparancyOfObjectInstantly(deathMenuParent, 0f);

                deathMenuParent.SetActive(true);
            }         

            deathMenuParent.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(deathMenuParent.GetComponent<UnityEngine.UI.Image>().color, new Color(deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.r, deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.g, deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.b, 1), Time.deltaTime * speed);

            foreach (UnityEngine.UI.Image img in deathMenuParent.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.color = Color.Lerp(img.color, new Color(img.color.r, img.color.g, img.color.b, 1), Time.deltaTime * speed);
            }

            foreach (UnityEngine.UI.Text txt in deathMenuParent.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                txt.color = Color.Lerp(txt.color, new Color(txt.color.r, txt.color.g, txt.color.b, 1), Time.deltaTime * speed);
            }

            if (deathMenuParent.GetComponent<UnityEngine.UI.Image>().color.a == 1.0f)
            {
                fadeDeathMenuIn = false;
            }
        }
    }

    private void SetTransparancyOfObjectInstantly(GameObject obj, float transparency)
    {
        obj.GetComponent<UnityEngine.UI.Image>().color = new Color(obj.GetComponent<UnityEngine.UI.Image>().color.r, obj.GetComponent<UnityEngine.UI.Image>().color.g, obj.GetComponent<UnityEngine.UI.Image>().color.b, transparency);

        foreach (UnityEngine.UI.Image img in obj.GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, transparency);
        }

        foreach (UnityEngine.UI.Text txt in obj.GetComponentsInChildren<UnityEngine.UI.Text>())
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, transparency);
        }
    }

    private float ConvertToPositiveFloat(float f)
    {
        if (f > 0f)
        {
            return f;
        }
        else
        {
            return f * -1f;
        }
    }

    void EasyClicked()
    {
        currentMode = GameMode.Easy;
        easyButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteBtnEasyPressed;
        hardButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteBtnHard;
        foreach (Transform child in headerUI.GetComponentInChildren<Transform>())
        {
            if (child.CompareTag("UIColourChange"))
            {
                child.GetComponent<UnityEngine.UI.Image>().color = new Color32(34, 177, 76, 255);
            }
        }
    }

    void HardClicked()
    {
        currentMode = GameMode.Hard;
        easyButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteBtnEasy;
        hardButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteBtnHardPressed;
        foreach (Transform child in headerUI.GetComponentInChildren<Transform>())
        {
            if (child.CompareTag("UIColourChange"))
            {
                child.GetComponent<UnityEngine.UI.Image>().color = new Color32(216, 69, 94, 255);
            }
        }
    }
}
