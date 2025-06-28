    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class slotmachineManager : NetworkBehaviour
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

    public GridManager assignedGridManager;
    public NetworkIdentity assignedGridIdentity;

    private List<GameObject> spawnedPlanes = new List<GameObject>();

    void Start()
    {
        if (!isOwned)
            gameObject.SetActive(false);
    }

    [Server]
    public void ActivateSlotMachine()
    {
        Debug.Log("▶ [Server] SlotMachine ACTIVATED!");
        StartCoroutine(SpinAndRevealServer());
    }

    [TargetRpc]
    public void TargetActivateSlotMachine(NetworkConnection target)
    {
        Debug.Log("🎯 [Client] Activating Slot Machine...");
        // (Tuỳ bạn: có thể thêm hiệu ứng khởi động hoặc âm thanh)
        StartCoroutine(SpinAndRevealServer());
    }


    IEnumerator SpinAndRevealServer()
    {
        Debug.Log("🎰 [Server] Spinning...");

        // Chọn máy bay
        List<GameObject> randomPlanes = allPlanes.OrderBy(x => Random.value).Take(3).ToList();
        List<GameObject> finalPlanes = new List<GameObject>(randomPlanes);
        finalPlanes.AddRange(defaultPlanes);
        finalPlanes = finalPlanes.OrderBy(x => Random.value).ToList();



        // Spawn từng máy bay và gọi Target để hiển thị
        for (int i = 0; i < slotPositions.Length; i++)
        {
            GameObject prefab = finalPlanes[i];
            Vector3 spawnPos = slotPositions[i].position;

            GameObject plane = Instantiate(prefab, spawnPos, Quaternion.identity);

            var mb = plane.GetComponent<maybay>();
            if (mb != null)
            {
                mb.gridManager = assignedGridManager;
                mb.gridIdentity = assignedGridIdentity;
            }

            NetworkServer.Spawn(plane, connectionToClient);

            // Gọi client để hiển thị lên slot
            TargetPlacePlane(connectionToClient, plane.GetComponent<NetworkIdentity>(), i);
        }

        yield return null;
    }

    [TargetRpc]
    void TargetPlacePlane(NetworkConnection target, NetworkIdentity planeNetId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotPositions.Length)
        {
            Debug.LogWarning("⚠ Invalid slot index");
            return;
        }

        var plane = planeNetId.gameObject;

        // Parent + Scale + Position
        plane.transform.SetParent(slotPositions[slotIndex]);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = Vector3.one;

        spawnedPlanes.Add(plane);

        PlaneSkill skill = plane.GetComponent<PlaneSkill>();
        if (skill == null)
        {
            Debug.LogError("❌ Không tìm thấy PlaneSkill trên máy bay: " + plane.name);
            return;
        }

        // Tạo icon kỹ năng
        GameObject icon = new GameObject("SkillIcon_" + slotIndex);
        icon.transform.SetParent(skillDisplayPanel);
        icon.transform.localPosition = new Vector3(slotIndex * 1.9f, 0, 0);
        icon.transform.position = new Vector3(icon.transform.position.x, icon.transform.position.y, 0);

        icon.AddComponent<BoxCollider2D>().isTrigger = true;
        icon.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        SpriteRenderer sr = icon.AddComponent<SpriteRenderer>();
        sr.sprite = skill.skillIcon;
        sr.sortingOrder = 100;
        sr.transform.localScale = Vector3.one * 0.25f;

        iconhandle handler = icon.AddComponent<iconhandle>();
        handler.skillLinked = skill;

        Debug.Log("🎯 Gán skill thành công cho icon: " + skill.skillType);
    }
}
