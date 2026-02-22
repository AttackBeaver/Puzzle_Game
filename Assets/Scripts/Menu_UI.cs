using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_UI : MonoBehaviour
{
    private bool isSoundOn = true;

    private void Start()
    {
        // Если GameManager ещё не создан (например, при первом запуске), создаём его
        if (GameManager.Instance == null)
        {
            // Здесь можно загрузить префаб GameManager, но проще убедиться, что он есть в сцене
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
            Debug.Log("Ошибка Новой игры");
        }
    }

    public void ContinueGame()
    {
        if (GameManager.Instance != null)
        {
            // Проверяем, есть ли сохранённый прогресс (если уровень > 1, но можно просто загрузить)
            // GameManager сам загрузит данные из файла при старте
            SceneManager.LoadScene("Level");
        }
        else
        {
            Debug.Log("Ошибка Продолжить");
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
        // Используем AudioManager, если он есть
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
        else
        {
            // Запасной вариант
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