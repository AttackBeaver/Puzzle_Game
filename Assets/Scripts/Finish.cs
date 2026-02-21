using UnityEngine;

public class Finish : MonoBehaviour
{
    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.LevelCompleted();
        }
    }
}
