using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Sdk;

namespace Tests.UI.OxfordSchool;

public class FlujoPrincipalE2ETests : PageTest
{

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        IgnoreHTTPSErrors = true
    };

    [Fact]
    public async Task FlujoPrincipal_LoginConsultarTareasYSubirEntrega()
    {
        var webBaseUrl = Environment.GetEnvironmentVariable("E2E_WEB_BASE_URL") ?? "http://localhost:5173";
        var apiBaseUrl = Environment.GetEnvironmentVariable("E2E_API_BASE_URL") ?? "https://localhost:7040/api";
        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var correo = $"estudiante.{unique}@oxfordschool.local";
        var contrasena = "Pass1234!";

        using var anonymousApiContext = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = apiBaseUrl,
            IgnoreHTTPSErrors = true
        });

        var registerResponse = await anonymousApiContext.PostAsync("/auth/register", new APIRequestContextOptions
        {
            DataObject = new
            {
                nombre = "Estudiante E2E",
                correo,
                contrasena,
                rol = "Estudiante"
            }
        });

        if (!registerResponse.Ok)
        {
            var registerDetail = await registerResponse.TextAsync();
            throw new XunitException($"No se pudo registrar el usuario de pruebas ({registerResponse.Status}). Detalle: {registerDetail}");
        }

        await Page.GotoAsync($"{webBaseUrl}/login");

        await Page.GetByPlaceholder("Correo").FillAsync(correo);
        await Page.GetByPlaceholder("Contraseña").FillAsync(contrasena);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ingresar" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@".*/dashboard$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Bienvenido:")).ToBeVisibleAsync();

        using var authenticatedApiContext = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = apiBaseUrl,
            IgnoreHTTPSErrors = true,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {await ObtenerTokenActualAsync()}"
            }
        });

        var tareasResponse = await authenticatedApiContext.GetAsync("/tareas");
        if (!tareasResponse.Ok)
        {
            var tareasDetail = await tareasResponse.TextAsync();
            throw new XunitException($"No fue posible consultar tareas ({tareasResponse.Status}). Detalle: {tareasDetail}");
        }

        var tareasDoc = JsonDocument.Parse(await tareasResponse.TextAsync());
        if (tareasDoc.RootElement.ValueKind != JsonValueKind.Array || tareasDoc.RootElement.GetArrayLength() == 0)
        {
            throw new XunitException("No hay tareas disponibles para completar el flujo E2E. Crea una tarea primero y vuelve a ejecutar.");
        }

        var primeraTarea = tareasDoc.RootElement.EnumerateArray().First();
        var tareaId = primeraTarea.GetProperty("id").GetInt32();
        var materiaNombre = primeraTarea.GetProperty("materiaNombre").GetString() ?? string.Empty;

        await Expect(Page.Locator("ul li").First).ToContainTextAsync(materiaNombre);

        var entregaResponse = await authenticatedApiContext.PostAsync("/entregas", new APIRequestContextOptions
        {
            DataObject = new
            {
                archivoAdjuntoUrl = $"https://entregas.oxfordschool.local/{unique}.txt",
                tareaId
            }
        });

        if (entregaResponse.Status is not 201 and not 409)
        {
            var entregaDetail = await entregaResponse.TextAsync();
            throw new XunitException($"No se pudo registrar la entrega ({entregaResponse.Status}). Detalle: {entregaDetail}");
        }

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión" })).ToBeVisibleAsync();
    }

    private async Task<string> ObtenerTokenActualAsync()
    {
        var token = await Page.EvaluateAsync<string>("() => localStorage.getItem('token') || ''");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new XunitException("No se encontró el token en localStorage después del login.");
        }

        return token;
    }
}
