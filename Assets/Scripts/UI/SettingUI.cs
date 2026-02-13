using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shurub
{
    public class SettingUI : UIBase<SettingUI>
    {
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [SerializeField] private TMP_Text bgmPercentText;
        [SerializeField] private TMP_Text sfxPercentText;

        [SerializeField] private Button controlButton;
        [SerializeField] private Button exitButton;

        protected override void OnAwake()
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();

            resolutionDropdown.onValueChanged.AddListener(OnValueChangedResolutionDropdown);
            bgmSlider.onValueChanged.AddListener(OnValueChangedBgmSlider);
            sfxSlider.onValueChanged.AddListener(OnValueChangedSfxSlider);

            resolutionDropdown.value = 0;
            bgmSlider.value = 0.5f;
            sfxSlider.value = 0.5f;
        }

        private void OnValueChangedResolutionDropdown(int value)
        {
            switch (value)
            {
                case 0:
                    Screen.SetResolution(1920, 1080, Screen.fullScreen);
                    break;
                case 1:
                    Screen.SetResolution(1600, 900, Screen.fullScreen);
                    break;
                case 2:
                    Screen.SetResolution(1280, 720, Screen.fullScreen);
                    break;
            }
        }

        private void OnValueChangedBgmSlider(float value)
        {
            SoundManager.Instance.SetAudioMixerVolume(SoundType.BGM, value);
            UpdateBgmPercentText();
        }

        private void OnValueChangedSfxSlider(float value)
        {
            SoundManager.Instance.SetAudioMixerVolume(SoundType.SFX, value);
            UpdateSfxPercentText();
        }

        private void UpdateBgmPercentText()
        {
            float normalizedValue = Mathf.InverseLerp(0.0001f, 1f, bgmSlider.value);
            bgmPercentText.text = $"{(int)(normalizedValue * 100f)}%";
        }

        private void UpdateSfxPercentText()
        {
            float normalizedValue = Mathf.InverseLerp(0.0001f, 1f, sfxSlider.value);
            sfxPercentText.text = $"{(int)(normalizedValue * 100f)}%";
        }

        private void OnClickControlButton()
        {

        }

        private void OnClickExitButton()
        {
            ModalManager.Instance.OpenNewModal("확인", "게임을 종료 하시겠습니까?", yesAction: Application.Quit);
        }
    }
}
