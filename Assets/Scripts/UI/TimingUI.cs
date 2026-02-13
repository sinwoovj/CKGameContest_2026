using UnityEngine;
using UnityEngine.UI;

public class TimingUI : MonoBehaviour
{
    [SerializeField]
    private Slider timingSlider;
    [SerializeField]
    private Slider pointSlider;
    private const float POINT_INTERVAL = 0.1f;
    private const float SUCCESSZONE_INTERVAL = 0.1f;
    [Range(0f, 1f)]
    public float speed = 1f;
    private float timer;
    private bool isProcess = false;
    private void Update()
    {
        if (isProcess)
        {
            // 게이지 왕복 이동
            timer += Time.deltaTime * speed;
            timingSlider.value = Mathf.PingPong(timer, 1f);
        }
    }
    public void Init()
    {
        isProcess = true;
        timingSlider.value = 0;
        pointSlider.value = Random.Range(POINT_INTERVAL, 1 - POINT_INTERVAL);
        gameObject.SetActive(true);
    }
    public void End()
    {
        isProcess = false;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    public bool CheckTiming() => (Mathf.Abs(pointSlider.value - timingSlider.value) <= SUCCESSZONE_INTERVAL);
}
