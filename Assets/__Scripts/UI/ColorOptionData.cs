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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
