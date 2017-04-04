using UnityEngine;

public class FFToolTip : PropertyAttribute
{
    public readonly string tooltip;
    public FFToolTip(string tooltip)
    {
        this.tooltip = tooltip;
    }
}

