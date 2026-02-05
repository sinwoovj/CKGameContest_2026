using UnityEngine;

namespace Shurub
{
    public class Lettuce : Ingredient
    {
        protected override string IngredientName => "Lettuce";
        protected override bool IsCuttable => true;
        protected override bool IsBakable => false;
    }
}
