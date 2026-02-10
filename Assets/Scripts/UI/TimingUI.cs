using UnityEngine;
using UnityEngine.UI;

public class TimingUI : MonoBehaviour
{
    [SerializeField]
    private Slider HandleSlider;
    [SerializeField]
    private Slider PointSlider;
    private const float POINT_INTERVAL = 0.045f;
    private const float PROCESS_TIME = 3f;
    private float currTime = 0f;
    void Update()
    {
        
    }
}
