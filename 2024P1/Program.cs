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
builder.Services.AddScoped<IWordValidator, DictionaryApiWordValidator>();
builder.Services.AddScoped<IPlayCardsService, PlayCardsService>();
builder.Services.AddScoped<IHandService, HandService>(_ => 
    new HandService(
        Enumerable.Range('a', 7).Select((c, index) => (Card)new LetterCard()
        {
            Id = index + 1,
            Letter = (char)c,
            Points = index
        }).ToList()
    ));

await builder.Build().RunAsync();