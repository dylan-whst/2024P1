using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using _2024P1;
using MudBlazor.Services;
using P1.Models;
using P1.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<IGameBoardViewModel, GameBoardViewModel>();
builder.Services.AddScoped<IGameService, StubGameService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IHandService, HandService>(_ => 
    new HandService([
        new Card() { Id = 1, Letter = 'a'},
        new Card() { Id = 2, Letter = 'b'},
        new Card() { Id = 3, Letter = 'c'},
        new Card() { Id = 4, Letter = 'd'},
        new Card() { Id = 5, Letter = 'e'},
    ]));

await builder.Build().RunAsync();