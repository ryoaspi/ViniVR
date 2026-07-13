using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheFoundation.Editor
{
    //TODO
    //Creators vont probablement toujours avoir une base similaire pour la création de ressources.
    //Il pourrait être intéressant d'avoir une manière d'unifier, si appliquable, l'endroit où créer 
    //La ressource.
    //Ex: Feature Creator crée forcément une feature (fixe).
    //Ex: ReadMe Creator pourrait vouloir créer en dehors de features (variable).
    //Ex: Le FBehaviour Creator pourrait vouloir créer dans une complexité de dossier imbriquée (variable).


    public class Creator : EditorWindow
    {

        [MenuItem("TheFoundation/Create/Creator")]
        protected static void ShowWindow()
        {
            
        }
        protected void OnGUI()
        {
            GUILayout.Label($"{_creatorLabel}", EditorStyles.boldLabel);
            GUILayout.Label("Output Folder");
            
            _targetFolder = EditorGUILayout.TextField(_targetFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected) && selected.Contains("Assets"))
                {
                    _targetFolder = "Assets" + selected.Replace(Application.dataPath, "");
                }
            }
            
            EditorGUILayout.EndHorizontal();

        }

        protected string _creatorLabel;
        protected string _targetFolder = "Assets";
    }
    public class FBehaviourScriptCreator : EditorWindow
    {
        private string _scriptName = "NewBehaviour";
        private string _targetFolder = "Assets";
        private string _namespaceName = "Features";

        [MenuItem("TheFoundation/Create/FBehaviour Script")]
        private static void ShowWindow()
        {
            GetWindow<FBehaviourScriptCreator>("Create FBehaviour Script");
        }

        private void OnGUI()
        {
            GUILayout.Label("FBehaviour Script Generator", EditorStyles.boldLabel);
            
            _scriptName = EditorGUILayout.TextField("Script Name",_scriptName);
            _namespaceName = EditorGUILayout.TextField("Namespace",_namespaceName);
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Output Folder");
            EditorGUILayout.BeginHorizontal();
            _targetFolder = EditorGUILayout.TextField(_targetFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected) && selected.Contains("Assets"))
                {
                    _targetFolder = "Assets" + selected.Replace(Application.dataPath, "");
                }
            }
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Script"))
            {
                if (string.IsNullOrWhiteSpace(_scriptName))
                {
                    EditorUtility.DisplayDialog("Error", "Script name cannot be empty", "OK");
                    return;
                }

                CreateScript(_scriptName.Trim(), _namespaceName.Trim(), _targetFolder);
            }
        }

        private void CreateScript(string name, string ns, string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("Error", "Folder doesn't exist", "OK");
                return;
            }

            string path = Path.Combine(folder, $"{name}.cs");

            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("error", $"Script already exists: \n {path}", "OK");
                return;
            }
            
            string script =
                $@"using UnityEngine;
using TheFoundation.Runtime;

namespace {ns}
{{
    public class {name} : FBehaviour
    {{
        #region Fields

        #endregion


        #region Initialization

        protected override void OnInit()
        {{
            // Called once on creation
        }}

        #endregion


        #region Update Loop

        protected override void OnUpdate()
        {{
            // Called every frame
        }}

        protected override void OnFixedUpdate()
        {{
            // Physics updates
        }}

        #endregion


        #region Events

        protected override void OnEnabled()
        {{
        }}

        protected override void OnDisabled()
        {{
        }}

        protected override void OnDestroyed()
        {{
        }}

        #endregion


        #region Private API

        #endregion
    }}
}}";

            File.WriteAllText(path, script);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Script '{name}' created successfully.", "OK");

        }
    }
}
