using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class CutsceneHolder : MonoBehaviour
{
    [SerializeField] VideoPlayer video;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject key;

    private bool canSkip = false;
    private int startLoading = 0;
    void FixedUpdate()
    {
        StartCoroutine(CanSkip());

        if (startLoading == 0)
        {
            StartCoroutine(EndCutscene());
        }

        if (startLoading == 0 && canSkip && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            startLoading++;
            Destroy(video, 0.2f);
            SceneManager.LoadScene("Hall");
        }
    }

    IEnumerator CanSkip()
    {
        yield return new WaitForSeconds(5f);

        canSkip = true;
        text.enabled = true;
        key.SetActive(true);
    }

    IEnumerator EndCutscene()
    {
        yield return new WaitForSeconds((float)video.clip.length);
        startLoading++;
        SceneManager.LoadScene("Hall");
    }
}
