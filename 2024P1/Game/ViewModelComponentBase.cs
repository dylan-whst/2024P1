// ViewModelComponentBase.cs
using Microsoft.AspNetCore.Components;
using P1.Services;

public abstract class ViewModelComponentBase : ComponentBase, IDisposable
{
    [Inject] protected IGameBoardViewModel _viewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        _viewModel.OnViewStateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        _viewModel.OnViewStateChanged -= StateHasChanged;
    }
}