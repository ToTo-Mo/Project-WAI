﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using static System.Random;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;

public class GameInterfaceManager : MonoBehaviourPunCallbacks
{
    private static GameInterfaceManager instance = null;

    public AudioSource chatLoadSound;

    int watchIdx = 0; // 관전 모드 인덱스
    float fps = 0.0f; // FPS

    [SerializeField]
    float lerpSpeed = 2f;

    private void Awake()
    {
        instance = this; // 최초 생성인 경우 해당 오브젝트를 계속 인스턴스로 가져감
    }

    public static GameInterfaceManager GetInstance()
    {
        return instance;
    }
    void Start()
    {
        chatLoadSound.Pause();
    }
    void Update()
    {
        if (GameManager.GetInstance() == null)
            return;

        // fps 체크
        fps += (Time.deltaTime - fps) * 0.1f;

        // 플레이어 데이터가 없는 상황, 게임이 끝난 상황에는 UI 미갱신
        if (GameManager.GetInstance().mPlayer == null || IsEnding())
            return;

        // UI 갱신
        refresh();

        // 관전모드 - 대상 전환 (방향키)
        if (IsWatching() && Input.GetKeyDown(KeyCode.LeftArrow))
            OnMoveWatch(-1);
        else if (IsWatching() && Input.GetKeyDown(KeyCode.RightArrow))
            OnMoveWatch(1);

        // 채팅모드 전환 (탭)
        if (Input.GetKeyDown(KeyCode.Tab))
            OnSwitchChat();

        // 채팅모드 - 채팅 (엔터)
        if (IsChating() && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            OnSendChat();

        if (Input.GetKeyDown(KeyCode.F1))
            OnSwitchGuide(true);

        if (Input.GetKeyDown(KeyCode.F2))
            OnSwitchGuide(false);
    }

    void refresh() // UI 갱신
    {
        GameObject.Find("UI_Timer_Bar").gameObject.GetComponent<Image>().fillAmount = GameManager.GetInstance().GetComponent<GameManager>().time / GameManager.GetInstance().GetComponent<GameManager>().timeMax;
        GameObject.Find("UI_Timer_Text").gameObject.GetComponent<Text>().text = Math.Truncate(GameManager.GetInstance().GetComponent<GameManager>().time / 60.0f).ToString() + ":" + Math.Truncate(GameManager.GetInstance().GetComponent<GameManager>().time % 60.0f);

        GameObject.Find("FPS").GetComponent<Text>().text = (int)(1.0f / fps) + " FPS";
        GameObject.Find("Ping").GetComponent<Text>().text = PhotonNetwork.GetPing().ToString() + " ms";

        if (GameManager.GetInstance().mPlayer == null)
            return;

        Player player = GameManager.GetInstance().mPlayer.GetComponent<Player>();

        // GameObject.Find("UI_Stat_HP_Bar").gameObject.GetComponent<Image>().fillAmount = (float)player.GetHP() / (float)player.GetHPMax();
        // GameObject.Find("UI_Stat_O2_Bar").gameObject.GetComponent<Image>().fillAmount = player.IsAlienObject() == true ? 0 : (float)player.GetO2() / (float)player.GetO2Max();
        // GameObject.Find("UI_Stat_Bt_Bar").gameObject.GetComponent<Image>().fillAmount = player.IsAlienObject() == true ? 0 : (float)player.GetBt() / (float)player.GetBtMax();

        // GameObject.Find("UI_Stat_HP_Text").gameObject.GetComponent<Text>().text = Math.Ceiling(player.GetHP()).ToString();
        // GameObject.Find("UI_Stat_O2_Text").gameObject.GetComponent<Text>().text = player.IsAlienObject() == true ? "" : Math.Ceiling(player.GetO2()).ToString();
        // GameObject.Find("UI_Stat_Bt_Text").gameObject.GetComponent<Text>().text = player.IsAlienObject() == true ? "" : Math.Ceiling(player.GetBt()).ToString();
        
        Image UI_Stat_HP_Bar = GameObject.Find("UI_Stat_HP_Bar").gameObject.GetComponent<Image>();
        Image UI_Stat_O2_Bar = GameObject.Find("UI_Stat_O2_Bar").gameObject.GetComponent<Image>();
        Image UI_Stat_Bt_Bar = GameObject.Find("UI_Stat_Bt_Bar").gameObject.GetComponent<Image>();

        UI_Stat_HP_Bar.fillAmount = Mathf.Lerp(UI_Stat_HP_Bar.fillAmount,(float)player.GetHP()/(float)player.GetHPMax(),Time.deltaTime * lerpSpeed);
        UI_Stat_O2_Bar.fillAmount = Mathf.Lerp(UI_Stat_O2_Bar.fillAmount,player.IsAlienObject() == true ? 0 : (float)player.GetO2() / (float)player.GetO2Max(),Time.deltaTime * lerpSpeed);
        UI_Stat_Bt_Bar.fillAmount = Mathf.Lerp(UI_Stat_Bt_Bar.fillAmount,player.IsAlienObject() == true ? 0 : (float)player.GetBt() / (float)player.GetBtMax(),Time.deltaTime * lerpSpeed);

        Text UI_Stat_HP_Text = GameObject.Find("UI_Stat_HP_Text").gameObject.GetComponent<Text>();
        Text UI_Stat_O2_Text = GameObject.Find("UI_Stat_O2_Text").gameObject.GetComponent<Text>();
        Text UI_Stat_Bt_Text = GameObject.Find("UI_Stat_Bt_Text").gameObject.GetComponent<Text>();

        UI_Stat_HP_Text.text = Math.Round(UI_Stat_HP_Bar.fillAmount * (float)player.GetHPMax()).ToString();
        UI_Stat_O2_Text.text = player.IsAlienObject() == true ? "" : Math.Round(UI_Stat_O2_Bar.fillAmount * (float)player.GetO2Max()).ToString();
        UI_Stat_Bt_Text.text = player.IsAlienObject() == true ? "" : Math.Round(UI_Stat_Bt_Bar.fillAmount * (float)player.GetBtMax()).ToString();

        GameObject.Find("UI_Meterial_Wood_Text").GetComponent<Text>().text = player.GetWood().ToString();
        GameObject.Find("UI_Meterial_Iron_Text").gameObject.GetComponent<Text>().text = player.GetIron().ToString();
        GameObject.Find("UI_Meterial_Part_Text").gameObject.GetComponent<Text>().text = player.GetPart().ToString();

        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] nickObj = GameObject.FindGameObjectsWithTag("Nickname");

        for (int i = 0; i < nickObj.Length; i++)
            nickObj[i].GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);

        for (int i = 0; i < playerObj.Length; i++)
        {
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(playerObj[i].transform.position);

            viewportPoint.x *= Screen.width;
            viewportPoint.y = (viewportPoint.y * Screen.height) + 150;

            nickObj[i].GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            nickObj[i].GetComponent<RectTransform>().position = viewportPoint;

            if (playerObj[i].GetComponent<Player>().IsDead())
                nickObj[i].GetComponent<Text>().text = "";
            else
                nickObj[i].GetComponent<Text>().text = playerObj[i].GetComponent<Player>().GetNickname();

            if (GameManager.GetInstance().mPlayer.GetComponent<Player>().IsAlienPlayer() && playerObj[i].GetComponent<Player>().IsAlienPlayer())
                nickObj[i].GetComponent<Outline>().effectColor = new Color(1, 0, 0);
            else
                nickObj[i].GetComponent<Outline>().effectColor = new Color(0, 0, 0);
        }   
    }
    // ---------------------------------------------------------------------------------------------------
    // 채팅 모드
    // ---------------------------------------------------------------------------------------------------
    public bool IsChating() // 채팅 여부
    {
        return !GameObject.Find("UI_Panel_Talk").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Talk_hide");
    }
    public void OnSwitchChat() // 채팅 모드 (스위칭)
    {
        OnSwitchChat(!IsChating());
    }
    public void OnSwitchChat(bool val) // 채팅 모드 (매뉴얼)
    {
        Player player = GameManager.GetInstance().mPlayer.GetComponent<Player>();

        Debug.Log(player.IsDead());

        if (val && !player.IsControllable() && !player.IsDead())
            return;

        GameObject.Find("UI_Talk_Active").gameObject.GetComponent<Image>().enabled = false;
        GameObject.Find("UI_Panel_Talk").gameObject.GetComponent<Animator>().Play(val ? "Talk_load" : "Talk_hide");
        GameObject.Find("UI_Panel_Talk_Input").gameObject.GetComponent<InputField>().DeactivateInputField();

        InputField field = GameObject.Find("UI_Panel_Talk_Input").GetComponent<InputField>();
        field.text = "";
        field.DeactivateInputField();

        GameManager.GetInstance().mPlayer.GetComponent<Player>().SetMove(!val);

        if (val)
        {
            GameManager.GetInstance().GetComponent<MissionController>().OnHide();
            chatLoadSound.Play();
        }
        else
        {
            GameManager.GetInstance().GetComponent<MissionController>().OnShow();
        }
    }
    public void OnSendChat() // 채팅 송신
    {
        if (!GameObject.Find("UI_Panel_Talk").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Talk_show"))
            return;

        
        InputField field = GameObject.Find("UI_Panel_Talk_Input").GetComponent<InputField>();
        if (field.text == "")
        {
            field.ActivateInputField();
            return;
        }

        Player player = GameManager.GetInstance().mPlayer.GetComponent<Player>();
        if (player.IsDead())
        {
            field.text = "";
            field.ActivateInputField();
            return;
        }

        photonView.RPC("OnReceiveChat", RpcTarget.AllBuffered, GameManager.GetInstance().mPlayer.GetComponent<Player>().GetNickname() + " : " + field.text);
        field.text = "";
        field.ActivateInputField();
    }
    [PunRPC]
    public void OnReceiveChat(string message) // 채팅 수신
    {
        GameObject.Find("UI_Talk_Active").GetComponent<Image>().enabled = true;

        Text chat = GameObject.Find("UI_Panel_Talk_Panel_Text").GetComponent<Text>();

        if (chat.text.Length > 10000)
            chat.text = chat.text.Substring(chat.text.Length - 10000, 10000);

        chat.text = chat.text + "\n" + message;
    }
    // ---------------------------------------------------------------------------------------------------
    // 관전 모드
    // ---------------------------------------------------------------------------------------------------
    public bool IsWatching() // 관전 여부
    {
        return GameObject.Find("UI_Watching").GetComponent<RectTransform>().localScale == new Vector3(1, 1, 1);
    }
    public void OnSwitchWatch() // 관전 모드 (스위칭)
    {
        OnSwitchWatch(!IsWatching());
    }
    public void OnSwitchWatch(bool val) // 관전 모드 (매뉴얼)
    {
        GameObject.Find("UI_Stats").GetComponent<RectTransform>().localScale = (val ? new Vector3(0, 0, 0) : new Vector3(1, 1, 1));
        GameObject.Find("UI_Inventory").GetComponent<RectTransform>().localScale = (val ? new Vector3(0, 0, 0) : new Vector3(1, 1, 1));
        GameObject.Find("UI_Watching").GetComponent<RectTransform>().localScale = (val ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0));

        GameManager.GetInstance().mPlayer.GetComponent<Player>().SetMove(!val);
        if (val) GameManager.GetInstance().GetComponent<MissionController>().OnHide();
        else GameManager.GetInstance().GetComponent<MissionController>().OnShow();

        OnSwitchChat(false);

        if (val) OnMoveWatch(1);
    }
    public void OnMoveWatch(int num) // 관전 대상 변경
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        int cnt = 1;
        
        while (cnt >= 1 && cnt <= 20)
        {
            watchIdx += num;
            if (watchIdx >= player.Length) watchIdx = 0;
            if (watchIdx < 0) watchIdx = player.Length - 1;

            if (player[watchIdx].GetComponent<Player>().IsDead() == false) cnt = -1;
            else cnt++;
        }

        if (cnt > 20)
        {
            GameObject.Find("UI_Watching").GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            GameObject.Find("UI_Watching_Nickname").GetComponent<Text>().text = "";
        }
        else if (cnt < 0)
        {
            GameObject.Find("UI_Watching").GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            GameManager.GetInstance().mCamera.GetComponent<CinemachineFreeLook>().Follow = player[watchIdx].transform;
            GameManager.GetInstance().mCamera.GetComponent<CinemachineFreeLook>().LookAt = player[watchIdx].transform;
            GameObject.Find("UI_Watching_Nickname").GetComponent<Text>().text = player[watchIdx].GetComponent<PhotonView>().Owner.NickName;
        }
    }
    // ---------------------------------------------------------------------------------------------------
    // 숨김 모드
    // ---------------------------------------------------------------------------------------------------
    public bool IsHiding() // 숨김 여부
    {
        return GameObject.Find("UI_Game").GetComponent<Canvas>().enabled == false;
    }
    public void OnSwitchHide() // 숨김 모드 (스위칭)
    {
        OnSwitchHide(!IsHiding());
    }
    public void OnSwitchHide(bool val) // 숨김 모드 (매뉴얼)
    {
        GameObject.Find("UI_Game").GetComponent<Canvas>().enabled = !val;
    }
    // ---------------------------------------------------------------------------------------------------
    // 가이드 모드
    // ---------------------------------------------------------------------------------------------------
    public void OnSwitchGuide(bool key) // 가이드 모드
    {
        RectTransform guide_1 = GameObject.Find("UI_Guide_1").GetComponent<RectTransform>();
        RectTransform guide_2 = GameObject.Find("UI_Guide_2").GetComponent<RectTransform>();
        RectTransform guide_key = GameObject.Find("UI_Guide_Key").GetComponent<RectTransform>();

        if (key)
        {
            if (guide_key.localScale == Vector3.zero)
            {
                guide_1.localScale = Vector3.zero;
                guide_2.localScale = Vector3.zero;
                guide_key.localScale = Vector3.one;
            }
            else
            {
                guide_1.localScale = Vector3.zero;
                guide_2.localScale = Vector3.zero;
                guide_key.localScale = Vector3.zero;
            }
        }
        else
        {
            if (guide_2.localScale == Vector3.one)
            {
                guide_1.localScale = Vector3.zero;
                guide_2.localScale = Vector3.zero;
                guide_key.localScale = Vector3.zero;
            }
            else if (guide_1.localScale == Vector3.one)
            {
                guide_1.localScale = Vector3.zero;
                guide_2.localScale = Vector3.one;
                guide_key.localScale = Vector3.zero;
            }
            else
            {
                guide_1.localScale = Vector3.one;
                guide_2.localScale = Vector3.zero;
                guide_key.localScale = Vector3.zero;
            }
        }
    }
    // ---------------------------------------------------------------------------------------------------
    // 게임 종료
    // ---------------------------------------------------------------------------------------------------
    public bool IsEnding() // 종료 여부
    {
        return GameObject.Find("UI_Result").GetComponent<Canvas>().enabled;
    }
    public void OnSwitchEnd() // 결과 출력 (스위칭)
    {
        OnSwitchEnd(!IsEnding());
    }
    public void OnSwitchEnd(bool val) // 결과 출력 (스위칭)
    {
        GameObject.Find("UI_Result").GetComponent<Canvas>().enabled = val;
        GameObject.Find("UI_Game").GetComponent<Canvas>().enabled = !val;
    }
    // ---------------------------------------------------------------------------------------------------
    // 콜백 메소드
    // ---------------------------------------------------------------------------------------------------
    public override void OnLeftRoom() // 방 퇴장
    {
        base.OnLeftRoom();

        SceneManager.LoadScene("proto_main");
    }
}