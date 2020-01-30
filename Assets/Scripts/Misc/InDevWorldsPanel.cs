using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InDevWorldsPanel : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private Button _loadButton;
    [SerializeField] private int _size;
    private Button _selected;

    [SerializeField] private InDevWorldInformation _worldInfo;
    [SerializeField] private SceneAsset _scene;
#pragma warning restore 0649

    
    private void Cleanup()
    {
        _loadButton.interactable = false;
        _selected = null;
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }
    }

    public void Setup()
    {
        Cleanup();

        //Get world folder
        var worldDirectoryPath = Path.Combine(Application.persistentDataPath, "World");
        var worldDirectory = new DirectoryInfo(worldDirectoryPath);

        var items = 0;
        //Iterate over subdirectories (the world-saves in this case)
        foreach (var subDirectory in worldDirectory.GetDirectories())
        {
            //We should have some check to validate stuff, but im lazy
            var worldButtonObj = Instantiate(_buttonPrefab, _content, true);
            var worldButton = worldButtonObj.GetComponent<Button>();
            var buttonText = worldButtonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = subDirectory.Name;

            worldButton.onClick.AddListener(() => Select(worldButton));
            items++;
        }

        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, items * _size);
    }

    public void Select(Button button)
    {
        if (_selected != null)
        {
            _selected.interactable = true;
        }

        _selected = button;
        if (_selected != null)
        {
            _selected.interactable = false;
        }

        _loadButton.interactable = (_selected != null);
    }

    public void Run()
    {
        if (_selected != null)
        {
            var buttonText = _selected.GetComponentInChildren<TMP_Text>();

            Debug.Log($"TODO - Load World : {buttonText.text}");

            _worldInfo.WorldName = buttonText.text;

            SceneManager.LoadScene(_scene.name);
        }
    }
}