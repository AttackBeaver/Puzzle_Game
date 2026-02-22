using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_UI : MonoBehaviour
{
    private bool isSoundOn = true;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Please add it to the scene.");
        }
    }

    public void NewGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress(); // сброс сохранения и данных
            SceneManager.LoadScene("Level");
        }
        else
        {
            Debug.LogError("Error: Start New Game");
        }
    }

    public void ContinueGame()
    {
        if (GameManager.Instance != null)
        {
            SceneManager.LoadScene("Level");
        }
        else
        {
            Debug.LogError("Error: Continue Game");
        }
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        Debug.Log("ExitApplication called.");
#else
        Application.Quit();
#endif
    }

    public void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
        else
        {
            if (isSoundOn)
            {
                AudioListener.volume = 0f;
                isSoundOn = false;
            }
            else
            {
                AudioListener.volume = 1f;
                isSoundOn = true;
            }
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }

    public void OpenInfopanel()
    {
        SceneManager.LoadScene("Info_Panel", LoadSceneMode.Additive);
    }

    public void CloseInfo()
    {
        SceneManager.UnloadSceneAsync("Info_Panel");
    }
}