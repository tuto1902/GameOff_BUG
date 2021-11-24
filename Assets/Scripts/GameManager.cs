using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverMessage;
    [SerializeField] private GameObject gameWinMessage;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip gameWinSound;
    [SerializeField] private GameObject countDownText;
    [SerializeField] private GameObject waveNameText;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void WinGame()
    {
        gameWinMessage.SetActive(true);
        Button restartButton = gameWinMessage.gameObject.GetComponentInChildren<Button>();
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.PlaySound(gameWinSound);
    }

    public void GameOver()
    {
        gameOverMessage.SetActive(true);
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.PlaySound(gameOverSound);
    }

    public void Restart()
    {
        SoundManager.Instance.StopSound();
        SoundManager.Instance.PlayMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }

    public void ShowCountdown()
    {
        countDownText.SetActive(true);
    }

    public void HideCountdown()
    {
        countDownText.SetActive(false);
    }

    public void ShowWaveName()
    {
        waveNameText.SetActive(true);
    }

    public void HideWaveName()
    {
        waveNameText.SetActive(false);
    }

    public void SetCountdownText(string text)
    {
        countDownText.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void SetWaveNameText(string text)
    {
        waveNameText.GetComponent<TextMeshProUGUI>().text = text;
    }
}
