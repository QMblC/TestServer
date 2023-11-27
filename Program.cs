using Microsoft.VisualBasic;
using System;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var users = new List<Person>
{
new Person(Guid.NewGuid().ToString(), "James", 18),
new Person(Guid.NewGuid().ToString(), "Tom", 25),
new Person(Guid.NewGuid().ToString(), "Bill", 54),
};

app.Run(async (context) =>
{
    var expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    var path = context.Request.Path;

    if (context.Request.Method == "GET" && path == "/api/users")
        await GetPeople(context.Response);
    else if (context.Request.Method == "GET" && Regex.IsMatch(path, expressionForGuid))
    {
        var id = path.Value?.Split('/')[3];
        await GetPerson(context.Response, id);
    }
    else if (context.Request.Method == "POST" && path == "/api/users")
    {
        await CreatePerson(context.Response, context.Request);
    }
    else if (context.Request.Method == "PUT" && path == "/api/users")
    {
        await UpdatePerson(context.Response, context.Request);
    }
    else if (context.Request.Method == "DELETE" && Regex.IsMatch(path, expressionForGuid))
    {
        var id = path.Value?.Split('/')[3];
        await DeletePerson(context.Response, id);
    }
    else
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        var updatedPath = "";
        if (path != "/")
            updatedPath = "Pages/" + path;
        else
            updatedPath = "Pages/index.html";
        if (!updatedPath.Contains(".html"))
            updatedPath += ".html";
        await context.Response.SendFileAsync(updatedPath);
    }


});
app.Run();

async Task GetPeople(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

async Task GetPerson(HttpResponse response, string? id)
{
    var desiredUser = users.FirstOrDefault(user => user.Id == id);
    if (desiredUser != null)
        await response.WriteAsJsonAsync(desiredUser);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Пользователь с данным ID: {desiredUser} не найден" });
    }
}

async Task CreatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        var user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
            users.Add(new Person(Guid.NewGuid().ToString()));
            await response.WriteAsJsonAsync(user);
        }
        else
            throw new Exception("Некорректные данные");
    }
    catch
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

async Task UpdatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        var userData = await request.ReadFromJsonAsync<Person>();
        if (userData != null)
        {
            var user = users.FirstOrDefault(user => user.Id == userData.Id);
            if (user != null)
            {
                user.UpdateName(userData.Name);
                user.UpdateAge(userData.Age);
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Пользователь с ID: {userData.Id} не найден" });
            }
        }
    }
    catch
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

async Task DeletePerson(HttpResponse response, string? id)
{
    var desiredUser = users.FirstOrDefault(user => user.Id == id);
    if (desiredUser != null)
    {
        users.Remove(desiredUser);
        await response.WriteAsJsonAsync(desiredUser);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
    }
}

class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }

    public Person()
    {

    }

    public Person(string id)
    {
        Id = id;
    }

    public Person(string id, string name, int age)
    {
        Id = id;
        Name = name;
        Age = age;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateAge(int age)
    {
        Age = age;
    }

}