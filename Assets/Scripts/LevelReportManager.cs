using System;
using System.IO;
using UnityEngine;

public class LevelReportManager : MonoBehaviour
{
    public void GenerateCompletionReport(int levelSeed)
    {
        string reportContent = "MAZE COMPLETION REPORT\n";
        reportContent += "======================\n\n";
        reportContent += "Status: SUCCESS - Level Completed!\n";
        reportContent += "Level Seed: " + levelSeed + "\n";
        reportContent += "Date Completed: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n";
        reportContent += "Great job navigating the maze and defeating the enemies!";

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        string fileName = "MazeReport_Level_" + levelSeed + ".txt";
        
        string fullFilePath = Path.Combine(desktopPath, fileName);

        try
        {
            File.WriteAllText(fullFilePath, reportContent);
            
            Debug.Log("<color=green>Report Downloaded successfully to your Desktop!</color> Path: " + fullFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save the report. Error: " + e.Message);
        }
    }
}