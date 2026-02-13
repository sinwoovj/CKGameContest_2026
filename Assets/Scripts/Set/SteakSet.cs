using Shurub;
using UnityEngine;

public class SteakSet : Ingredient
{
    public override string IngredientName => "SteakSet";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.SteakSet;
}
