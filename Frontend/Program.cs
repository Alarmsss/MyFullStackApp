using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend; //引用本地命名空間，通常會用到 App 元件
using Frontend.Services; //引入服務層，例如自訂的 GoogleAuthService、PkceHelper

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app"); //App 是專案中的 App.razor，是整個應用的入口組件
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddScoped<PkceHelper>();

// 加載配置
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var response = await http.GetAsync("appsettings.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

await builder.Build().RunAsync();
