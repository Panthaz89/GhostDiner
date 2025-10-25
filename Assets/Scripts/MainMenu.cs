using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void PlayGame()
    {
        // Make sure your game scene is in Build Settings!
        SceneManager.LoadScene("Diner"); 
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // This line only runs inside the editor for testing:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}