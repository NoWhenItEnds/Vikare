using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vikare.Utilities.Extensions
{
    /// <summary> Helpful extensions for working with resources. </summary>
    public static class ResourceExtensions
    {
        /// <summary> Search the given directory, recursively, and load all the resources of the given type. </summary>
        /// <typeparam name="T"> The type of resource to load. </typeparam>
        /// <param name="directoryPath"> The path of the root directory. </param>
        /// <returns> An array of loaded resources. </returns>
        public static T[] GetResources<T>(String directoryPath) where T : Resource
        {
            String[] resourcePaths = FileExtensions.GetFilepaths(directoryPath, [".tres"]);

            List<T> results = new List<T>();
            foreach (String path in resourcePaths)
            {
                // Check the resource is of the correct type.
                Resource resource = ResourceLoader.Load(path);
                if (resource is T castResource)
                {
                    results.Add(castResource);
                }
            }

            return results.ToArray();
        }


        /// <summary> Search the given directory, recursively, and load all the resources of the given type into a searchable map based upon their filenames. </summary>
        /// <typeparam name="T"> The type of resource to load. </typeparam>
        /// <param name="directoryPath"> The path of the root directory. </param>
        /// <param name="divider"> The divider to use to separate the filename into a searchable path. </param>
        /// <returns> A mapped array of path to loaded resource. </returns>
        public static Dictionary<String, T> GetMappedResources<T>(String directoryPath, Char divider = '_') where T : Resource
        {
            T[] resources = GetResources<T>(directoryPath);
            Dictionary<String, T> map = new Dictionary<String, T>();    // TODO - Is String[] the best way to represent a path? Am I over-complicating it?

            foreach (T resource in resources)
            {
                String fullPath = resource.ResourcePath;
                String resourceName = Path.GetFileNameWithoutExtension(fullPath);
                String[] path = resourceName.Split(divider, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Boolean wasAdded = map.TryAdd(String.Join('.', path), resource);
                if(!wasAdded)
                {
                    GD.PushWarning($"Attempted to map resource to a pre-existing path. Path was <{path}>.");
                }
            }

            return map;
        }
    }
}
