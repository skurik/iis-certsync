using System.IO;
using Microsoft.Web.Administration;

var allArgs = string.Join(" ", Env.ScriptArgs);

Console.WriteLine($"All arguments ({Env.ScriptArgs.Count} items): {allArgs}");

var logPath = Env.ScriptArgs[0];
var logDir = Path.GetDirectoryName(logPath);
if (!Directory.Exists(logDir))
	Directory.CreateDirectory(logDir);

using (var stream = File.Open(logPath, FileMode.OpenOrCreate | FileMode.Append))
using (var writer = new StreamWriter(stream))
{
	try
	{		
		var sourceSiteName = Env.ScriptArgs[1];
		var sourceBindingInfo = Env.ScriptArgs[2];
		var targetList = Env.ScriptArgs[3];

		writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Source site and binding info: {sourceSiteName}/{sourceBindingInfo}");
		writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Target list: {targetList}");

		var targets = targetList.Split(',').Select(target =>
		{
			var tokens = target.Split('|');
			if (string.IsNullOrEmpty(tokens[0]) || string.IsNullOrEmpty(tokens[1]))
				throw new Exception($"Invalid target specification: '{target}'");

			writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Target added: {tokens[0]}/{tokens[1]}");
			return new { Site = tokens[0], Binding = tokens[1] };
		}).ToList();

		var serverManager = new ServerManager();
		var sourceSite = serverManager.Sites[sourceSiteName];
		var sourceBinding = Helper.GetBinding(sourceSite, sourceBindingInfo);
		var certificateHash = sourceBinding.CertificateHash;
		if (certificateHash == null) throw new Exception("The source binding's CertificateHash is null");
		var certificateHashString = string.Concat(certificateHash.Select(b => string.Format("{0:X}", b)));
		Console.WriteLine($"Will use the certificate hash {certificateHashString}");
		writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Will use the certificate hash {certificateHashString}");
		var certificateStoreName = sourceBinding.CertificateStoreName;
		if (certificateStoreName == null) throw new Exception("The source binding's CertificateStoreName is null");
		Console.WriteLine($"Will use the certificate store name {certificateStoreName}");
		writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Will use the certificate store name {certificateStoreName}");

		foreach (var target in targets)
		{
			var site = serverManager.Sites[target.Site];
			var binding = Helper.GetBinding(site, target.Binding);
			var currentCertificateHashString = binding.CertificateHash == null ? string.Empty : string.Concat(binding.CertificateHash.Select(b => string.Format("{0:X}", b)));
			if (currentCertificateHashString == certificateHashString && binding.CertificateStoreName == certificateStoreName)
			{
				Console.WriteLine($"{site}/{binding} already up-to-date, skipping");
				writer.WriteLine($"{DateTime.UtcNow.ToString("o")} {site}/{binding} already up-to-date, skipping");
				continue;
			}

			var method = binding.Methods["AddSslCertificate"];
			if (method == null)
		    	throw new Exception("Unable to access the AddSslCertificate configuration method");

			var mi = method.CreateInstance();
			mi.Input.SetAttributeValue("certificateHash", certificateHashString);
			mi.Input.SetAttributeValue("certificateStoreName", certificateStoreName);
			mi.Execute();
			Console.WriteLine($"Updated {site}/{binding} to use {certificateHashString}");
			writer.WriteLine($"{DateTime.UtcNow.ToString("o")} Updated {site}/{binding} to use {certificateHashString}");
		}
	}
	catch (Exception e)
	{
		writer.WriteLine($"{DateTime.UtcNow.ToString("o")} ERROR: {e.Message}");
		writer.WriteLine(e.StackTrace);
		throw;
	}
}

public static class Helper
{
	public static Binding GetBinding(Site site, string bindingInfo)
	{
		// foreach (var b in site.Bindings)
		// {
		// 	Console.WriteLine($"Binding information: {b.BindingInformation}");
		// 	Console.WriteLine($"Binding EndPoint: {b.EndPoint}");
		// 	Console.WriteLine($"Binding Host: {b.Host}");
		// 	Console.WriteLine($"Binding IsIPPortHostBinding: {b.IsIPPortHostBinding}");
		// 	Console.WriteLine($"Binding Protocol: {b.Protocol}");
		// 	Console.WriteLine($"Binding: {b.ToString()}");
		// }

		var binding = site.Bindings.SingleOrDefault(b => b.Protocol.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase) && b.BindingInformation.Equals(bindingInfo));
		if (binding == null)
			throw new Exception($"Could not find the binding '{bindingInfo}' on site '{site}'");

		return binding;
	}
}