using Shurub;
using UnityEngine;

public class Ice : Ingredient
{
    public override string IngredientName => "Ice";
    public override bool IsCuttable => false;
    public override bool IsBakable => false;
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Ice;
    private void Start()
    {
        state = IngredientState.cooked;
    }
}
