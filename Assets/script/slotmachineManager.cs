using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class slotmachineManager : NetworkBehaviour
{
    [Header("Skill Display")]
    public Button[] skillButtons;         // Nút kỹ năng
    public Image[] skillIcons;            // Icon trong mỗi nút
    private PlaneSkill[] assignedSkills = new PlaneSkill[3];

    [Header("Prefabs")]
    public GameObject[] allPlanes;        // 5 máy bay random
    public GameObject[] defaultPlanes;    // 2 máy mặc định

    [Header("Slot Display")]
    public Transform[] slotPositions;     // 5 vị trí slot
    public float spinDuration = 3.0f;
    public float spinInterval = 0.1f;

    [SyncVar]
    public NetworkIdentity assignedGridIdentity;
    public GridManager assignedGridManager;

    private List<GameObject> spawnedPlanes = new List<GameObject>();

    public GridManager AssignedGridManager => assignedGridIdentity != null
        ? assignedGridIdentity.GetComponent<GridManager>()
        : null;

    void Start()
    {
        if (!isOwned)
            gameObject.SetActive(false);

        // Gán sự kiện click cho 3 nút
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;
            skillButtons[i].onClick.AddListener(() => OnSkillButtonPressed(index));
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (assignedGridIdentity != null)
        {
            assignedGridManager = assignedGridIdentity.GetComponent<GridManager>();
        }
    }

    [Server]
    public void ActivateSlotMachine()
    {
        Debug.Log("▶ [Server] SlotMachine ACTIVATED!");
        StartCoroutine(SpinAndRevealServer());
    }

    IEnumerator SpinAndRevealServer()
    {
        List<GameObject> randomPlanes = allPlanes.OrderBy(x => Random.value).Take(3).ToList();
        List<GameObject> finalPlanes = new List<GameObject>(randomPlanes);
        finalPlanes.AddRange(defaultPlanes);
        finalPlanes = finalPlanes.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < slotPositions.Length; i++)
        {
            GameObject prefab = finalPlanes[i];
            GameObject plane = Instantiate(prefab, slotPositions[i].position, Quaternion.identity);

            var mb = plane.GetComponent<maybay>();
            if (mb != null)
            {
                mb.gridManager = AssignedGridManager;
                mb.gridIdentity = assignedGridIdentity;
            }

            NetworkServer.Spawn(plane, connectionToClient);

            var netId = plane.GetComponent<NetworkIdentity>();
            if (netId.connectionToClient == null)
                netId.AssignClientAuthority(connectionToClient);

            //TargetPlacePlane(connectionToClient, netId, i);
            if (connectionToClient != null)
            {
                TargetPlacePlane(connectionToClient, netId, i);
            }
            else
            {
                Debug.LogWarning("⚠️ connectionToClient is null (likely host). Using ClientRpc fallback.");
                RpcPlacePlane(netId.netId, i);
            }

        }

        yield return null;
    }

    [ClientRpc]
    void RpcPlacePlane(uint planeNetId, int slotIndex)
    {
        if (!isLocalPlayer) return;

        var plane = NetworkClient.spawned[planeNetId].gameObject;

        plane.transform.SetParent(slotPositions[slotIndex]);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = Vector3.one;
        spawnedPlanes.Add(plane);

        PlaneSkill skill = plane.GetComponent<PlaneSkill>();
        if (skill == null) return;

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (assignedSkills[i] == null)
            {
                skillIcons[i].sprite = skill.skillIcon;
                assignedSkills[i] = skill;

                var tmpText = skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = skill.skillType.ToString();

                skillIcons[i].gameObject.SetActive(true);
                skillButtons[i].gameObject.SetActive(true);
                break;
            }
        }
    }


    [TargetRpc]
    void TargetPlacePlane(NetworkConnection target, NetworkIdentity planeNetId, int slotIndex)
    {
        var plane = planeNetId.gameObject;
        plane.transform.SetParent(slotPositions[slotIndex]);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = Vector3.one;
        spawnedPlanes.Add(plane);

        PlaneSkill skill = plane.GetComponent<PlaneSkill>();
        if (skill == null) return;

        // Gán skill vào các nút cố định
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (assignedSkills[i] == null)
            {
                skillIcons[i].sprite = skill.skillIcon;
                assignedSkills[i] = skill;

                // Gán tên kỹ năng (nếu muốn)
                var tmpText = skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = skill.skillType.ToString();

                skillIcons[i].gameObject.SetActive(true);
                skillButtons[i].gameObject.SetActive(true);
                break;
            }
        }
    }

    public void OnSkillButtonPressed(int index)
    {
        if (index >= 0 && index < assignedSkills.Length && assignedSkills[index] != null)
        {
            assignedSkills[index].ActivateSkill();
        }
    }
}