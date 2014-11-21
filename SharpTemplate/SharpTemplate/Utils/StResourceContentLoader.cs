// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System.IO;
using System.Reflection;

namespace SharpTemplate.Utils
{
	public static class StResourceContentLoader
	{
		/// <summary>
		/// Read text from a resource inside an assembly
		/// </summary>
		/// <param name="resourceName"></param>
		/// <param name="assembly"></param>
		/// <param name="throwIfNotFound"></param>
		/// <returns></returns>
		public static string LoadText(string resourceName, Assembly assembly, bool throwIfNotFound = true)
		{
			string result;
			var realResourceName = GetResourceName(resourceName, assembly, throwIfNotFound);
			if (realResourceName == null) return null;
			using (var stream = assembly.GetManifestResourceStream(realResourceName))
			{
				if (stream == null) return null;
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			return result;
		}

		/// <summary>
		/// Read binary from a resource inside an assembly
		/// </summary>
		/// <param name="resourceName"></param>
		/// <param name="assembly"></param>
		/// <param name="throwIfNotFound"></param>
		/// <returns></returns>
		public static byte[] LoadBytes(string resourceName, Assembly assembly, bool throwIfNotFound = true)
		{
			byte[] result;
			var realResourceName = GetResourceName(resourceName, assembly, throwIfNotFound);
			if (realResourceName == null) return null;
			using (var stream = assembly.GetManifestResourceStream(realResourceName))
			{
				if (stream == null) return null;
				using (var reader = new BinaryReader(stream))
				{
					result = reader.ReadBytes((int)stream.Length);
				}
			}
			return result;
		}

		/// <summary>
		/// Get a resource path matching the required
		/// </summary>
		/// <param name="resourcePath"></param>
		/// <param name="asm"></param>
		/// <param name="throwIfNotFound"></param>
		/// <returns></returns>
		public static string GetResourceName(string resourcePath, Assembly asm, bool throwIfNotFound = true)
		{
			var lowerResourcePath = resourcePath.ToLowerInvariant();
			foreach (var resource in asm.GetManifestResourceNames())
			{
				var resourceName = resource.ToLowerInvariant();
				if (resourceName.EndsWith(lowerResourcePath))
				{
					return resource;
				}
			}
			if (throwIfNotFound)
			{
				throw new FileNotFoundException(string.Format("Resource matching '{0}' not found in assembly '{1}", resourcePath, asm.FullName));
			}
			return null;
		}
	}
}
