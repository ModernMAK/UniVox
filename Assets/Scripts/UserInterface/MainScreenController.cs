using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenController : MonoBehaviour
{
    [Serializable]
    public struct ScreenTransition
    {
        public GameObject Screen;
//        public Selectable Target;
    }

    [Serializable]
    public struct NewWorldScreenData
    {
        public TMP_InputField Input;
    }

    // Start is called before the first frame update
    void Start()
    {
        _prev = MainTitle;
        MainTitle.Screen.SetActive(true);
        NewWorld.Screen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }


    private ScreenTransition _prev;
    public ScreenTransition NewWorld;
    public NewWorldScreenData NewWorldData;
    public ScreenTransition MainTitle;

    public void Click_MainTitle_NewWorld()
    {
        ApplyTransition(NewWorld);
    }

    public void Click_MainTitle_LoadWorld()
    {
    }

    public void Click_MainTitle_Quit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void Click_NewWorld_Create()
    {
        var worldDir = Path.Combine(Application.persistentDataPath, "Saves", NewWorldData.Input.text);
        Directory.CreateDirectory(worldDir);
        Debug.Log(worldDir);
    }

    public void Click_NewWorld_Back()
    {
        ApplyTransition(MainTitle);
    }

    private void ApplyTransition(ScreenTransition transition)
    {
        _prev.Screen.SetActive(false);
        transition.Screen.SetActive(true);
        _prev = transition;
    }
}