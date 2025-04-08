using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BallTracker : MonoBehaviour
{
    public string csvFileName = "ball_coordinates"; // Name of the CSV file (without .csv)
    public GameObject ball;
    public float frameRate = 30f; // 30 FPS
    private List<Vector3> ballPositions = new List<Vector3>();
    private int currentFrame = 0;
    private LineRenderer lineRenderer;

    void Start()
    {
        // Add LineRenderer to ball
        lineRenderer = ball.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = ball.AddComponent<LineRenderer>();
        }
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.blue;

        LoadCoordinates();
        if (ballPositions.Count > 0)
        {
            StartCoroutine(TrackBall());
        }
        else
        {
            Debug.LogError("No valid ball positions loaded.");
        }
    }

    void LoadCoordinates()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName + ".csv");
        Debug.Log("Looking for file at: " + filePath);

        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at path: " + filePath);
            return;
        }
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] values = line.Trim().Split(',');
            if (values.Length >= 3)
            {
                float x, y, z;
                if (float.TryParse(values[0].Trim(), out x) &&
                    float.TryParse(values[1].Trim(), out y) &&
                    float.TryParse(values[2].Trim(), out z))
                {
                    ballPositions.Add(new Vector3(x, y, z));
                }
                else
                {
                    Debug.LogWarning("Skipping invalid row: " + line);
                }
            }
        }
        Debug.Log("Loaded " + ballPositions.Count + " positions.");
    }

    IEnumerator TrackBall()
    {
        while (currentFrame < ballPositions.Count)
        {
            if (ball == null)
            {
                Debug.LogError("Ball GameObject is not assigned!");
                yield break;
            }
            ball.transform.position = ballPositions[currentFrame];

            // Update LineRenderer to draw the path
            lineRenderer.positionCount = currentFrame + 1;
            lineRenderer.SetPosition(currentFrame, ball.transform.position);

            currentFrame++;
            yield return new WaitForSeconds(1f / frameRate);
        }
    }
}
