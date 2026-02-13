using Shurub;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIManager : Singleton<PlayerUIManager>
{
    private Dictionary<int, ResultSetUI> plateUIMap = new();

    [SerializeField]
    private GameObject ResultSetUIPrefab;
    private readonly string uiRoot = "UIRootWorld";

    public void UpdatePlateUI(int plateViewId, Vector3 worldPos, int setType, bool active)
    {
        if (!plateUIMap.TryGetValue(plateViewId, out var ui))
        {
            ui = Instantiate(ResultSetUIPrefab, GameObject.Find(uiRoot).transform).GetComponent<ResultSetUI>();
            plateUIMap.Add(plateViewId, ui);
        }

        ui.SetPos(worldPos + new Vector3(0.7f, 0.7f));
        ui.SetActive(active);
        ui.SetSprite(setType);
    }

    public void RemovePlateUI(int plateViewId)
    {
        if (plateUIMap.TryGetValue(plateViewId, out var ui))
        {
            Destroy(ui.gameObject);
            plateUIMap.Remove(plateViewId);
        }
    }

    public void ClearPlateUI()
    {
        foreach(var ui in plateUIMap)
        {
            RemovePlateUI(ui.Key);
        }
    }
}
