using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PublicInfo
{
    public int roleID;
    public string name;
    public int status;
}

public class WaitRoom : MonoBehaviour {

    public LayoutGroup waitPlayerLayout;
    public Button readyButton;
    public Button startButton;
    public GameObject gameScene;
    public GameObject gameUI;
    public GameObject gameHis;

    public List<PublicInfo> waitCacheList;
    private static GameObject waitPlayerPrefab;
    private Dictionary<int, WaitPlayer> waitPlayers = new Dictionary<int, WaitPlayer>();

    private int roomHolder;
    bool gameStart = false;
    private int actionRoleID;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        UpdatePlayerList();
        TryStartGame();
    }

    void TryStartGame()
    {
        if (gameStart)
        {
            Debug.Log("game start");
            this.gameObject.SetActive(false);
            gameScene.SetActive(true);
            gameScene.GetComponent<GameController>().waitPlayers = waitPlayers;
            gameScene.GetComponent<GameController>().actionRoleID = this.actionRoleID;
            gameUI.SetActive(true);
        }
    }

    private void UpdatePlayerList()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (waitCacheList.Count <= 0)
        {
            return;
        }
        foreach (var item in waitCacheList)
        {
            int thisRoleID = item.roleID;
            if (waitPlayers.ContainsKey(thisRoleID))
            {
                waitPlayers[thisRoleID].status.text = item.status.ToString();
            }
            else if (thisRoleID == PlayerData.Instance.RoleID)
            {
                PlayerData.Instance.Init(item);
            }
            Debug.Log("show new player. roleID:" + thisRoleID);
            GameObject go = Instantiate(waitPlayerPrefab);
            WaitPlayer wp = go.GetComponent<WaitPlayer>();
            wp.nameText.text = item.name;
            wp.roleIDText.text = thisRoleID.ToString();
            wp.status.text = item.status.ToString();

            go.transform.SetParent(waitPlayerLayout.transform);
            go.name = "动态" + waitPlayers.Count;
            waitPlayers[thisRoleID] = wp;
        }
        waitCacheList.Clear();
    }

    private void Awake()
    {
        Debug.Log("room awake");
        waitCacheList = new List<PublicInfo>();
        waitPlayerPrefab = (GameObject)Resources.Load("Prefabs/WaitPlayer");
        SingleNet.Instance.PlayerJoinEvent += new PlayerJoinEventHandler(AddNewWaitPlayer);
        SingleNet.Instance.PlayerStatusModify += new PlayerStatusModifyEventHandler(OnPlayerStatusModify);
    }

    public void WaitRoomOnLogin(Protobuf.playersInfo pinfos)
    {
        Debug.Log("login success, player count:" + pinfos);
        foreach(var p in pinfos.BaseInfos)
        {
            PublicInfo pi;
            pi.roleID = p.RoleID;
            pi.name = p.Name;
            pi.status = p.Status;
            this.waitCacheList.Add(pi);
        }
        //同时返回的还有房主ID
        this.roomHolder = pinfos.RoomHolder;
        ModifyStatusText();
    }

    void AddNewWaitPlayer(Protobuf.playersInfo pinfos)
    {
        foreach (var pinfo in pinfos.BaseInfos)
        {
            PublicInfo pi = new PublicInfo
            {
                roleID = pinfo.RoleID,
                name = pinfo.Name,
                status = pinfo.Status,
            };
            this.waitCacheList.Add(pi);
        }
    }

    void ModifyStatusText()
    {
        if (PlayerData.Instance.RoleID == this.roomHolder)
        {
            startButton.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(false);
        }
        else
        {
            startButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
        }
    }

    public void ClickReady()
    {
        StatusRequest(PlayerStatus.psReady);
    }

    public void ClickStart()
    {
        StatusRequest(PlayerStatus.psStart);
    }

    void StatusRequest(PlayerStatus ps)
    {
        Protobuf.statusRequest request = new Protobuf.statusRequest
        {
            Cmd = (int)ps
        };
        SingleNet.Instance.SendMsgCommon(request, "modifyStatus");
    }

    public void OnPlayerStatusModify(Protobuf.statusBroadcast sb)
    {
        if (sb.Cmd == (int)PlayerStatus.psStart)
        {
            //j将所有人都设为ingame
            gameStart = true;
            actionRoleID = sb.ActionRoleID;
            return;
        }

        PublicInfo pi = new PublicInfo
        {
            roleID = sb.Roleid,
            status = sb.Cmd,
        };
    }

}
