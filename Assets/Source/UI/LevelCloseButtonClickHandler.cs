using UnityEngine;
using UnityEngine.UI;

public class LevelCloseButtonClickHandler : MonoBehaviour
{
    public Button levelCloseButton;
    public Canvas mainScreen;
    public Canvas gameScreen;

    void Start()
    {
       levelCloseButton.onClick.AddListener(CloseCanvas);
    }

    void CloseCanvas()
    {
        
        gameScreen.gameObject.SetActive(false);
        mainScreen.gameObject.SetActive(true);
    }
}
