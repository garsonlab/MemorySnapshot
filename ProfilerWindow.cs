using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TinyJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public static class ProfilerWindow
{
    private static List<Dynamic> _windows = null;

    private static Dynamic _GetWindow(ProfilerArea area)
    {
        if (null == _windows)
        {
            var dynamicType = new DynamicType(typeof(EditorWindow));
            var type = dynamicType.GetType("UnityEditor.ProfilerWindow");
            var list = type.PrivateStaticField<IList>("m_ProfilerWindows");
            _windows = new List<Dynamic>();
            foreach (var window in list)
            {
                _windows.Add(new Dynamic(window));
            }
        }
        foreach (var dynamic in _windows)
        {
            var val = (ProfilerArea)dynamic.PrivateInstanceField("m_CurrentArea");
            if (val == area)
            {
                return dynamic;
            }
        }
        return null;
    }

    public static MemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize, bool sortSize)
    {
        var windowDynamic = _GetWindow(ProfilerArea.Memory);
        if (windowDynamic == null) return null;
        var listViewDynamic = new Dynamic(windowDynamic.PrivateInstanceField("m_MemoryListView"));
        var rootDynamic = listViewDynamic.PrivateInstanceField("m_Root");
        return rootDynamic != null ? MemoryElement.Create(new Dynamic(rootDynamic), 0, filterDepth, filterSize, sortSize) : null;
    }

    public static void WriteMemoryDetail(StreamWriter writer, MemoryElement root)
    {
        if (null == root) return;
        writer.WriteLine(root.ToString());
        foreach (var memoryElement in root.children)
        {
            if (null != memoryElement)
            {
                WriteMemoryDetail(writer, memoryElement);
            }
        }
    }

    public static void RefreshMemoryData()
    {
        
        var dynamic = _GetWindow(ProfilerArea.Memory);
        if (null != dynamic)
        {
            dynamic.CallPrivateInstanceMethod("RefreshMemoryData");
        }
        else
        {
            Debug.Log("请打开Profiler 窗口的 Memory 视图");
        }
    }

    public static void ExtractMemory(Node root, MemoryElement element)
    {
        if(element == null)
            return;

        string name = string.IsNullOrEmpty(element.name) ? "-" : element.name;
        if (element.children.Count == 0)
        {
            root[name] = element.GetSize();
        }
        else
        {
            var node = Node.NewTable();
            root[$"{name}-{element.GetSize()}"] = node;
            foreach (var child in element.children)
            {
                ExtractMemory(node, child);
            }
        }
    }
}

public class DynamicType
{
    private readonly Assembly _assembly;

    public DynamicType(Type type)
    {
        _assembly = type.Assembly;
    }

    public Dynamic GetType(string typeName)
    {
        return new Dynamic(_assembly.GetType(typeName));
    }
}
