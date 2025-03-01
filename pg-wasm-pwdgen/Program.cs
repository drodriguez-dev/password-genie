using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PG.Data.Files.DataFiles;
using PG.Logic.Passwords.Extractors;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Loaders;
using PG.Shared.Services;
using PG.Wasm.PasswordGenerator;

internal class Program
{
	private static async Task Main(string[] args)
	{
		var builder = WebAssemblyHostBuilder.CreateDefault(args);
		builder.RootComponents.Add<App>("#app");
		builder.RootComponents.Add<HeadOutlet>("head::after");

		builder.Services
			.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
			.AddBlazorBootstrap()
			.AddSingleton<PasswordGeneratorFactory>()
			.AddSingleton<WordExtractorFactory>()
			.AddSingleton<IDictionaryLoaderFactory, DictionaryLoaderFactory>()
			.AddSingleton<IDictionariesDataFactory, DictionariesDataFactory>()
			.AddTransient<RandomService>()
			.BuildServiceProvider();

		await builder.Build().RunAsync();
	}
}