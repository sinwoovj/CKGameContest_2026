using Shurub;
using UnityEngine;

public class IceWater : Ingredient
{
    public override string IngredientName => "IceWater";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.IceWater;
}
