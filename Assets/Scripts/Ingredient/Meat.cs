using Shurub;
using UnityEngine;

public class Meat : Ingredient
{
    protected override string IngredientName => "Meat";
    protected override bool IsCuttable => false;
    protected override bool IsBakable => true;
}
