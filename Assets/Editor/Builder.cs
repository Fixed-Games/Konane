﻿using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TrainingProject
{
    public static class Builder
    {
        public static string Separator => Path.DirectorySeparatorChar.ToString();

        [MenuItem("Tools/Build/All")]
        public static void BuildAll()
        {
            string fileName1 = string.Join(Separator, "Bin", "All", "konane.apk");
            string fileName2 = string.Join(Separator, "Bin", "All", "konane.exe");
            if (EditorUtility.DisplayDialog("Build All", fileName1 + '\n' + fileName2, "OK", "Cancel"))
            {
                Build(fileName1, BuildTarget.Android);
                Build(fileName2, BuildTarget.StandaloneWindows64);
            }
        }

        [MenuItem("Tools/Build/Android", false, 0)]
        public static void BuildAndroid()
        {
            string fileName = string.Join(Separator, "Bin", "Android", "konane.apk");
            if (EditorUtility.DisplayDialog("Build Android", fileName, "OK", "Cancel"))
            {
                Build(fileName, BuildTarget.Android);
            }
        }

        [MenuItem("Tools/Build/Standalone (Win64)", false, 0)]
        public static void BuildStandaloneWindows64()
        {
            string fileName = string.Join(Separator, "Bin", "Win64", "konane.exe");
            if (EditorUtility.DisplayDialog("Build Standalone (Win64)", fileName, "OK", "Cancel"))
            {
                Build(fileName, BuildTarget.StandaloneWindows64);
            }
        }

        public static void BuildViaCommandLine()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string dir = null;
            string version = null;
            for (int i = 0; i < args.Length; ++i)
            {
                switch (args[i].Trim('"'))
                {
                    case "--buildingFolder":
                        dir = args[++i].Trim('"');
                        break;
                    case "--buildingVersion":
                        version = args[++i].Trim('"');
                        break;
                }
            }
            if (dir == null)
            {
                dir = string.Join(Separator, "Bin", "Release");
            }
            if (version != null)
            {
                PlayerSettings.bundleVersion = version;
            }
#if UNITY_ANDROID
            EditorApplication.Exit(Build(Path.Combine(dir, "konane.apk"), BuildTarget.Android));
#elif UNITY_STANDALONE_WIN
            EditorApplication.Exit(Build(Path.Combine(dir, "konane.exe"), BuildTarget.StandaloneWindows64));
#else
            EditorApplication.Exit(2);
#endif
        }

        private static int Build(string fileName, BuildTarget buildTarget)
        {
            BuildReport buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, fileName, buildTarget, BuildOptions.StrictMode);
            if (BuildResult.Succeeded.Equals(buildReport.summary.result))
            {
                return 0;
            }
            return 1;
        }
    }
}
