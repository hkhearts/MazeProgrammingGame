using UnityEngine;
using UnityEngine.SceneManagement;

public class TressureScript : MonoBehaviour
{
    [Tooltip("The ID or Seed printed on the text file report.")]
    public int levelID = 2;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelReportManager reportManager = FindObjectOfType<LevelReportManager>();
            if (reportManager != null)
            {
                reportManager.GenerateCompletionReport(levelID);
            }
            else
            {
                Debug.LogWarning("LevelReportManager not found in the scene! Couldn't print report.");
            }

            SceneManager.LoadScene(2);
        }
    }
}