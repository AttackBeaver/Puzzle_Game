using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_UI : MonoBehaviour
{
    // Метод для перезапуска текущей сцены
    public void RestartScene()
    {
        // Получаем текущую активную сцену
        Scene currentScene = SceneManager.GetActiveScene();
        // Перезагружаем её по имени
        SceneManager.LoadScene(currentScene.name);
    }
}