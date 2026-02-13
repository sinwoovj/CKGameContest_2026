using Shurub;
using UnityEngine;

public class Steak : Ingredient
{
    public override string IngredientName => "Steak";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.Steak;
}
