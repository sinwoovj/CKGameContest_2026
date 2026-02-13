using Shurub;
using UnityEngine;

public class AppleJuice : Ingredient
{
    public override string IngredientName => "AppleJuice";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.AppleJuice;
}
