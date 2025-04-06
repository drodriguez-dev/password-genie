using System.Reflection;

namespace PG.Shared.Services
{
	public class ApplicationService
	{
		public static string GetVersion()
		{
			var assembly = Assembly.GetEntryAssembly();
			var versionAttribute = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>();
			return versionAttribute?.Version ?? "Version not found";
		}
	}
}
