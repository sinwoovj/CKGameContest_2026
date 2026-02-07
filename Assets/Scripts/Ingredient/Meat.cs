using Shurub;
using UnityEngine;

public class Meat : Ingredient
{
    public override string IngredientName => "Meat";
    public override bool IsCuttable => false;
    public override bool IsBakable => true;
}
