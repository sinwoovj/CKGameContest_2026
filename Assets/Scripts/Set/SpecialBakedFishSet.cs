using Shurub;
using UnityEngine;

public class SpecialBakedFishSet : Ingredient
{
    public override string IngredientName => "SpecialBakedFishSet";
    public override IngredientState State => IngredientState.unCookable;
    public override IngredientManager.SetType kindOfSet => IngredientManager.SetType.SpecialBakedFishSet;
}
