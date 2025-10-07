//using UnityEngine;
//using UnityEditor;

//public class BulkPrefabReplacer : EditorWindow
//{
//    public GameObject prefab;

//    [MenuItem("Tools/Bulk Prefab Replacer")]
//    static void Init()
//    {
//        GetWindow<BulkPrefabReplacer>("Prefab Replacer");
//    }

//    void OnGUI()
//    {
//        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

//        if (prefab == null)
//        {
//            EditorGUILayout.HelpBox("Assign a prefab to replace with.", MessageType.Warning);
//            return;
//        }

//        if (GUILayout.Button("Replace Selected"))
//        {
//            ReplaceSelected();
//        }
//    }

//    void ReplaceSelected()
//    {
//        foreach (GameObject go in Selection.gameObjects)
//        {
//            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform.parent);
//            newObj.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
//            newObj.transform.localScale = go.transform.localScale;

//            Undo.RegisterCreatedObjectUndo(newObj, "Replace With Prefab");
//            Undo.DestroyObjectImmediate(go);
//        }
//    }
//}