using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{

    private NetworkManagerCustom nm;

    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject helpPanel;
    [SerializeField] GameObject connectingPanel;
    [SerializeField] GameObject serverIsOfflinePanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] TMP_InputField lobbyCodeInput;

    public AudioSource audioSource;
    public AudioClip MouseOver;
    public AudioClip PressedDown;

    [SerializeField] TMP_InputField playerNameInput;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        nm = NetworkManagerCustom.Instance;

        if (!PlayerPrefs.HasKey("playerName"))
        {
            PlayerPrefs.SetString("playerName", "PlayerName");
        }

        playerNameInput.text = PlayerPrefs.GetString("playerName");
        nm.lrm.diconnectedFromRelay.AddListener(FailedToConnectToRelay);
    }

    public void LoadGamePanel()
    {
        if (nm.lrm.Available())
        {
            nm.lrm.Shutdown();
        }
        playerNameInput.gameObject.SetActive(true);
        gamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        connectingPanel.SetActive(false);
        serverIsOfflinePanel.SetActive(false);
        lobbyPanel.SetActive(false);
        helpPanel.SetActive(false);
        audioSource.PlayOneShot(PressedDown);
    }

    public void LoadMainMenuPanel()
    {
        playerNameInput.gameObject.SetActive(true);
        mainMenuPanel.SetActive(true);
        gamePanel.SetActive(false);
        connectingPanel.SetActive(false);
        serverIsOfflinePanel.SetActive(false);
        lobbyPanel.SetActive(false);
        helpPanel.SetActive(false);
        audioSource.PlayOneShot(PressedDown);
    }

    public void LoadHelpPanel()
    {
        mainMenuPanel.SetActive(false);
        helpPanel.SetActive(true);
        audioSource.PlayOneShot(PressedDown);
    }

    public void OfflineLobby()
    {
        nm.CreateLobby();
        audioSource.PlayOneShot(PressedDown);
    }

    public void OnlineLobby()
    {
        playerNameInput.gameObject.SetActive(false);
        gamePanel.SetActive(false);
        connectingPanel.SetActive(true);
        nm.ConnectToRelay();
        StartCoroutine(ConnectingToRelay());
        audioSource.PlayOneShot(PressedDown);
    }

    public void CreateLobby()
    {
        nm.CreateLobby();
        audioSource.PlayOneShot(PressedDown);
    }

    public void JoinLobby()
    {
        nm.networkAddress = lobbyCodeInput.text;
        nm.StartClient();
        audioSource.PlayOneShot(PressedDown);
    }

    public void FailedToConnectToRelay()
    {
        StopCoroutine(ConnectingToRelay());
        connectingPanel.SetActive(false);
        gamePanel.SetActive(true);
        //serverIsOfflinePanel.SetActive(true);
    }

    public void QuitGame()
    {
        audioSource.PlayOneShot(PressedDown);
        Application.Quit();
    }

    IEnumerator ConnectingToRelay()
    {
        float rand = Random.Range(0.5f, 1.5f);
        yield return new WaitForSecondsRealtime(rand);
        yield return new WaitUntil(() => nm.lrm.Available());
        connectingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void SetPlayerName()
    {
        if(!string.IsNullOrEmpty(playerNameInput.text))
        {
            PlayerPrefs.SetString("playerName", playerNameInput.text);
        }
        else
        {
            playerNameInput.text = "SomeDumbName";
            PlayerPrefs.SetString("playerName", playerNameInput.text);
        }
    }

    public void OnMouseOver()
    {
        audioSource.PlayOneShot(MouseOver);
    }
}
