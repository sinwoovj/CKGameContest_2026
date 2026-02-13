using Shurub;
using UnityEngine;

public class SpecialSteakSet : Ingredient
{
    public override string IngredientName => "SpecialSteakSet";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.SpecialSteakSet;
}
