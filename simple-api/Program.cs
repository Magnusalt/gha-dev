using Microsoft.AspNetCore.Mvc;
using simple_api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DataAccessLayer>();

var app = builder.Build();

app.MapPost("/user", (DataAccessLayer dal, [FromBody] UserInformation user) =>
    dal.UpdateUserInformation(user));

app.Run();
