using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModalManager : Singleton<ModalManager>
{
    [SerializeField] private Modal modalPrefab;

    private List<Modal> modals = new List<Modal>();

    public void OpenNewModal(string title, string body, bool disableNo = false, UnityAction yesAction = null, UnityAction noAction = null)
    {
        Modal modal = null;
        for (int i = 0; i < modals.Count; i++)
        {
            if (!modals[i].gameObject.activeSelf)
            {
                modal = modals[i];
                break;
            }
        }

        if (modal == null)
        {
            modal = Instantiate(modalPrefab);
            modal.transform.SetParent(transform);
            modals.Add(modal);
        }

        modal.Open(title, body, disableNo, yesAction, noAction);
    }

    public void Close(Modal modal)
    {
        modal.Close();
    }

    public void CloseAll()
    {
        for (int i = 0; i < modals.Count; i++)
        {
            Close(modals[i]);
        }
    }

    public int GetActiveModalCount()
    {
        int count = 0;
        for (int i = 0; i < modals.Count; i++)
        {
            if (modals[i].gameObject.activeSelf)
            {
                count++;
            }
        }

        return count;
    }
}
