using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class EmptyScene : MonoBehaviour
    {
        void Start() {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
}
