using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenCap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            var timestamp = DateTime.Now;
            var fName = timestamp.ToString().Replace("/", "-").Replace(":","-"); // Python-esqe code to get the job done
            var dName = Path.Combine(Application.persistentDataPath, "Screen Captures");
            var fPath = Path.Combine(dName, $"{fName}.png");
            System.IO.Directory.CreateDirectory(dName);
            ScreenCapture.CaptureScreenshot(fPath);
            Debug.Log($"Saved to '{fPath}'");
        }
        
    }
}
