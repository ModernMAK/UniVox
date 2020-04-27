using UnityEngine;

namespace UniVox.Unity
{
    public class InDevDDOL : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}