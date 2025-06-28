using UnityEngine;
using Mirror;

public class ManaNotificationUI : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnManaChanged))]
    public int mana;

    [SyncVar]
    public int maxMana = 10;

    public void SetMana(int amount)
    {
        mana = amount;
    }

    void OnManaChanged(int oldMana, int newMana)
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.SetActive(i < mana);
        }
    }
}
