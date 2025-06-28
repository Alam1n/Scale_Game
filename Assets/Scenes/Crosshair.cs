using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Color crosshairColor = Color.white;
    public float crosshairSize = 4f;
    public bool showCrosshair = true;

    void OnGUI()
    {
        if (!showCrosshair) return;

        // Calculate center of screen
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        // Create a small rect for the crosshair dot
        Rect crosshairRect = new Rect(
            centerX - crosshairSize / 2f,
            centerY - crosshairSize / 2f,
            crosshairSize,
            crosshairSize
        );

        // Draw the crosshair
        GUI.color = crosshairColor;
        GUI.DrawTexture(crosshairRect, Texture2D.whiteTexture);
        GUI.color = Color.white; // Reset color
    }
}