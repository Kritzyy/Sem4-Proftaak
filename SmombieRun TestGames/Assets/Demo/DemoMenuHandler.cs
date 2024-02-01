using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DemoMenuHandler : MenuHandler
{
    private void Start()
    {
        // Get objects from Object Scene
        if (FindAnyObjectByType(typeof(EventSystem)) == null)
        {
            Debug.Log("No Objects found yet, loading objects.");
            SceneManager.LoadScene("ObjectsScene", LoadSceneMode.Additive);
        }
    }

    new public void Connect()
    {
        SceneManager.LoadScene("DemoGame");
    }

    new public void SwitchToDemo()
    {
        SceneManager.LoadScene("DemoPlaytest");
    }

    
}
