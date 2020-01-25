using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InDevCreateWorldButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameField;

    // Start is called before the first frame update
    public void Run()
    {
        //Get world folder
        var worldDirectoryPath = Path.Combine(Application.persistentDataPath, "World");
        //Get world name from user input
        var worldName = _nameField.text;
        //Check if worldName is set, if empty, set to a valid default
        if (worldName == string.Empty)
        {
            worldName = "Default World";
        }
        else
        {
            //Otherwise we need to sanitize our input, IM LAZY and dont want to do that but lets assume I do that here
            //TODO sanitize world name
            //we might need to do this before checking for empty if we sanitize the entire world name and make it empty
        }

        //Create a full path
        var worldFullPath = Path.Combine(worldDirectoryPath, worldName);

        //Create the directory (
        var worldDirectory = Directory.CreateDirectory(worldFullPath);

        Debug.Log(worldFullPath);
    }
}