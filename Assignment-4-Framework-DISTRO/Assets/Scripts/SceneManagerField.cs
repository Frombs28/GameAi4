using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerField : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
