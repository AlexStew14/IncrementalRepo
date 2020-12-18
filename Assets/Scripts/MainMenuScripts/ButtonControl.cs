using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// OnClick method for the start button being pressed 
    /// </summary>
    public void StartPressed()
    {
        SceneManager.LoadScene("EricTesting");
    }

    /// <summary>
    /// OnClick method for the start button being pressed 
    /// </summary>
    public void HowToPlayPressed()
    {
        
    }

    /// <summary>
    /// OnClick method for the start button being pressed 
    /// </summary>
    public void SettingsPressed()
    {
        
    }

    /// <summary>
    /// OnClick method for the start button being pressed 
    /// </summary>
    public void QuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
