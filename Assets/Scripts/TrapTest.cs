﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TrapTest : MonoBehaviourPun
{
    // Start is called before the first frame update
    public GameObject center;
    void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<PhotonView>() == null || other.GetComponent<PhotonView>().IsMine == false)
            return;
        
        if(other.gameObject.tag != "HitBox")
            return ;
        
       other.transform.parent.position = center.transform.position;
    }
}
