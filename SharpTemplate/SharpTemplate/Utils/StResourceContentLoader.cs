// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
