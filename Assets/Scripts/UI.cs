using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Slider progressBar;
    public PlayerController playerController;
    public float progress;
    public bool activateSlider;
    void Update()
    {
        activateSlider = playerController.isWallRunning;
        if (activateSlider)
        {
            progressBar.gameObject.SetActive(true);
        }
        else
        {
            progressBar.gameObject.SetActive(false);
        }
        progress = playerController.wallRunTimer;
        progressBar.value = progress;
    }
}
