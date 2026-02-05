using Shurub;
using UnityEngine;

public class Rice : Ingredient
{
    protected override string IngredientName => "Rice";
    protected override bool IsCuttable => false;
    protected override bool IsBakable => true;
}
