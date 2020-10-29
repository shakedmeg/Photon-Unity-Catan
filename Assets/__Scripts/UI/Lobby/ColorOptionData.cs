using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorOptionData : Dropdown.OptionData
{

    public Color Color { get; set; }

    public ColorOptionData(string text, Color color) : base(text)
    {
        Color = color;
    }
}
