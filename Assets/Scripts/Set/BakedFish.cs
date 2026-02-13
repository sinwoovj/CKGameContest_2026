using Shurub;
using UnityEngine;

public class BakedFish : Ingredient
{
    public override string IngredientName => "BakedFish";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.BakedFish;
}
