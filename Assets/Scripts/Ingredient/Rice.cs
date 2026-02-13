using Shurub;
using UnityEngine;

public class Rice : Ingredient
{
    public override string IngredientName => "Rice";
    public override bool IsCuttable => false;
    public override bool IsBakable => true;
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Rice;
}
