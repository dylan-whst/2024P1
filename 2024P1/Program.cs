using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using _2024P1;
using _2024P1.Game;
using MudBlazor.Services;
using P1.Models;
using P1.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IGameBoardViewModel, GameBoardViewModel>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IWordValidator, DictionaryApiWordValidator>();
builder.Services.AddScoped<IPlayCardsService, PlayCardsService>();
builder.Services.AddScoped<IDiscardSelectionService, DiscardSelectionService>();

builder.Services.AddScoped<IGameplayRulesService, GameplayRulesService>();
builder.Services.AddScoped<ITurnService, TurnService>();
builder.Services.AddScoped<ICardMovementService, CardMovementService>();
builder.Services.AddScoped<IDeckService, DeckService>();
var handService = new HandService([]);
handService.HandSize = 6;
builder.Services.AddScoped<IHandService, HandService>(_ => handService);


builder.Services.AddMudServices();

await builder.Build().RunAsync();