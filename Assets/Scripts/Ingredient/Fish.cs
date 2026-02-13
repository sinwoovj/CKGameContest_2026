using Shurub;
using UnityEngine;

public class Fish : Ingredient
{
    public override string IngredientName => "Fish";
    public override bool IsCuttable => false;
    public override bool IsBakable => true;
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Fish;
}
