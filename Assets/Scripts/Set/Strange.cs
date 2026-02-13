using Shurub;
using UnityEngine;

public class Strange : Ingredient
{
    public override string IngredientName => "Strange";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.Strange;
}
