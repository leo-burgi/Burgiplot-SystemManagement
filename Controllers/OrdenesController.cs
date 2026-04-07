using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class OrdenesController : Controller
{
    private static readonly string[] EstadosPermitidos =
    [
        "Borrador",
        "En producción",
        "Pausada",
        "Finalizada",
        "Entregada"
    ];

    private readonly BurgiplotContext _context;

    public OrdenesController(BurgiplotContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ordenes = await _context.Ordens
            .Include(o => o.Cliente)
            .OrderByDescending(o => o.FechaUtc)
            .ToListAsync();

        return View(ordenes);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var orden = await _context.Ordens
            .Include(o => o.Cliente)
            .Include(o => o.OrdenItems)
                .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (orden == null)
        {
            return NotFound();
        }

        return View(orden);
    }

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Orden
        {
            FechaUtc = DateTime.UtcNow,
            Estado = "Borrador"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClienteId,FechaUtc,Estado,Observaciones")] Orden orden)
    {
        if (!EstadosPermitidos.Contains(orden.Estado))
        {
            ModelState.AddModelError(nameof(Orden.Estado), "Seleccione un estado válido para la orden.");
        }

        if (!ModelState.IsValid)
        {
            CargarCombos(orden.ClienteId, orden.Estado);
            return View(orden);
        }

        orden.CreatedAtUtc = DateTime.UtcNow;
        orden.UpdatedAtUtc = DateTime.UtcNow;
        orden.Total = 0m;

        _context.Add(orden);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = orden.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var orden = await _context.Ordens.FindAsync(id);
        if (orden == null)
        {
            return NotFound();
        }

        CargarCombos(orden.ClienteId, orden.Estado);
        return View(orden);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,FechaUtc,Estado,Observaciones")] Orden orden)
    {
        if (id != orden.Id)
        {
            return NotFound();
        }

        if (!EstadosPermitidos.Contains(orden.Estado))
        {
            ModelState.AddModelError(nameof(Orden.Estado), "Seleccione un estado válido para la orden.");
        }

        if (!ModelState.IsValid)
        {
            CargarCombos(orden.ClienteId, orden.Estado);
            return View(orden);
        }

        var ordenDb = await _context.Ordens.FindAsync(id);
        if (ordenDb == null)
        {
            return NotFound();
        }

        ordenDb.ClienteId = orden.ClienteId;
        ordenDb.FechaUtc = orden.FechaUtc;
        ordenDb.Estado = orden.Estado;
        ordenDb.Observaciones = orden.Observaciones;
        ordenDb.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var orden = await _context.Ordens
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (orden == null)
        {
            return NotFound();
        }

        return View(orden);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var orden = await _context.Ordens
            .Include(o => o.OrdenItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (orden.OrdenItems.Any())
        {
            TempData["OrdenError"] = "No se puede eliminar una orden con consumos de stock cargados. Primero elimine sus ítems.";
            return RedirectToAction(nameof(Index));
        }

        _context.Ordens.Remove(orden);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void CargarCombos(int? clienteId = null, string? estado = null)
    {
        ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nombre), "Id", "Nombre", clienteId);
        ViewData["Estado"] = new SelectList(EstadosPermitidos, estado);
    }
}
