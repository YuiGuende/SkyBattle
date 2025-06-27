using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    public Sprite[] planeSprites;      // 5 sprite máy bay
    public Image[] slotImages;         // 3 Image slot
    public float spinDuration = 2f;    // Thời gian quay

    private Sprite[] results = new Sprite[3];

    void Start()
    {
        StartCoroutine(SpinAllSlots());
    }

    IEnumerator SpinAllSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            StartCoroutine(SpinSingleSlot(i));
        }

        yield return new WaitForSeconds(spinDuration + 0.5f);

        // Sau khi quay xong → bạn có thể Instantiate ra trận hình ở đây
    }

    IEnumerator SpinSingleSlot(int index)
    {
        float t = 0f;
        Image img = slotImages[index];

        while (t < spinDuration)
        {
            int randomIndex = Random.Range(0, planeSprites.Length);
            img.sprite = planeSprites[randomIndex];
            t += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        // Chọn kết quả cuối cùng
        int resultIndex = Random.Range(0, planeSprites.Length);
        img.sprite = planeSprites[resultIndex];
        results[index] = planeSprites[resultIndex];
    }
}
