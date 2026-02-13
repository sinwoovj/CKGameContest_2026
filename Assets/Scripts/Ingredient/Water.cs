using Shurub;
using UnityEngine;

public class Water : Ingredient
{
    public override string IngredientName => "Water";
    public override bool IsCuttable => false;
    public override bool IsBakable => false;
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Water;
    private void Start()
    {
        state = IngredientState.cooked;
    }
}
