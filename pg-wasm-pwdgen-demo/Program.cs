using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PG.Logic.Passwords.Generators;
using PG.Shared.Services;
using pg_wasm_pwdgen_demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
	.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
	.AddSingleton<PasswordGeneratorFactory>()
	.AddTransient<RandomService>()
	.BuildServiceProvider();

await builder.Build().RunAsync();
