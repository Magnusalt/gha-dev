using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using simple_api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedTransient<ICustomService, CustomServiceA>("A");
builder.Services.AddKeyedTransient<ICustomService, CustomServiceB>("B");
builder.Services.AddSingleton<EventBasedRelay>();

var app = builder.Build();

app.MapPost("/user", (DataAccessLayer dal, [FromBody] UserInformation user) =>
    dal.UpdateUserInformation(user));

app.MapGet("/trigger", ctx =>
{
    var relay = ctx.RequestServices.GetService<EventBasedRelay>();

    relay.RaiseOnEvent("triggered");

    return ctx.Response.WriteAsync("triggered event");
});

app.MapGet("/events", (HttpContext ctx, CancellationToken cancellationToken) =>
    TypedResults.ServerSentEvents<string>(ctx.RequestServices.GetKeyedService<ICustomService>("A")
        .DoSomething(cancellationToken)));

app.MapGet("/eventBasedEvent", (EventBasedRelay relay, HttpContext ctx) =>
{
    var t = new TaskCompletionSource<string>();

    void OnRelayEvent(object? sender, string s)
    {
        t.TrySetResult(s);
    }

    relay.OnEvent += OnRelayEvent;

    async IAsyncEnumerable<string> Processor()
    {
        try
        {
            yield return "started";
            while (!ctx.RequestAborted.IsCancellationRequested)
            {
                var localTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                t = localTcs;
                await using var reg = ctx.RequestAborted.Register(() => localTcs.TrySetCanceled());

                var mes = await t.Task;
                yield return mes;
            }
        }
        finally
        {
            relay.OnEvent -= OnRelayEvent;
        }
    }

    return TypedResults.ServerSentEvents(Processor());
});

app.Run();


public interface ICustomService
{
    IAsyncEnumerable<string> DoSomething(CancellationToken cancellationToken = default);
}

public class CustomServiceA : ICustomService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

    public async IAsyncEnumerable<string> DoSomething([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _timer.WaitForNextTickAsync(cancellationToken);
            yield return "I am doing something";
        }
    }
}

public class CustomServiceB : ICustomService
{
    public IAsyncEnumerable<string> DoSomething(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class EventBasedRelay
{
    public event EventHandler<string>? OnEvent;

    public void RaiseOnEvent(string eventName)
    {
        OnEvent?.Invoke(this, eventName);
    }
}