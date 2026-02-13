using Shurub;
using UnityEngine;

public class Salad : Ingredient
{
    public override string IngredientName => "Salad";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.Salad;
}
