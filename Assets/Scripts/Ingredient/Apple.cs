using Shurub;
using UnityEngine;

public class Apple : Ingredient
{
    public override string IngredientName => "Apple";
    public override bool IsCuttable => true;
    public override bool IsBakable => false;
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Apple;
}
