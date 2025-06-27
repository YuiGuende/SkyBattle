using UnityEngine;

public class start_game : MonoBehaviour
{    public slotmachineManager slotMachineManager; // <-- Đặt trong class

    void OnMouseDown()
    {
        slotMachineManager.ActivateSlotMachine(); // Gọi quay slot
    }
}