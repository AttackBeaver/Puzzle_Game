using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_UI : MonoBehaviour
{
    // Флаг для отслеживания состояния звука (включён/выключен)
    private bool isSoundOn = true;

    // Метод для перезапуска текущей сцены
    public void RestartScene()
    {
        // Получаем текущую активную сцену
        Scene currentScene = SceneManager.GetActiveScene();
        // Перезагружаем её по имени
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// Полный выход из приложения.
    /// При запуске в редакторе выводит сообщение.
    /// </summary>
    public void ExitApplication()
    {
#if UNITY_EDITOR
        Debug.Log("ExitApplication called. In Editor, Application.Quit() не работает.");
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Переключает состояние звука:
    /// Если звук включён, выключает его (AudioListener.volume = 0),
    /// если выключен – включает (AudioListener.volume = 1).
    /// </summary>
    public void ToggleSound()
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

    /// <summary>
    /// Переходит в сцену Main_Menu.
    /// Убедитесь, что сцена "Main_Menu" добавлена в Build Settings.
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Level_1");
    }
}