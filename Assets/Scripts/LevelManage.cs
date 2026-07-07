using UnityEngine;

public class LevelManage : MonoBehaviour
{
    public GameObject Scene1;
    public GameObject Scene2;
    
    [Tooltip("The ID or Seed printed on the text file report.")]
    public int levelID = 1; 

    private void OnTriggerEnter(Collider other)
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

            Scene2.SetActive(true);
            Scene1.SetActive(false);
        }
    }
}