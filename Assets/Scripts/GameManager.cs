using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager _instance;

    //private AudioManager audioManager;
    public AudioSource audioSource;

    public static GameManager Instance { get { return _instance; } }

    private NetworkManagerCustom nm;

    public HUDManager hud;

    public List<GameObject> avatarList;

    [SerializeField] private Transform spawnPoints;

    private Transform[] spawnPoint = new Transform[4];

    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private float gameEndedDelayTime;

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        nm = NetworkManagerCustom.Instance;
        avatarList = new List<GameObject>();
        if (isClientOnly) return;
        //audioManager = AudioManager.instance;
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();

        SetSpawnPoints();

        SpawnPlayers();
        
    }

    private void Update()
    {
        PauseMenuInput();
    }

    public void AddToAvatarList(GameObject avatar)
    {
        avatarList.Add(avatar);
    }

    private void PauseMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }

    private void SetSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.childCount; i++)
        {
            spawnPoint[i] = spawnPoints.GetChild(i);
        }
    }

    private void SpawnPlayers()
    {
        if (isClientOnly) return;
        
        float rot = 0;

        for (int i = 0; i < nm.playerList.transform.childCount; i++)
        {

            if (i == 1 || i == 3) 
            { 
                rot = 180; 
            } 
            else
            {
                rot = 0;
            }

            Vector3 pos = spawnPoint[i].position;
            PlayerLobby pn = nm.playerList.transform.GetChild(i).GetComponent<PlayerLobby>();
            pn.CmdSpawnPlayer(i + 1, pn.gamepadNum, pos, rot);
        }
    }

    public void CheckForPlayerRemaining()
    {
        print("checcking player remaining");
        int aliveNum = 0;
        foreach (var item in avatarList)
        {
            PlayerStats s = item.GetComponent<PlayerStats>();
            if (s.alive) aliveNum += 1;     
        }

        if(aliveNum <= 1)
        {
            if (aliveNum == 0)
            {
                print("GameEnded in tie");
            }
            else if (aliveNum == 1)
            {
                print("GameEnded");
            }

            RestartGame();
        }
    }

    private int GetWinner()
    {
        int winNum = -1;

        foreach (var item in avatarList)
        {
            PlayerStats s = item.GetComponent<PlayerStats>();
            if (s.alive) winNum = s.playerNum;
        }
        return winNum;
    }

    private void RestartGame()
    {
        StartCoroutine(GameEndDelay());
    }

    private IEnumerator GameEndDelay()
    {
        hud.AnnounceWinner(GetWinner());
        yield return new WaitForSeconds(gameEndedDelayTime);
        DestroyAllPlayers();
        SpawnPlayers();

        //hud.UpdatePlayerHUD();
    }

    private void DestroyAllPlayers()
    {
        if (!isClientOnly)
        {
            for (int i = 0; i < avatarList.Count; i++)
            {
                NetworkServer.Destroy(avatarList[i].gameObject);
            }

            RpcResetAvatarList();
        }
    }

    private void ResetAvatarList()
    {
        avatarList.Clear();
    }

    [ClientRpc]
    private void RpcResetAvatarList()
    {
        ResetAvatarList();
    }

    public void Disconnect()
    {
        nm.numOfActivePlayers = 0;

        if (nm.lrm.Available())
        {
            nm.StopServer();
            nm.StopHost();
            nm.StopClient();
            nm.lrm.Shutdown();
        }
        else
        {
            nm.StopServer();
            nm.StopHost();
        }
    }
}
