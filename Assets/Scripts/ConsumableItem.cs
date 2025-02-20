﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Stat{
    HP,O2,BATTERY
}

[CreateAssetMenu(fileName = "new ConsumableItem", menuName = "Item/ConsumableItem", order = 0)]
public class ConsumableItem : Item
{

    [SerializeField]
    int hpModifier;
    [SerializeField]
    int o2Modifier;
    [SerializeField]
    int batteryModifier;

    [SerializeField]
    float castingTime = 1f;
    Progress progress = Progress.END; 

    public override void Use(Player playerStat)
    {
        ResearcherAnimation researcherAnimation = playerStat.researcher.GetComponent<ResearcherAnimation>();
        IEnumerator coroutine = OnStartAfterTime(playerStat,castingTime);

        Debug.Log(name + "을 사용하였습니다.");

        if(Inventory.instance != null && !researcherAnimation.IsWalk){
            researcherAnimation.OnCasting();
            Inventory.instance.StartCoroutine(coroutine);
            Inventory.instance.StartCoroutine(IsCasting(coroutine,researcherAnimation));
        }
    }

    IEnumerator IsCasting(IEnumerator coroutine,ResearcherAnimation researcherAnimation){

        // float calibrationTime = 0.5f;

        // yield return new WaitForSeconds(calibrationTime);

        float startTime = Time.time;

        while(Time.time <= startTime + castingTime){
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            bool hasHorizontalInput = !Mathf.Approximately(horizontal,0f);
            bool hasVeritcalInput = !Mathf.Approximately(vertical,0f);

            bool isWalk = hasHorizontalInput || hasVeritcalInput;

            if(isWalk){
                Inventory.instance.StopCoroutine(coroutine);
                researcherAnimation.EndAnimation();
                break;
            }
            yield return null;  //1프레임 마다 체크합니다.
        }
        researcherAnimation.EndAnimation();
    }

    public override void Continue(Player playerStat)
    {

    }

    IEnumerator OnStartAfterTime(Player playerStat, float delayTime){
        
        float startTime = Time.time;

        while(Time.time <= startTime + castingTime){
            yield return null;  //1프레임 마다 체크합니다.
        }

        // yield return new WaitForSeconds(delayTime);

        Debug.Log("효과가 발동합니다.");

        TakeEffect(playerStat, hpModifier,Stat.HP);
        TakeEffect(playerStat, o2Modifier,Stat.O2);
        TakeEffect(playerStat, batteryModifier,Stat.BATTERY);

        Inventory.instance.Remove(this);
    }

    void TakeEffect(Player playerStat, int modifier, Stat stat){
        Debug.Log("수치가 변동됩니다.");

        // 연구원
        if(!playerStat.IsAlienPlayer()){
            if(Stat.HP == stat)
                playerStat.SetHP(playerStat.GetHP() + modifier);
            if(Stat.O2 == stat)
                playerStat.SetO2(playerStat.GetO2() + modifier);
            if(Stat.BATTERY == stat)
                playerStat.SetBt(playerStat.GetBt() + modifier);

        // 외계인
        }else{
            if(Stat.HP == stat)
                playerStat.SetHP(playerStat.GetHP() + modifier/2);
            if(Stat.O2 == stat)
                playerStat.SetO2(playerStat.GetO2() + modifier/2);
            if(Stat.BATTERY == stat)
                playerStat.SetBt(playerStat.GetBt() + modifier);
        }


    }

    public int HPModifier
    {
        get => hpModifier;
        set => hpModifier = value;
    }

    public int O2Modifier
    {
        get => o2Modifier;
        set => o2Modifier = value;
    }

    public int BatteryModifier{
        get => batteryModifier;
        set => batteryModifier = value;
    }

    public float CastingTime{
        get => castingTime;
    }
}