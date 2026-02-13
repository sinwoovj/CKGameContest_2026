using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class OrderUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text FoodNameText;
        [SerializeField] private TMP_Text RemainTimeText;
        [SerializeField] private TMP_Text RecoveryHpCountText;
        [SerializeField] private Transform RightSideTransform;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Image SetImage;
        [SerializeField] private Image timerSliderFill;
        [SerializeField] private GameObject IGUIPrefab;
        [SerializeField] private GameObject PlusTextPrefab;

        private int orderId;
        private float totalTime;

        public void Init(int id, int recipeType, float time)
        {
            orderId = id;
            totalTime = time;

            timerSlider.maxValue = time;
            timerSlider.value = time;

            Recipe recipe = OrderManager.Instance.GetRecipe(recipeType);

            FoodNameText.text = recipe.recipeName;
            RemainTimeText.text = $"{(int)(time / 60):00} : {(int)(time % 60):00}";
            RecoveryHpCountText.text = "+10";
            RecoveryHpCountText.color = Color.green;
            for (int i = 0; i < recipe.requiredIngredients.Count; i++)
            {
                if (i > 0)
                {
                    Instantiate(PlusTextPrefab, RightSideTransform);
                }
                Instantiate(IGUIPrefab, RightSideTransform).GetComponent<OrderIGUI>().
                    SetSprite(IngredientManager.Instance.ingredientSprites[(int)recipe.requiredIngredients[i]]);
            }
            SetImage.sprite = IngredientManager.Instance.setSprites[recipeType];
        }

        public void UpdateTime(float time)
        {
            timerSlider.value = time;
            RemainTimeText.text = $"{(int)(time / 60):00} : {(int)(time % 60):00}";

            float ratio = time / totalTime;
            RecoveryHpCountText.text = "+" + CalculateHpReward(ratio);
            RecoveryHpCountText.color = CalculateHpRewardColor(ratio);
            timerSliderFill.color = CalculateHpRewardColor(ratio);
        }
        int CalculateHpReward(float ratio)
        {
            if (ratio >= 0.5f)
                return 10;
            else if (ratio >= 0.25f)
                return 5;
            else
                return 3;
        }
        Color CalculateHpRewardColor(float ratio)
        {
            if (ratio >= 0.5f)
                return Color.green;
            else if (ratio >= 0.25f)
                return Color.yellow;
            else
                return Color.red;
        }

        public int GetOrderId() => orderId;
    }

}
