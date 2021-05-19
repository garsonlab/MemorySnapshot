using System;
using System.Diagnostics;
using System.IO;
using TinyJSON;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExtractMemoryEditor: EditorWindow
{

    private float _memorySize = 1f;
    private int _memoryDepth = 10;
    private bool _sortSize = false;

    public static ExtractMemoryEditor Window;

    [MenuItem("Window/Extract Profiler Memory")]
    public static void ShowWindow()
    {
        EditorApplication.ExecuteMenuItem("Window/Profiler");
        if (Window == null)
        {
            Window = CreateInstance<ExtractMemoryEditor>();
        }
        Window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Take Sameple前需要选中Profiler中的Memory视图并选择Detail\nExtract Memory结果保存至E:/MemorySnapshot", MessageType.Info);
        
        EditorGUILayout.LabelField("当前Profiler连接对象: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));

        if (GUILayout.Button("Take Sample"))
        {
            TakeSample();
        }

        GUILayout.Space(20);
        
        _memorySize = EditorGUILayout.FloatField("获取项最小内存(KB) >= ", _memorySize);
        _memoryDepth = EditorGUILayout.IntField("获取项最大深度(>=1)", _memoryDepth);
        _sortSize = EditorGUILayout.Toggle("导出时按内存排序", _sortSize);

        if (GUILayout.Button("Extract Memory"))
        {
            if (_memoryDepth <= 0 )
            {
                _memoryDepth = 1;
            }
            ExtractMemory(_memorySize, _memoryDepth - 1);
        }
        
    }
    private MemoryElement _memoryElementRoot;
    private void ExtractMemory(float memSize, int memDepth)
    {
        var filterSize = memSize * 1024;
        var parent = "E:/MemorySnapshot";// Directory.GetParent(Application.dataPath);
        if (!Directory.Exists(parent))
            Directory.CreateDirectory(parent);
        
        var outputPath = $"{parent}/MemoryDetailed{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.json";
        File.Create(outputPath).Dispose();

        _memoryElementRoot = ProfilerWindow.GetMemoryDetailRoot(memDepth, filterSize, _sortSize);

        Node root = Node.NewTable();
        root["获取项最小内存KB(>=)"] = this._memorySize;
        root["获取项最大深度"] = this._memoryDepth;
        root["导出时按内存排序"] = this._sortSize;
        root["当前Profiler连接对象"] = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);
        
        if (this._memoryElementRoot != null)
        {
            ProfilerWindow.ExtractMemory(root, this._memoryElementRoot);
        }
        
        var json = new TinyJSON.Printer(true).String(root);
        File.WriteAllText(outputPath, json);

        Process.Start(outputPath);
    }

    private static void TakeSample()
    {
        ProfilerWindow.RefreshMemoryData();
    }
}
