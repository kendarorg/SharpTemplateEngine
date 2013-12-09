using System.IO;
using System.Reflection;

namespace SharpTemplate.Utils
{
	public static class ResourceContentLoader
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
