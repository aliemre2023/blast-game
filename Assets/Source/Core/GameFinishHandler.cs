using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameFinishHandler : MonoBehaviour
{
    public Image popup_base;
    public TextMeshProUGUI congrats;
    public TextMeshProUGUI unfortunately;

    public Canvas mainScreen; 
    public Canvas gameScreen;
    public TextMeshProUGUI levelText;


    public void ShowWinPopup()
    {
        StartCoroutine(ShowPopup(popup_base, congrats));
        int levelNum = PlayerPrefs.GetInt("LevelNum", 1);
        levelNum %= 10;
        levelNum++;
        levelText.text = $"Level {levelNum}";
        
        PlayerPrefs.SetInt("LevelNum", levelNum);
        PlayerPrefs.Save(); 
    }

    public void ShowLosePopup()
    {
        StartCoroutine(ShowPopup(popup_base, unfortunately));
    }

    private IEnumerator ShowPopup(Image popup, TextMeshProUGUI message)
    {
        if (popup == null || message == null)
        {
            Debug.LogError("Popup or message is not assigned.");
            yield break;
        }

        // Make the popup and message visible
        popup.gameObject.SetActive(true);
        message.gameObject.SetActive(true);

        // Wait for 3 seconds
        yield return new WaitForSeconds(3);

        // Hide the popup and message
        popup.gameObject.SetActive(false);
        message.gameObject.SetActive(false);

        CloseCanvas();
    }

    private void CloseCanvas()
    {
        if (gameScreen != null)
        {
            gameScreen.gameObject.SetActive(false);
        }

        if (mainScreen != null)
        {
            mainScreen.gameObject.SetActive(true);
        }
    }
}