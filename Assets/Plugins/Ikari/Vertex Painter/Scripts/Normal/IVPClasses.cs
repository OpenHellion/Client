using UnityEngine;
using System.Collections;

public class IVPHotkey
{
    public Event data = new Event();
    public bool enabled;

    public IVPHotkey(KeyCode keyCode, EventModifiers modifiers)
    {
        data.modifiers = modifiers;
        data.keyCode = keyCode;
    }
}
