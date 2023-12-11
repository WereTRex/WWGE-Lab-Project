using System.Collections.Generic;
using UnityEngine;

/// <summary> A struct containing the colours available to the Crosshair</summary>
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

    /// <summary> Find the index of a CrosshairColour from a passed colour. </summary>
    public static int FindIndexOfColour(Color color)
    {
        // Loop through each value in the Colours KVP array.
        for (int i = 0; i < Colours.Length; i++)
        {
            // If the two colours are the same, then return this index.
            if (Colours[i].Value == color)
                return i;
        }

        // We couldn't find this colour.
        return -1;
    }
}

/// <summary> A struct representing a pair of Key (T1) and a Value (T2).</summary>
public struct KeyValuePair<T1, T2>
{
    public T1 Key;
    public T2 Value;

    public KeyValuePair(T1 Key, T2 Value) {
        this.Key = Key;
        this.Value = Value;
    }
}