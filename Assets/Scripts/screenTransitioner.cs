using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class screenTransitioner : MonoBehaviour
{
    Animator sceneAnim;

    public void Awake()
    {
        sceneAnim = GetComponent<Animator>();
    }

    public void triggerTransition()
    {
        StartCoroutine("nextScene");
    }

    IEnumerator nextScene()
    {
        sceneAnim.SetTrigger("nextScene");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
