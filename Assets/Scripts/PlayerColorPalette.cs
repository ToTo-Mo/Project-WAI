﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerColorPalette", menuName = "PlayerColorPalette")]

public class PlayerColorPalette : ScriptableObject
{
    // Start is called before the first frame update

    public List<Color> colors;
}
