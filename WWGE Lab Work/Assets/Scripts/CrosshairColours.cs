using System.Collections.Generic;
using UnityEngine;

public struct CrosshairColours
{
    public static KeyValuePair<string, Color>[] Colours = new KeyValuePair<string, Color>[]
    {
        new KeyValuePair<string, Color>(Key: "White", Value: Color.white),
        new KeyValuePair<string, Color>(Key: "Black", Value: Color.black),
        new KeyValuePair<string, Color>(Key: "Yellow", Value: Color.yellow),
        new KeyValuePair<string, Color>(Key: "Blue", Value: Color.blue),
        new KeyValuePair<string, Color>(Key: "Pink", Value: new Color(r: 255, g: 0, b: 150)),
    };

    public static int FindIndexOfColour(Color color)
    {
        for (int i = 0; i < Colours.Length; i++)
        {
            if (Colours[i].Value == color)
                return i;
        }

        return -1;
    }
}

public struct KeyValuePair<T1, T2>
{
    public T1 Key;
    public T2 Value;

    public KeyValuePair(T1 Key, T2 Value) {
        this.Key = Key;
        this.Value = Value;
    }
}