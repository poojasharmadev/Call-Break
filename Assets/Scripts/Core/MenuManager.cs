using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
   public class MenuManager : MonoBehaviour
   {
      public void OnPlay()
      {
         SceneManager.LoadScene("GameScene");
      }
   
      public void OnQuit()
      {
         Application.Quit();
      }
   }
}
