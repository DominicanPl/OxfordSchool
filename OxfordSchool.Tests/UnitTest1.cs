using System.ComponentModel.DataAnnotations;
using OxfordSchool.API.DTOs;

namespace OxfordSchool.Tests;

public class DtoValidationTests
{
    [Fact]
    public void CreateEntregaRequest_ConUrlInvalida_DebeFallarValidacion()
    {
        var request = new CreateEntregaRequest
        {
            ArchivoAdjuntoUrl = "archivo-local",
            TareaId = 1
        };

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateEntregaRequest.ArchivoAdjuntoUrl)));
    }

    [Fact]
    public void CalificarEntregaRequest_FueraDeRango_DebeFallarValidacion()
    {
        var request = new CalificarEntregaRequest
        {
            Calificacion = 120
        };

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CalificarEntregaRequest.Calificacion)));
    }

    [Fact]
    public void CreateMateriaRequest_SinNombre_DebeFallarValidacion()
    {
        var request = new CreateMateriaRequest
        {
            Nombre = string.Empty
        };

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMateriaRequest.Nombre)));
    }

    private static List<ValidationResult> Validate<T>(T model)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model!, context, results, true);
        return results;
    }
}
