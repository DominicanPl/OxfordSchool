# Ejecución y demostración del Sprint final (QA)

## 1) Precondiciones
- Tener SQL Server disponible con la base de datos del proyecto.
- API levantada en `https://localhost:7040`.
- Frontend levantado en `http://localhost:5173`.
- SDK de .NET 10 instalado.

## 2) Inicializar Playwright (solo la primera vez)
Desde la raíz del repositorio:

```powershell
dotnet build Tests.UI.OxfordSchool/Tests.UI.OxfordSchool.csproj
pwsh Tests.UI.OxfordSchool/bin/Debug/net10.0/playwright.ps1 install
```

## 3) Ejecutar pruebas E2E (flujo principal)
La prueba implementada automatiza:
- Login de estudiante.
- Consulta de tareas/materia disponible.
- Registro de entrega sobre la primera tarea encontrada.

Comando:

```powershell
dotnet test Tests.UI.OxfordSchool/Tests.UI.OxfordSchool.csproj
```

Variables opcionales:
- `E2E_WEB_BASE_URL` (default: `http://localhost:5173`)
- `E2E_API_BASE_URL` (default: `https://localhost:7040/api`)

Ejemplo:

```powershell
$env:E2E_WEB_BASE_URL="http://localhost:5173"
$env:E2E_API_BASE_URL="https://localhost:7040/api"
dotnet test Tests.UI.OxfordSchool/Tests.UI.OxfordSchool.csproj
```

## 4) Demostración en video
Checklist sugerido para grabación:
1. Mostrar API y Frontend ejecutándose localmente.
2. Ejecutar la prueba desde Test Explorer o terminal.
3. Evidenciar resultado `Passed` de `FlujoPrincipal_LoginConsultarTareasYSubirEntrega`.
4. Mostrar el código del test en `Tests.UI.OxfordSchool/FlujoPrincipalE2ETests.cs`.
5. Mostrar el repositorio actualizado con el proyecto `Tests.UI.OxfordSchool`.
