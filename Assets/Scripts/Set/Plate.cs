using Photon.Pun;
using Shurub;
using System.Collections.Generic;
using UnityEngine;

public class Plate : Ingredient
{
    public override string IngredientName => "Plate";
    public override IngredientManager.IngredientType kindOfIngredient => IngredientManager.IngredientType.Plate;
    public override IngredientState State => IngredientState.unCookable;

    [SerializeField] private List<Recipe> allRecipes;

    private List<IngredientManager.IngredientType> ingredients = new List<IngredientManager.IngredientType>();

    public IReadOnlyList<IngredientManager.IngredientType> Ingredients => ingredients;

    public void AddIngredient(int kindOfIngredient)
    {
        //이미 있는 재료 인지
        //

        ingredients.Add((IngredientManager.IngredientType)kindOfIngredient);
    }

    public bool TryCook(out GameObject result)
    {
        result = null;

        foreach (var recipe in allRecipes)
        {
            if (IsRecipeMatch(recipe))
            {
                result = recipe.result;
                //result << 프리팹임 이걸 통해 새로운 세트 오브젝트 생성 후 GetPlate 적용
                Clear();
                return true;
            }
        }

        return false;
    }

    private bool IsRecipeMatch(Recipe recipe)
    {
        if (recipe.requiredIngredients.Count != ingredients.Count)
            return false;

        List<IngredientManager.IngredientType> temp = new List<IngredientManager.IngredientType>(ingredients);

        foreach (var required in recipe.requiredIngredients)
        {
            if (!temp.Contains(required))
                return false;

            temp.Remove(required);
        }

        return true;
    }

    public void Clear()
    {
        ingredients.Clear();
    }
}