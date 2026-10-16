using FactioX.Models;
using FactioX.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace FactioX.Components.Pages.Tpv;

public abstract class TpvPaginaBase : ComponentBase
{
    protected const string ClaveTerminal = "factiox.tpv.terminal";

    [Inject] protected IAuthService AuthService { get; set; } = null!;
    [Inject] protected ITpvService TpvService { get; set; } = null!;
    [Inject] protected ITicketPdfService TicketPdfService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected IJSRuntime JS { get; set; } = null!;

    protected bool Cargado { get; private set; }
    protected string? ErrorCarga { get; set; }
    protected virtual bool RequiereAdministrador => false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await AuthService.InicializarAsync();
        if (!AuthService.EstaAutenticado)
        {
            Navigation.NavigateTo("/login", forceLoad: true);
            return;
        }
        if (!await TpvService.PuedeUsarTpvAsync())
        {
            Navigation.NavigateTo("/modulo-no-disponible/TPV");
            return;
        }
        if (RequiereAdministrador && !TpvService.EsAdministrador)
        {
            Navigation.NavigateTo("/tpv");
            return;
        }

        try
        {
            await CargarAsync();
        }
        catch (Exception ex)
        {
            ErrorCarga = ex.Message;
        }
        Cargado = true;
        StateHasChanged();
        await DespuesDeCargarAsync();
    }

    protected abstract Task CargarAsync();

    protected virtual Task DespuesDeCargarAsync() => Task.CompletedTask;

    protected async Task<bool> EjecutarAsync(Func<Task> accion)
    {
        try
        {
            await accion();
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException or DbUpdateException)
        {
            var mensaje = ex is DbUpdateException ? "No se pudo guardar. Revise los datos e inténtelo de nuevo." : ex.Message;
            await JS.InvokeVoidAsync("alert", mensaje);
            return false;
        }
    }

    protected async Task<int?> LeerTerminalGuardadoAsync()
    {
        var valor = await JS.InvokeAsync<string?>("tpv.obtener", ClaveTerminal);
        return int.TryParse(valor, out var id) ? id : null;
    }

    protected Task GuardarTerminalAsync(int terminalId) =>
        JS.InvokeVoidAsync("tpv.guardar", ClaveTerminal, terminalId.ToString()).AsTask();

    protected async Task ImprimirPdfAsync(byte[] pdf)
    {
        using var stream = new MemoryStream(pdf);
        using var referencia = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("printPdfFromStream", referencia);
    }

    protected async Task ImprimirTicketAsync(int ticketId, int anchoMm)
    {
        var ticket = await TpvService.ObtenerTicketAsync(ticketId)
            ?? throw new InvalidOperationException("Ticket no encontrado.");
        var config = await TpvService.ObtenerConfiguracionAsync();
        await ImprimirPdfAsync(TicketPdfService.GenerarTicketPdf(ticket, config, ticket.Terminal?.ImpresoraAnchoMm ?? anchoMm));
    }

    protected static string NombreCliente(Cliente? cliente) => cliente == null
        ? "Cliente de contado"
        : cliente.Tipo == TipoCliente.Empresa && !string.IsNullOrWhiteSpace(cliente.NombreEmpresa)
            ? cliente.NombreEmpresa!
            : $"{cliente.Nombre} {cliente.Apellidos}".Trim();

    protected static decimal Pvp(LineaDocumento linea) =>
        Math.Round(linea.PrecioUnitario * (1 + linea.IVA / 100m), 2, MidpointRounding.AwayFromZero);

    protected static decimal Pvp(Producto producto) =>
        Math.Round(producto.PrecioUnitario * (1 + producto.IVA / 100m), 2, MidpointRounding.AwayFromZero);

    protected static decimal ImporteLinea(LineaDocumento linea) =>
        Math.Round(linea.Cantidad * linea.PrecioUnitario * (1 - linea.Descuento / 100m) * (1 + linea.IVA / 100m), 2, MidpointRounding.AwayFromZero);

    protected static string ClaseEstado(EstadoTicketTpv estado) => estado switch
    {
        EstadoTicketTpv.Cobrado => "bg-success",
        EstadoTicketTpv.Facturado => "bg-primary",
        EstadoTicketTpv.Aparcado => "bg-warning text-dark",
        EstadoTicketTpv.Anulado => "bg-secondary",
        _ => "bg-light text-dark"
    };
}
