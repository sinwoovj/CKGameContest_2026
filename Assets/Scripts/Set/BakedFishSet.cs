using Shurub;
using UnityEngine;

public class BakedFishSet : Ingredient
{
    public override string IngredientName => "BakedFishSet";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.BakedFishSet;
}
