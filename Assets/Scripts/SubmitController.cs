﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class SubmitController : MonoBehaviourPunCallbacks
{
    public InteractableScreen buttonHint;
    public GameObject safezone;
    public List<GameObject> smoke;
    
    public int phase;
    public Item repairItem;
    public int repairCurrentCount;
    public int repairMaxCount;
    public Item crystalItem;
    public int crystalCurrentCount;
    public int crystalMaxCount;

    public bool isActive;

    public GameObject soundStart;

    void Start()
    {
        for (int i = 0; i < smoke.Count; i++)
            smoke[i].SetActive(true);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isActive && GameManager.GetInstance().mPlayer.GetComponent<Player>().IsControllable())
            Submit();
    }
    public void Submit()
    {
        Item item;

        if (phase == 1)
            item = repairItem;
        else if (phase == 2)
            item = crystalItem;
        else
            return;

        if (!Inventory.instance.items.Contains(item))
        {
            GameManager.GetInstance().GetComponent<MiniAlertController>().OnEnableAlert("아이템 부족", "필요한 아이템이 없습니다.", new Color(0.5333334f, 0.2666667f, 0.7333333f));
            return;
        }
        
        Inventory.instance.Remove(item);
        photonView.RPC("OnSubmit", RpcTarget.AllBuffered, GameManager.GetInstance().mPlayer.GetComponent<Player>().GetNickname(), phase);
    }
    // ---------------------------------------------------------------------------------------------------
    // # 네트워크 메소드
    // ---------------------------------------------------------------------------------------------------
    [PunRPC]
    public void OnSubmit(string nickname, int phase)
    {
        if (phase == 1)
        {
            repairCurrentCount++;

            if (repairCurrentCount > repairMaxCount)
                repairCurrentCount = repairMaxCount;

            buttonHint.SetDecription(KeyCode.E, "수리 (" + repairCurrentCount + "/" + repairMaxCount + ")");
            GameManager.GetInstance().GetComponent<MissionController>().OnModify("우주선 수리하기", " (" + repairCurrentCount + "/" + repairMaxCount + ")");

            float smokeProgress = (float)repairCurrentCount / (float)repairMaxCount * ((float)smoke.Count - 1.0f);
            for (int i = 0; i <= smokeProgress; i++)
                smoke[i].SetActive(false);

            if (repairCurrentCount < repairMaxCount)
                GameManager.GetInstance().GetComponent<MiniAlertController>().OnEnableAlert("우주선 수리중", nickname + "(이)가 우주선을 수리했습니다.", new Color(0.5333334f, 0.2666667f, 0.7333333f));

            if (PhotonNetwork.IsMasterClient && repairCurrentCount >= repairMaxCount)
                photonView.RPC("OnSubmitFinish", RpcTarget.AllBuffered, phase);
        }
        else if (phase == 2)
        {
            crystalCurrentCount++;

            buttonHint.SetDecription(KeyCode.E, "보석 적재 (" + crystalCurrentCount + "/" + crystalMaxCount + ")");
            GameManager.GetInstance().GetComponent<MissionController>().OnModify("보석 적재중", " (" + crystalCurrentCount + "/" + crystalMaxCount + ")");

            if (crystalCurrentCount < crystalMaxCount)
                GameManager.GetInstance().GetComponent<MiniAlertController>().OnEnableAlert("보석 적재중", nickname + "(이)가 우주선에 보석을 적재했습니다.", new Color(0.5333334f, 0.2666667f, 0.7333333f));

            if (PhotonNetwork.IsMasterClient && crystalCurrentCount >= crystalMaxCount)
                photonView.RPC("OnSubmitFinish", RpcTarget.AllBuffered, phase);
        }
    }
    [PunRPC]
    public void OnSubmitFinish(int phase)
    {
        if (phase == 1)
        {
            this.phase = 2;
            GameManager.GetInstance().GetComponent<MiniAlertController>().OnEnableAlert("우주선 수리 완료", "우주선 수리가 완료되었습니다.\n이제, 보석을 적재할 수 있습니다.", new Color(0.5333334f, 0.2666667f, 0.7333333f));
            GameManager.GetInstance().GetComponent<MissionController>().OnClear("우주선 수리하기");
            buttonHint.SetDecription(KeyCode.E, "보석 적재 (" + crystalCurrentCount + "/" + crystalMaxCount + ")");
        }
        else if (phase == 2)
        {
            this.phase = 3;
            GameManager.GetInstance().GetComponent<MiniAlertController>().OnEnableAlert("보석 적재 완료", "보석을 모두 적재하자, 행성 전체가 흔들리기 시작합니다.\n빨리 우주선을 타고 탈출해야 합니다.", new Color(0.5333334f, 0.2666667f, 0.7333333f));
            GameManager.GetInstance().GetComponent<MissionController>().OnClear("보석 적재하기");
            buttonHint.SetDecription(KeyCode.E, "보석 적재 (" + crystalCurrentCount + "/" + crystalMaxCount + ")");

            // 탈출 시퀀스 시작
            GameManager.GetInstance().time = 60;
            GameManager.GetInstance().clear = true;

            GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            soundStart.GetComponent<EarthSound>().cnt++;
            safezone.SetActive(true);
        }
    }
    // ---------------------------------------------------------------------------------------------------
    // # 트리거 메소드
    // ---------------------------------------------------------------------------------------------------
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || !other.GetComponent<PhotonView>().IsMine)
            return;

        isActive = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !other.GetComponent<PhotonView>().IsMine)
            return;

        isActive = false;
    }
}
