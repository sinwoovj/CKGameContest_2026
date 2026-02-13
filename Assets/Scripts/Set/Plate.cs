using Photon.Pun;
using Shurub;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Plate : Ingredient
{
    public override string IngredientName => "Plate";
    public override IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Plate;
    private void Start()
    {
        state = IngredientState.unCookable;
    }

    [SerializeField] private List<Recipe> allRecipes;
    [SerializeField]
    private List<IngredientManager.IngredientType> ingredients = new List<IngredientManager.IngredientType>();

    public List<IngredientManager.IngredientType> Ingredients => ingredients;

    [SerializeField]
    private List<int> ingredientList = new List<int>();

    public bool TryAddIngredient(int ingredientViewId, int ingredientType, int playerViewId)
    {
        if (ingredientType < 0 || ingredientType >= (int)IngredientManager.IngredientType.Count)
        {
            Debug.Log("유효하지 않은 인자 값입니다. in Try AddIngredient()");
            return false;
        }

        //이미 있는 재료 인지 아닌지
        if (ingredients.Contains((IngredientManager.IngredientType)ingredientType))
        {
            Debug.Log("이미 포함된 재료 입니다.");
            return false;
        }

        PhotonView ingredientPV = PhotonView.Find(ingredientViewId);
        if (ingredientPV == null) return false;

        ingredientList.Add(ingredientViewId);
        ingredients.Add((IngredientManager.IngredientType)ingredientType);

        photonView.RPC(
            nameof(RPC_UpdatePlateUI),
            RpcTarget.All,
            photonView.ViewID,
            transform.position,
            TryCook(),
            true
        );

        return true;
    }

    [PunRPC]
    void RPC_UpdatePlateUI(int plateViewId, Vector3 pos, int setType, bool active)
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) return;

        PlayerUIManager.Instance.UpdatePlateUI(
            plateViewId,
            pos,
            setType,
            active
        );
    }

    [PunRPC]
    void RPC_RemovePlateUI(int plateViewId)
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) return;

        PlayerUIManager.Instance.RemovePlateUI(
            plateViewId
        );
    }
    public bool TrySubtractIngredient(out int ingredientViewId, int playerViewId)
    {
        ingredientViewId = 0;
        if (ingredients.Count <= 0)
        {
            Debug.Log("접시가 비어 있습니다.");
            return false;
        }

        ingredientViewId = ingredientList[^1];
        ingredientList.RemoveAt(ingredientList.Count - 1);
        ingredients.RemoveAt(ingredients.Count - 1);

        if (ingredients.Count > 0)
        {
            photonView.RPC(
                nameof(RPC_UpdatePlateUI),
                RpcTarget.All,
                photonView.ViewID,
                transform.position,
                TryCook(),
                true
            );
        }
        else
        {
            photonView.RPC(
                nameof(RPC_UpdatePlateUI),
                RpcTarget.All,
                photonView.ViewID,
                transform.position,
                TryCook(),
                false
            );
        }
        return true;
    }
    public bool HasIngredient() => ingredients.Count > 0;
    public bool CanCook() => HasIngredient();

    public int TryCook()
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe.setType == IngredientManager.SetType.Strange) continue;
            if (IsRecipeMatch(recipe))
            {
                return (int)recipe.setType;
            }
        }

        return (int)IngredientManager.SetType.Strange;
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

    public void ClearIngredients(int playerViewId)
    {
        Clear();
        photonView.RPC(
            nameof(RPC_RemovePlateUI),
            RpcTarget.All,
            photonView.ViewID
        );
        foreach (int ingredientViewId in ingredientList)
        {
            PhotonView ingredientPV = PhotonView.Find(ingredientViewId);
            if (ingredientPV == null) continue;
            IngredientManager.Instance.DestroyIngredient(ingredientPV.GetComponent<Ingredient>());            
        }
    }
}