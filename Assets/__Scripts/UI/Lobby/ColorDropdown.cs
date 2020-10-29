using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorDropdown : Dropdown
{

    private int optionsIndex = 0;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        optionsIndex = 0;
        return base.CreateDropdownList(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        var item = base.CreateItem(itemTemplate);
        var colorImage = item.transform.GetChild(3);
        var colorImageComp = colorImage.GetComponent<Image>();

        var data = this.options[optionsIndex];
        if (data is ColorOptionData colorOptionData)
        {
            colorImageComp.color = colorOptionData.Color;
        }
        optionsIndex++;
        return item;
    }
}
