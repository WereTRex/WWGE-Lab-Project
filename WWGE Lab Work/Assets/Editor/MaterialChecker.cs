using System.Linq;
using UnityEngine;
using UnityEditor;

public class MaterialChecker : Editor
{
    [MenuItem("Assets/Check Material")]
    private static void CheckMaterial()
    {
        Material matToCheck = Selection.activeObject as Material;

        foreach (var renderer in FindObjectsOfType<MeshRenderer>())
        {
            if (renderer.sharedMaterials.Contains(matToCheck))
                Debug.Log("Material used by " + renderer.transform.name, renderer.gameObject);
        }
    }
}
