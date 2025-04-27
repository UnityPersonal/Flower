using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace GolemKin.Swarm
{
    public class SwarmInstaller : EditorWindow
{
    private const string PackageName = "com.unity.mathematics";
    private static AddRequest _addRequest;
    private static bool isInstalled;

    [MenuItem("Tools/Swarm Installer")]
    public static void ShowWindow()
    {
        GetWindow<SwarmInstaller>("Swarm Installer");
    }

    private void OnEnable()
    {
        // Check if the package is installed upon opening the window
        isInstalled = IsMathematicsPackageInstalled();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Swarm Installer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("This installer will add the Unity Mathematics package,", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("which is required for optimal performance of the asset.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        if (isInstalled)
        {
            EditorGUILayout.HelpBox("Unity Mathematics package is already installed.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Unity Mathematics package is not installed.", MessageType.Warning);

            if (GUILayout.Button("Install Unity Mathematics"))
            {
                InstallMathematicsPackage();
            }
        }
    }

    private static void InstallMathematicsPackage()
    {
        if (!isInstalled)
        {
            Debug.Log("Installing Unity Mathematics...");
            _addRequest = Client.Add(PackageName);
            EditorApplication.update += PackageInstallProgress;
        }
    }

    private static void PackageInstallProgress()
    {
        if (_addRequest.IsCompleted)
        {
            if (_addRequest.Status == StatusCode.Success)
            {
                Debug.Log("Unity Mathematics package installed successfully.");
                isInstalled = true;
            }
            else if (_addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError("Failed to install Unity Mathematics package: " + _addRequest.Error.message);
            }

            EditorApplication.update -= PackageInstallProgress;
        }
    }

    private static bool IsMathematicsPackageInstalled()
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (File.Exists(manifestPath))
        {
            string manifestContent = File.ReadAllText(manifestPath);
            return manifestContent.Contains(PackageName);
        }

        return false;
    }
}
}