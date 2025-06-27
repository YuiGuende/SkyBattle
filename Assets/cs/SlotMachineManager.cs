using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlotMachineManager : MonoBehaviour
{
    [Header("Skill Display")]
    public Transform skillDisplayPanel;

    [Header("Prefabs")]
    public GameObject[] allPlanes;          // 5 máy bay random
    public GameObject[] defaultPlanes;      // 2 máy mặc định

    [Header("Slot Display")]
    public Transform[] slotPositions;       // 5 vị trí slot
    public float spinDuration = 3.0f;       // Thời gian quay tổng
    public float spinInterval = 0.1f;       // Khoảng thời gian mỗi lần update

    private List<GameObject> spawnedPlanes = new List<GameObject>();

    public void ActivateSlotMachine()
    {
        Debug.Log("▶ SlotMachine ACTIVATED!");
        StopAllCoroutines();
        StartCoroutine(SpinAndReveal());
    }

    IEnumerator SpinAndReveal()
    {
        Debug.Log("🎰 Spinning...");

        // 🧹 Xoá máy bay cũ nếu có
        foreach (GameObject plane in spawnedPlanes)
        {
            if (plane != null)
                Destroy(plane);
        }
        spawnedPlanes.Clear();

        // 🧹 Xoá icon kỹ năng cũ
        if (skillDisplayPanel != null)
        {
            foreach (Transform child in skillDisplayPanel)
            {
                Destroy(child.gameObject);
            }
        }

        // 🎲 Lấy 3 máy random + 2 default
        List<GameObject> randomPlanes = allPlanes.OrderBy(x => Random.value).Take(3).ToList();
        List<GameObject> finalPlanes = new List<GameObject>(randomPlanes);
        finalPlanes.AddRange(defaultPlanes);
        finalPlanes = finalPlanes.OrderBy(x => Random.value).ToList();

        Debug.Log("✅ Final slot planes count: " + finalPlanes.Count);

        // 🔁 Hiệu ứng quay
        float timer = 0f;
        while (timer < spinDuration)
        {
            for (int i = 0; i < slotPositions.Length; i++)
            {
                if (slotPositions[i].childCount > 0)
                    Destroy(slotPositions[i].GetChild(0).gameObject);

                GameObject temp = Instantiate(allPlanes[Random.Range(0, allPlanes.Length)], slotPositions[i].position, Quaternion.identity, slotPositions[i]);
                temp.transform.localScale = Vector3.one * 0.7f;
                temp.transform.position = new Vector3(temp.transform.position.x, temp.transform.position.y, 0);
            }

            yield return new WaitForSeconds(spinInterval);
            timer += spinInterval;
        }

        // ✅ Hiển thị máy bay kết quả và kỹ năng
        for (int i = 0; i < slotPositions.Length; i++)
        {
            if (slotPositions[i].childCount > 0)
                Destroy(slotPositions[i].GetChild(0).gameObject);

            // Tạo máy bay
            GameObject finalPlane = Instantiate(finalPlanes[i]);
            finalPlane.transform.SetParent(slotPositions[i]);
            finalPlane.transform.localPosition = Vector3.zero;
            finalPlane.transform.localScale = Vector3.one * 0.7f;
            finalPlane.transform.localPosition = new Vector3(0, 0, 0);

            PlaneSkill skill = finalPlane.GetComponent<PlaneSkill>();
            if (skill == null)
            {
                Debug.LogError("❌ Không tìm thấy PlaneSkill trên máy bay: " + finalPlane.name);
            }
            else
            {
                Debug.Log("✅ Đã tìm thấy PlaneSkill trên: " + finalPlane.name + ", skill: " + skill.skillType);

                // Tạo icon kỹ năng
                GameObject icon = new GameObject("SkillIcon_" + i);
                icon.transform.SetParent(skillDisplayPanel);
                icon.transform.localPosition = new Vector3(i * 1.9f, 0, 0);
                icon.transform.position = new Vector3(icon.transform.position.x, icon.transform.position.y, 0);

                icon.AddComponent<BoxCollider2D>().isTrigger = true; ;
                icon.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

                SpriteRenderer sr = icon.AddComponent<SpriteRenderer>();
                sr.sprite = skill.skillIcon;
                sr.sortingOrder = 100;
                sr.transform.localScale = Vector3.one * 0.25f;

                SkillIconHandler handler = icon.AddComponent<SkillIconHandler>();
                handler.skillLinked = skill;

                Debug.Log("🎯 Gán skill thành công cho icon: " + skill.skillType);
            }

            spawnedPlanes.Add(finalPlane);
        }
    }
}
