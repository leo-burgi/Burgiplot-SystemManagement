using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class MaterialesController : Controller
{
    private readonly BurgiplotContext _context;

    public MaterialesController(BurgiplotContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var materiales = await _context.Materials
            .OrderBy(m => m.Descripcion)
            .ToListAsync();

        return View(materiales);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (material == null)
        {
            return NotFound();
        }

        return View(material);
    }

    public IActionResult Create()
    {
        return View(new Material());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Codigo,Descripcion,Unidad,PrecioStd,StockActual")] Material material)
    {
        ValidarMaterial(material);

        if (!ModelState.IsValid)
        {
            return View(material);
        }

        material.CreatedAtUtc = DateTime.UtcNow;
        material.UpdatedAtUtc = DateTime.UtcNow;

        _context.Add(material);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        return View(material);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Descripcion,Unidad,PrecioStd,StockActual")] Material material)
    {
        if (id != material.Id)
        {
            return NotFound();
        }

        ValidarMaterial(material);

        if (!ModelState.IsValid)
        {
            return View(material);
        }

        var materialDb = await _context.Materials.FindAsync(id);
        if (materialDb == null)
        {
            return NotFound();
        }

        materialDb.Codigo = material.Codigo.Trim();
        materialDb.Descripcion = material.Descripcion.Trim();
        materialDb.Unidad = material.Unidad.Trim();
        materialDb.PrecioStd = material.PrecioStd;
        materialDb.StockActual = material.StockActual;
        materialDb.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (material == null)
        {
            return NotFound();
        }

        return View(material);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var material = await _context.Materials
            .Include(m => m.OrdenItems)
            .Include(m => m.FacturaItems)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (material.OrdenItems.Any() || material.FacturaItems.Any())
        {
            TempData["MaterialError"] = "No se puede eliminar un material que ya tiene movimientos en órdenes o facturas.";
            return RedirectToAction(nameof(Index));
        }

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidarMaterial(Material material)
    {
        if (string.IsNullOrWhiteSpace(material.Codigo))
        {
            ModelState.AddModelError(nameof(Material.Codigo), "El código es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(material.Descripcion))
        {
            ModelState.AddModelError(nameof(Material.Descripcion), "La descripción es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(material.Unidad))
        {
            ModelState.AddModelError(nameof(Material.Unidad), "La unidad es obligatoria (ejemplo: m2, rollo, unidad).");
        }

        if (material.StockActual < 0)
        {
            ModelState.AddModelError(nameof(Material.StockActual), "El stock no puede ser negativo.");
        }

        if (material.PrecioStd is < 0)
        {
            ModelState.AddModelError(nameof(Material.PrecioStd), "El precio estándar no puede ser negativo.");
        }

        material.Codigo = material.Codigo?.Trim() ?? string.Empty;
        material.Descripcion = material.Descripcion?.Trim() ?? string.Empty;
        material.Unidad = material.Unidad?.Trim() ?? string.Empty;
    }
}
