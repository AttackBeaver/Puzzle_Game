using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    private void Start()
    {
        if (GameManager.Instance != null)
            UpdateLevelText(GameManager.Instance.currentGameData.currentLevel);
    }

    public void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = "LVL " + level;
    }

    [System.Obsolete]
    public void OnRestartButton()
    {
        GameManager.Instance.RestartLevel();
    }
}
