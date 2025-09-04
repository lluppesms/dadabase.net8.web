//-----------------------------------------------------------------------
// <copyright file="Index.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2024, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Index Page Code Behind
// </summary>
//-----------------------------------------------------------------------
using DadABase.Web.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace DadABase.Web.Pages;

/// <summary>
/// Index Page Code Behind
/// </summary>
[AllowAnonymous]
public partial class Index : ComponentBase
{
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IChatAgent ChatAgent { get; set; }

    private Joke myJoke = new();
    private readonly bool addDelay = false;
    private LoadingIndicator jokeLoadingIndicator;
    private SnackbarStack snackbarstack;

    // Store the last 10 jokes
    private List<Joke> jokeHistory = new();
    private bool isHistoryCollapsed = true;

    private string jokeImageMessage = string.Empty;
    private string jokeImageDescription = string.Empty;
    private string jokeImageUrl = string.Empty;

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            await ExecuteRandom();
            StateHasChanged();
        }
    }

    private async Task ExecuteRandom()
    {
        myJoke = new();
        await jokeLoadingIndicator.Show();
        var timer = Stopwatch.StartNew();
        if (addDelay) { await Task.Delay(500).ConfigureAwait(false); } // I want to see the spinners for now...
        myJoke = JokeRepository.GetRandomJoke();
        // Add to history, but skip if duplicate of last
        if (jokeHistory.Count == 0 || jokeHistory[0].JokeTxt != myJoke.JokeTxt)
        {
            jokeHistory.Insert(0, myJoke);
            if (jokeHistory.Count > 10)
                jokeHistory.RemoveAt(jokeHistory.Count - 1);
        }
        var elaspsedMS = timer.ElapsedMilliseconds;
        await jokeLoadingIndicator.Hide().ConfigureAwait(false);
        await snackbarstack.PushAsync($"Joke Elapsed: {(decimal)elaspsedMS / 1000m:0.0} seconds", SnackbarColor.Info).ConfigureAwait(false);
        jokeImageMessage = string.Empty;
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;
    }
    private async Task CreatePicture()
    {
        jokeImageMessage = "🚀 Generating image... this will take a sec... please wait!";
        jokeImageUrl = string.Empty;
        jokeImageDescription = string.Empty;

        (jokeImageDescription, jokeImageUrl, _) = await ChatAgent.ChatWithAgent(myJoke.JokeTxt);
        jokeImageMessage = "Here you go!";

        StateHasChanged();
    }
    private void ToggleHistory()
    {
        isHistoryCollapsed = !isHistoryCollapsed;
    }
}
