using UnityEngine;

[CreateAssetMenu(menuName = "Data/Registry/Key",fileName = "Key.rk.asset")]
public class RegistryKey : ScriptableObject
{
    [SerializeField] private RegistryKey _parent;
    [SerializeField] private string _key;
    [SerializeField] private char _customSeparator = Separator;
    private const char Separator = '/';

    public RegistryKey Parent => _parent;
    public string Key => _key;

    public virtual string GetFullKey()
    {
        var localKey = _key;
        if (_customSeparator != Separator)
        {
            localKey = localKey.Replace(_customSeparator, Separator);
        }

        var fullKey = localKey;
        if (_parent != null)
        {
            var parentKey = _parent.GetFullKey();
            fullKey = $"{parentKey}{Separator}{localKey}";
        }

        return fullKey;
    }
}