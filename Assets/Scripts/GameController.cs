using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour
{
    // Player
    public GameObject player;
    private Animator animator;

    // Prefabs
    public GameObject corrdior;
    public GameObject spikeObstacleBottom;
    public GameObject spikeObstacleTop;
    public GameObject thrownWeapon;
    public GameObject enemy;
    public GameObject enemyShooter;
    public GameObject hangingBall;
    public GameObject trapDoor;

    // Backgrounds
    public Sprite redBackground;
    public Sprite greenBackground;
    public Sprite blueBackground;
    public Sprite lightBlueBackground;
    public Sprite purpleBackground;
    public Sprite orangeBackground;
    public Sprite startBackground;

    // UI
    public GameObject btnLeft;
    public GameObject btnRight;
    public GameObject btnStart;

    // Variables
    public int corridorsToCreateAtStart;
    public float negativeYDistance;
    public int numberOfColouredBackgrounds;

    // Object Pools
    private List<Corridor> corridorList = new List<Corridor>();
    private List<Corridor> pooledStartCorridors = new List<Corridor>();
    private List<Obstacle> pooledObstacles = new List<Obstacle>();

    // Privates
    private int groupIndex = 1;
    private int currentCorridorID = 1;

    private float distanceBetweenCorridorsVertical;
    private float highestCorridorPosY;

    private float animTimeLeft = 0f;
    private float newLevelWaitTime = 0.1f;
    private float jumpTimer = -1.2f;

    private float startPosY;

    private System.Random rand = new System.Random();

    private bool gameStarted;
    private bool trapDoorObstacle = false;
    private TypeOfObstacle previousObstacle = TypeOfObstacle.NONE;
    private Sprite previousBackground = null;

    private enum TypeOfObstacle { BottomSpike, HangingBall, ThrownWeapon, Enemy, EnemyShooter, TrapDoor, NONE };

    public static EventHandler LevelComplete;

    // Structs
    struct Background
    {
        public Sprite background;
        public string tag;
    }

    struct Corridor
    {
        public GameObject corridor;
        public int obstacleid;
    }

    struct Obstacle
    {
        public GameObject obstacle;
        public TypeOfObstacle obstacleType;
    }

    void Start()
    {
        // Get all required values
        distanceBetweenCorridorsVertical = corrdior.GetComponentInChildren<Renderer>().bounds.size.y;
        animator = player.GetComponent<Animator>();
        startPosY = player.GetComponent<Transform>().position.y;

        // Setup the start button
        btnStart.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        btnStart.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);

        // Setup the event handlers
        UIController.Restarting += Restarting;
        UIController.StartGame += StartGame;

        // Add two start corridors (white corridors at start of game)
        AddStartCorridor(); AddStartCorridor();

        // Instantiate the corridors
        for (int i = 0; i < corridorsToCreateAtStart; i++)
        {
            GameObject corridorNewInstance = GameObject.Instantiate(corrdior) as GameObject;

            // Position them equidistant apart
            corridorNewInstance.transform.position = new Vector3(0, (groupIndex == 1) ? groupIndex + negativeYDistance : corridorList[groupIndex - 2].corridor.transform.position.y + distanceBetweenCorridorsVertical, 0);

            // Declare the position of the current highest corridor (used when moving corridors from the bottom back to the top)
            highestCorridorPosY = corridorNewInstance.transform.position.y;

            // Set the background to the start background (white)
            foreach (Transform child in corridorNewInstance.transform)
            {
                if (child.gameObject.CompareTag("CorridorBackground"))
                {
                    child.GetComponent<SpriteRenderer>().sprite = startBackground;
                    break;
                }
            }

            // New instance of corridor. Obstacle id is -1 as it won't have a obstacle
            Corridor newCorridor = new Corridor();
            newCorridor.obstacleid = -1;
            newCorridor.corridor = corridorNewInstance;

            // Add it to the obstacle pool
            corridorList.Add(newCorridor);

            groupIndex++;
        }

        // Create thrown weapons and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(thrownWeapon) as GameObject;
            obstacle.SetActive(false);
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.ThrownWeapon;
            pooledObstacles.Add(obs);
        }

        // Create enemies and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(enemy) as GameObject;
            obstacle.SetActive(false);
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.Enemy;
            pooledObstacles.Add(obs);
        }

        // Create spikes and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(spikeObstacleBottom) as GameObject;
            obstacle.SetActive(false);
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.BottomSpike;
            pooledObstacles.Add(obs);
        }

        // Create hanging balls and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(hangingBall) as GameObject;
            obstacle.SetActive(false);
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.HangingBall;
            pooledObstacles.Add(obs);
        }

        // Create enemy shooters and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(enemyShooter) as GameObject;
            obstacle.SetActive(false);
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.EnemyShooter;
            pooledObstacles.Add(obs);
        }

        // Create trapdoors and add to pool
        for (int i = 0; i < 2; i++)
        {
            GameObject obstacle;
            obstacle = GameObject.Instantiate(trapDoor) as GameObject;
            Obstacle obs = new Obstacle();
            obs.obstacle = obstacle;
            obs.obstacleType = TypeOfObstacle.TrapDoor;
            obstacle.SetActive(false);
            pooledObstacles.Add(obs);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (player.GetComponent<PlayerController>().dead)
            {
                // Animations
                btnLeft.GetComponent<Animator>().SetTrigger("SlideOut");
                btnRight.GetComponent<Animator>().SetTrigger("SlideOut");
                animator.SetTrigger("Dead");
                gameStarted = false;

            }
            else
            {
                // Wait ~2 secs before checking if we've completed another corridor (bug fix)
                if (corridorList.Count > 0 && newLevelWaitTime < 0f)
                {
                    // If the player is close to the side of the screen they're moving towards
                    if ((player.GetComponent<Rigidbody2D>().velocity.x > 0 && player.transform.position.x > 0) || (player.GetComponent<Rigidbody2D>().velocity.x < 0 && player.transform.position.x < 0))
                    {
                        // If the player has gone off the screen and isn't visible (AKA completed the corridor)
                        if (!player.GetComponent<Renderer>().isVisible)
                        {
                            NextLevel();
                            RemoveBottomLevel();
                        }
                    }
                }

                // Jump physics
                if (jumpTimer < -0f)
                {
                    if (!trapDoorObstacle)
                        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
                }

                // Gives the player the possibility to fall to their death
                if (player.GetComponent<Renderer>().isVisible && trapDoorObstacle)
                {
                    player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                }

                UpdateTimers();
                UpdateAnimations();
            }
        }
    }

    // Called when the player gets to a new corridor. Some obstacles have scripts attached to them
    void RunObstacles()
    {
        switch (corridorList[currentCorridorID].corridor.tag)
        {
            case "ThrownWeapon":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<ThrownWeaponController>().RunObstacle(player.GetComponent<PlayerController>().forwardMovementSpeed);
                break;
            case "HangingBall":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<HangingObstacleController>().RunObstacle();
                break;
            case "Enemy":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<EnemyScript>().RunObstacle(player.GetComponent<PlayerController>().forwardMovementSpeed);
                break;
            case "EnemyShooter":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<EnemyShooterScript>().RunObstacle();
                break;
            case "BottomSpike":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<SpikeBottomController>().RunObstacle();
                break;
            case "TrapDoor":
                corridorList[currentCorridorID].corridor.GetComponentInChildren<TrapDoorController>().RunObstacle();
                trapDoorObstacle = true;
                return;
        }

        trapDoorObstacle = false;
    }

    // Some obstacles need to be manually reset when the corridor they're in goes off the screen
    void ResetObstacle(Corridor corridorToReset, int corridorIndex)
    {
        if (corridorToReset.obstacleid != -1)
        {
            switch (pooledObstacles[corridorToReset.obstacleid].obstacleType)
            {
                case TypeOfObstacle.Enemy:
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<EnemyScript>().PrepObstacle(corridorToReset.corridor);
                    break;
                case TypeOfObstacle.HangingBall:
                    float direction = player.GetComponent<PlayerController>().forwardMovementSpeed;
                    for (int i = 0; i < corridorIndex; i++) // Get the direction that the player will be on the floor
                    {
                        direction *= -1;
                    }
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<HangingObstacleController>().PrepObstacle(corridorToReset.corridor, direction);
                    break;
                case TypeOfObstacle.TrapDoor:
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<TrapDoorController>().PrepObstacle(corridorToReset.corridor);
                    break;
                case TypeOfObstacle.BottomSpike:
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<SpikeBottomController>().PrepObstacle(corridorToReset.corridor);
                    break;
                case TypeOfObstacle.EnemyShooter:
                    float dir = player.GetComponent<PlayerController>().forwardMovementSpeed;
                    for (int i = 0; i <= corridorIndex; i++) // Get the direction that the player will be on the floor
                    {
                        dir *= -1;
                    }
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<EnemyShooterScript>().PrepObstacle(corridorToReset.corridor, dir);
                    break;
                case TypeOfObstacle.ThrownWeapon:
                    pooledObstacles[corridorToReset.obstacleid].obstacle.GetComponentInChildren<ThrownWeaponController>().PrepObstacle(corridorToReset.corridor);
                    break;
            }
        }
    }

    void StartGame()
    {
        gameStarted = true;
        player.GetComponent<PlayerController>().moving = true;
        player.GetComponent<Animator>().SetTrigger("Moving");

        btnStart.GetComponent<Animator>().SetTrigger("Started");
        btnLeft.SetActive(true);
        btnRight.SetActive(true);
    }

    void RemoveBottomLevel()
    {
        // If the bottom level is invisible (off the bottom of the screen)
        if (!corridorList[0].corridor.GetComponentInChildren<Renderer>().isVisible && corridorList[0].corridor.tag != "StartingCorridor")
        {
            // Update the highest corridor position
            highestCorridorPosY = highestCorridorPosY + distanceBetweenCorridorsVertical;

            // Move the lowest corridor to the new highest position
            corridorList[0].corridor.GetComponent<Transform>().position = new Vector3(0, highestCorridorPosY, 0);

            if (corridorList[0].obstacleid != -1)
            {
                // Remove the attached obstacle to that corridor
                pooledObstacles[corridorList[0].obstacleid].obstacle.SetActive(false);
                pooledObstacles[corridorList[0].obstacleid].obstacle.transform.parent = null;

                // If the obstacle was a trapdoor we have to put the floor of the corridor back
                if (corridorList[0].corridor.CompareTag("TrapDoor"))
                {
                    foreach (Transform child in corridorList[0].corridor.transform)
                    {
                        if (child.CompareTag("CorridorFloor"))
                        {
                            child.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
            }

            // Give the corridor a new obstacle
            corridorList[0] = RandomObstacle(corridorList[0], corridorList.Count);

            // Add it to the top of the list
            corridorList.Add(corridorList[0]);
            corridorList.RemoveAt(0);

            groupIndex++;
            currentCorridorID--;
        }
        else if (corridorList[0].corridor.tag == "StartingCorridor")
        {
            // Add it to the pool to be re-used when restarting
            pooledStartCorridors.Add(corridorList[0]);

            // Remove it from our list of corridors
            corridorList.RemoveAt(0);
            currentCorridorID--;
        }
    }

    void NextLevel()
    {
        // If the player is mid jump put him back on the floor
        if (player.GetComponent<Rigidbody2D>().transform.position.y != startPosY)
        {
            player.GetComponent<Rigidbody2D>().transform.position = new Vector2(player.GetComponent<Rigidbody2D>().transform.position.x, startPosY);
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            jumpTimer = -1.2f;
        }

        // Invert the player's x velocity making him move the other way
        player.GetComponent<PlayerController>().forwardMovementSpeed *= -1;

        // Bump the player up a corridor
        Vector2 playerPos = player.GetComponent<Rigidbody2D>().transform.position;
        playerPos.y += distanceBetweenCorridorsVertical;
        player.GetComponent<Rigidbody2D>().transform.position = playerPos;

        // Flip the sprite to make him run the other way
        player.GetComponent<SpriteRenderer>().flipX = (player.GetComponent<SpriteRenderer>().flipX) ? false : true;

        // Update the corridor number we're on in the obstacle pool
        currentCorridorID++;

        if (currentCorridorID == corridorsToCreateAtStart)
            currentCorridorID = 0;

        // Update the buttons to match the current level
        UpdateButtons();

        // Start the obstacle
        RunObstacles();

        // Update the score
        if (LevelComplete != null) LevelComplete(null, null);

        // Set the base Y pos
        startPosY = player.GetComponent<Transform>().position.y;
        newLevelWaitTime = 1f;
    }

    void SetButton(GameObject button, string type)
    {
        switch (type)
        {
            case "Slide":
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlayerSlide);
                button.GetComponentInChildren<UnityEngine.UI.Text>().text = "Slide";
                button.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(11, 180, 255, 255);
                break;

            case "Jump":
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlayerJump);
                button.GetComponentInChildren<UnityEngine.UI.Text>().text = "Jump";
                button.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(11, 180, 61, 255);
                break;

            case "Fight":
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlayerAttack);
                button.GetComponentInChildren<UnityEngine.UI.Text>().text = "Fight";
                button.GetComponentInChildren<UnityEngine.UI.Text>().color = new Color32(206, 82, 82, 255);
                break;
        }
    }

    void UpdateButtons()
    {
        iTween.ScaleFrom(btnLeft.gameObject, iTween.Hash("scale", new Vector3(1.06f, 1.06f, 0), "time", 0.2f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none));
        iTween.ScaleFrom(btnRight.gameObject, iTween.Hash("scale", new Vector3(1.06f, 1.06f, 0), "time", 0.2f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none));

        btnLeft.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        btnRight.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        switch (corridorList[currentCorridorID].corridor.tag)
        {
            case "ThrownWeapon":
            case "HangingBall":
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    SetButton(btnLeft, "Slide");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnRight, "Jump");
                    }
                    else
                    {
                        SetButton(btnRight, "Fight");
                    }
                }
                else
                {
                    SetButton(btnRight, "Slide");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnLeft, "Jump");
                    }
                    else
                    {
                        SetButton(btnLeft, "Fight");
                    }
                }
                return;

            case "BottomSpike":
            case "TrapDoor":
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    SetButton(btnLeft, "Jump");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnRight, "Slide");
                    }
                    else
                    {
                        SetButton(btnRight, "Fight");
                    }
                }
                else
                {
                    SetButton(btnRight, "Jump");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnLeft, "Slide");
                    }
                    else
                    {
                        SetButton(btnLeft, "Fight");
                    }
                }
                return;

            case "Enemy":
            case "EnemyShooter":
                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    SetButton(btnLeft, "Fight");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnRight, "Slide");
                    }
                    else
                    {
                        SetButton(btnRight, "Jump");
                    }
                }
                else
                {
                    SetButton(btnRight, "Fight");

                    if (UnityEngine.Random.Range(0, 2) == 1)
                    {
                        SetButton(btnLeft, "Slide");
                    }
                    else
                    {
                        SetButton(btnLeft, "Jump");
                    }
                }
                return;


        }
    }

    void UpdateAnimations()
    {
        if (animTimeLeft < 0)
        {
            animator.ResetTrigger("Jump 0");
            animator.ResetTrigger("Slide 0");
            animator.ResetTrigger("Attack 0");

            player.GetComponent<PlayerController>().attacking = false;
        }
    }

    void UpdateTimers()
    {
        if (animTimeLeft > -2f)
            animTimeLeft -= Time.deltaTime;
        if (jumpTimer > -2f)
            jumpTimer -= Time.deltaTime;
        if (newLevelWaitTime > -0.1f)
            newLevelWaitTime -= Time.deltaTime;
    }

    void PlayerJump()
    {
        if (animTimeLeft < 0 && gameStarted)
        {
            animator.SetTrigger("Jump 0");
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 200f));
            jumpTimer = 1f;
            animTimeLeft = 1f;
        }
    }

    void PlayerSlide()
    {
        if (animTimeLeft < 0 && gameStarted)
        {
            animator.SetTrigger("Slide 0");
            animTimeLeft = 1f;
        }
    }

    void PlayerAttack()
    {
        if (animTimeLeft < 0 && gameStarted)
        {
            animator.SetTrigger("Attack 0");
            animTimeLeft = 1f;
            player.GetComponent<PlayerController>().attacking = true;
        }
    }

    void AddStartCorridor(bool addToStart = false)
    {
        GameObject startingCorridor = GameObject.Instantiate(corrdior) as GameObject;
        startingCorridor.transform.position = new Vector3(0, (groupIndex == 1) ? groupIndex + negativeYDistance : corridorList[groupIndex - 2].corridor.transform.position.y + distanceBetweenCorridorsVertical, 0);
        highestCorridorPosY = startingCorridor.transform.position.y;
        startingCorridor.tag = "StartingCorridor";

        foreach (Transform child in startingCorridor.transform)
        {
            if (child.gameObject.CompareTag("CorridorBackground"))
            {
                child.GetComponent<SpriteRenderer>().sprite = startBackground;
                break;
            }
        }

        Corridor newCorridor = new Corridor();
        newCorridor.obstacleid = -1;
        newCorridor.corridor = startingCorridor;

        if (addToStart)
            corridorList.Insert(0, newCorridor);
        else
            corridorList.Add(newCorridor);

        groupIndex++;
    }

    void StartGame(System.Object obj, EventArgs args)
    {
        // Start our button animations
        StartButtonAnimations();

        for (int i = 0; i < corridorList.Count; i++)
        {
            if (!corridorList[i].corridor.CompareTag("StartingCorridor"))
            {
                corridorList[i] = RandomObstacle(corridorList[i], i);
            }

            if (corridorList[i].corridor.tag == "Untagged")
            {
                Debug.Log(corridorList[i].corridor.tag);
                corridorList[i] = RandomObstacle(corridorList[i], i);
            }
        }
    }

    private Corridor RandomObstacle(Corridor corr, int corridorIndex)
    {
        int rand = 0;
        int count = 0;
        bool complete = false;

        while (!complete)
        {
            rand = UnityEngine.Random.Range(0, pooledObstacles.Count);
            count++;
            if (!pooledObstacles[rand].obstacle.activeSelf)
            {
                if (pooledObstacles[rand].obstacleType != previousObstacle &&
                    !(pooledObstacles[rand].obstacleType == TypeOfObstacle.TrapDoor && previousObstacle == TypeOfObstacle.HangingBall))
                {
                    complete = true;
                    previousObstacle = pooledObstacles[rand].obstacleType;
                    if (UIController.currentMode == UIController.GameMode.Easy)
                    {
                        corr = SetObstacleEasy(rand, corr);
                    }
                    else
                    {
                        corr = SetObstacleHard(rand, corr);
                    }                 
                    ResetObstacle(corr, corridorIndex + 1);
                    if (corr.corridor.CompareTag("TrapDoor"))
                    {
                        foreach (Transform child in corr.corridor.transform)
                        {
                            if (child.CompareTag("CorridorFloor"))
                            {
                                child.gameObject.SetActive(false);
                                break;
                            }
                        }
                    }
                }
            }

            if (count > pooledObstacles.Count)
                complete = true;
        }

        return corr;
    }

    private Corridor SetObstacleHard(int obstacleID, Corridor corr)
    {
        foreach (Transform child in corr.corridor.transform) // Find the background
        {
            if (child.gameObject.CompareTag("CorridorBackground"))
            {
                child.GetComponent<SpriteRenderer>().sprite = RandomBackground(); // Set its background colour
                corr.obstacleid = obstacleID;
                corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                pooledObstacles[obstacleID].obstacle.SetActive(true);
                break;
            }
        }
        return corr;
    }

    private Sprite RandomBackground()
    {
        int rand = 0;
        bool complete = false;
        Sprite returnSprite = null;

        while (!complete)
        {
            rand = UnityEngine.Random.Range(0, numberOfColouredBackgrounds);
            switch (rand)
            {
                case 0:
                    returnSprite = blueBackground;
                    break;

                case 1:
                    returnSprite = redBackground;
                    break;

                case 2:
                    returnSprite = greenBackground;
                    break;

                case 3:
                    returnSprite = lightBlueBackground;
                    break;

                case 4:
                    returnSprite = orangeBackground;
                    break;

                case 5:
                    returnSprite = purpleBackground;
                    break;
            }
            if (returnSprite != previousBackground)
            {
                complete = true;
                previousBackground = returnSprite;
                return returnSprite;
            }
        }

        return blueBackground;
    }

    private Corridor SetObstacleEasy(int obstacleID, Corridor corr)
    {
        switch (pooledObstacles[obstacleID].obstacleType)
        {
            case TypeOfObstacle.BottomSpike:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = greenBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;

            case TypeOfObstacle.ThrownWeapon:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = blueBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;

            case TypeOfObstacle.Enemy:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = redBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;

            case TypeOfObstacle.EnemyShooter:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = redBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;

            case TypeOfObstacle.HangingBall:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = blueBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;

            case TypeOfObstacle.TrapDoor:
                foreach (Transform child in corr.corridor.transform) // Find the background
                {
                    if (child.gameObject.CompareTag("CorridorBackground"))
                    {
                        child.GetComponent<SpriteRenderer>().sprite = greenBackground; // Set its background colour
                        corr.obstacleid = obstacleID;
                        corr.corridor.tag = pooledObstacles[obstacleID].obstacleType.ToString(); // Set it's tag
                        pooledObstacles[obstacleID].obstacle.transform.parent = corr.corridor.transform;
                        pooledObstacles[obstacleID].obstacle.SetActive(true);
                        break;
                    }
                }
                break;
        }
        return corr;
    }

    private void StartButtonAnimations()
    {
        btnStart.GetComponent<Animator>().SetTrigger("slidein");
        btnStart.GetComponent<Animator>().ResetTrigger("Started");

        btnLeft.GetComponent<Transform>().localPosition = new Vector3(-155f, -492f, 0f);
        btnRight.GetComponent<Transform>().localPosition = new Vector3(155f, -492f, 0f);
        btnLeft.GetComponent<Animator>().StopPlayback();
        btnLeft.GetComponent<Animator>().StartPlayback();
        btnLeft.SetActive(false);
        btnRight.GetComponent<Animator>().StopPlayback();
        btnRight.GetComponent<Animator>().StartPlayback();
        btnRight.SetActive(false);
    }

    void Restarting(System.Object obj, EventArgs args)
    {
        gameStarted = false;
        player.GetComponent<Transform>().position = new Vector3(0.111f, -2.8f, 0);

        if (player.GetComponent<PlayerController>().forwardMovementSpeed > 0f)
        {
            player.GetComponent<PlayerController>().forwardMovementSpeed = player.GetComponent<PlayerController>().startingSpeed;
        }
        else
        {
            player.GetComponent<PlayerController>().forwardMovementSpeed = -player.GetComponent<PlayerController>().startingSpeed;
        }

        startPosY = player.GetComponent<Transform>().position.y;
        groupIndex = 1;
        currentCorridorID = 1;

        foreach (Corridor corr in pooledStartCorridors)
        {
            corridorList.Insert(0, corr);
        }

        pooledStartCorridors.Clear();

        StartButtonAnimations();

        player.GetComponent<Animator>().SetTrigger("Ressurect");

        player.GetComponent<PlayerController>().dead = false;
        player.GetComponent<PlayerController>().moving = false;
        gameStarted = false;

        for (int i = 0; i < corridorList.Count; i++)
        {
            corridorList[i].corridor.transform.position = new Vector3(0, (groupIndex == 1) ? groupIndex + negativeYDistance : corridorList[groupIndex - 2].corridor.transform.position.y + distanceBetweenCorridorsVertical, 0);
            ResetObstacle(corridorList[i], i + 1);
            groupIndex++;
            highestCorridorPosY = corridorList[i].corridor.transform.position.y;
        }
    }
}
