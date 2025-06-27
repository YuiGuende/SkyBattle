using UnityEngine;

public class start_game : MonoBehaviour
{
    public GridManager gridManager;
    public SlotMachineManager slotMachineManager; // <-- Đặt trong class

    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.StartGame();

            if (slotMachineManager != null)
            {
                slotMachineManager.ActivateSlotMachine(); // Gọi quay slot
            }

            gameObject.SetActive(false);
        }
    }
}
