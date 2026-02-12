using UnityEngine;

public class UI : MonoBehaviour
{
    
    float deltaTime = 0.0f;


    
    // Update is called once per frame
    void Update()
    {
        // Smooth delta time for stable FPS reading
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }
    
    void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 0.02f);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        string text = string.Format("FPS: {0:0.}", fps);

        GUI.Label(rect, text, style);
    }
}
