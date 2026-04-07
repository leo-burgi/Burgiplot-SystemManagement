using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OrdenItemsController : Controller
    {
        private readonly BurgiplotContext _context;

        public OrdenItemsController(BurgiplotContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var burgiplotContext = _context.OrdenItems
                .Include(o => o.Material)
                .Include(o => o.Orden)
                .ThenInclude(or => or.Cliente)
                .OrderByDescending(o => o.OrdenId)
                .ThenBy(o => o.Id);

            return View(await burgiplotContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenItem = await _context.OrdenItems
                .Include(o => o.Material)
                .Include(o => o.Orden)
                    .ThenInclude(or => or.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenItem == null)
            {
                return NotFound();
            }

            return View(ordenItem);
        }

        public IActionResult Create()
        {
            CargarCombos();
            return View(new OrdenItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrdenId,MaterialId,Cantidad,PrecioUnit")] OrdenItem ordenItem)
        {
            if (!await ValidarStockDisponible(ordenItem.MaterialId, ordenItem.Cantidad))
            {
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            if (ordenItem.Cantidad <= 0)
            {
                ModelState.AddModelError(nameof(OrdenItem.Cantidad), "La cantidad debe ser mayor a 0.");
            }

            if (ordenItem.PrecioUnit < 0)
            {
                ModelState.AddModelError(nameof(OrdenItem.PrecioUnit), "El precio unitario no puede ser negativo.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            using var trx = await _context.Database.BeginTransactionAsync();

            var material = await _context.Materials.FindAsync(ordenItem.MaterialId);
            if (material == null)
            {
                ModelState.AddModelError(nameof(OrdenItem.MaterialId), "Material no encontrado.");
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            material.StockActual -= (int)Math.Ceiling(ordenItem.Cantidad);

            _context.Add(ordenItem);
            await _context.SaveChangesAsync();
            await RecalcularTotalOrden(ordenItem.OrdenId);

            await trx.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenItem = await _context.OrdenItems.FindAsync(id);
            if (ordenItem == null)
            {
                return NotFound();
            }
            CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
            return View(ordenItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrdenId,MaterialId,Cantidad,PrecioUnit")] OrdenItem ordenItem)
        {
            if (id != ordenItem.Id)
            {
                return NotFound();
            }

            if (ordenItem.Cantidad <= 0)
            {
                ModelState.AddModelError(nameof(OrdenItem.Cantidad), "La cantidad debe ser mayor a 0.");
            }

            if (ordenItem.PrecioUnit < 0)
            {
                ModelState.AddModelError(nameof(OrdenItem.PrecioUnit), "El precio unitario no puede ser negativo.");
            }

            var ordenItemDb = await _context.OrdenItems.AsNoTracking().FirstOrDefaultAsync(oi => oi.Id == id);
            if (ordenItemDb == null)
            {
                return NotFound();
            }

            var stockNecesario = ordenItem.MaterialId == ordenItemDb.MaterialId
                ? ordenItem.Cantidad - ordenItemDb.Cantidad
                : ordenItem.Cantidad;

            if (stockNecesario > 0 && !await ValidarStockDisponible(ordenItem.MaterialId, stockNecesario))
            {
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            using var trx = await _context.Database.BeginTransactionAsync();

            var materialViejo = await _context.Materials.FindAsync(ordenItemDb.MaterialId);
            if (materialViejo != null)
            {
                materialViejo.StockActual += (int)Math.Ceiling(ordenItemDb.Cantidad);
            }

            var materialNuevo = await _context.Materials.FindAsync(ordenItem.MaterialId);
            if (materialNuevo == null)
            {
                ModelState.AddModelError(nameof(OrdenItem.MaterialId), "Material no encontrado.");
                CargarCombos(ordenItem.OrdenId, ordenItem.MaterialId);
                return View(ordenItem);
            }

            materialNuevo.StockActual -= (int)Math.Ceiling(ordenItem.Cantidad);

            try
            {
                _context.Update(ordenItem);
                await _context.SaveChangesAsync();
                await RecalcularTotalOrden(ordenItem.OrdenId);
                if (ordenItemDb.OrdenId != ordenItem.OrdenId)
                {
                    await RecalcularTotalOrden(ordenItemDb.OrdenId);
                }

                await trx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrdenItemExists(ordenItem.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenItem = await _context.OrdenItems
                .Include(o => o.Material)
                .Include(o => o.Orden)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordenItem == null)
            {
                return NotFound();
            }

            return View(ordenItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordenItem = await _context.OrdenItems.FindAsync(id);
            if (ordenItem != null)
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var material = await _context.Materials.FindAsync(ordenItem.MaterialId);
                if (material != null)
                {
                    material.StockActual += (int)Math.Ceiling(ordenItem.Cantidad);
                }

                _context.OrdenItems.Remove(ordenItem);
                await _context.SaveChangesAsync();
                await RecalcularTotalOrden(ordenItem.OrdenId);

                await trx.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrdenItemExists(int id)
        {
            return _context.OrdenItems.Any(e => e.Id == id);
        }

        private void CargarCombos(int? ordenId = null, int? materialId = null)
        {
            var ordenes = _context.Ordens
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.FechaUtc)
                .Select(o => new
                {
                    o.Id,
                    Texto = $"OT-{o.Id} | {o.Cliente.Nombre} | {o.Estado}"
                })
                .ToList();

            var materiales = _context.Materials
                .OrderBy(m => m.Descripcion)
                .Select(m => new
                {
                    m.Id,
                    Texto = $"{m.Codigo} - {m.Descripcion} (Stock: {m.StockActual})"
                })
                .ToList();

            ViewData["OrdenId"] = new SelectList(ordenes, "Id", "Texto", ordenId);
            ViewData["MaterialId"] = new SelectList(materiales, "Id", "Texto", materialId);
        }

        private async Task<bool> ValidarStockDisponible(int materialId, decimal cantidad)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null)
            {
                ModelState.AddModelError(nameof(OrdenItem.MaterialId), "Material no encontrado.");
                return false;
            }

            if (cantidad <= 0)
            {
                return true;
            }

            var salida = (int)Math.Ceiling(cantidad);
            if (material.StockActual < salida)
            {
                ModelState.AddModelError(nameof(OrdenItem.Cantidad),
                    $"Stock insuficiente. Disponible: {material.StockActual} {material.Unidad}. Solicitado: {salida}.");
                return false;
            }

            return true;
        }

        private async Task RecalcularTotalOrden(int ordenId)
        {
            var orden = await _context.Ordens.FindAsync(ordenId);
            if (orden == null)
            {
                return;
            }

            var total = await _context.OrdenItems
                .Where(x => x.OrdenId == ordenId)
                .SumAsync(x => x.Cantidad * x.PrecioUnit);

            orden.Total = Math.Round(total, 2);
            orden.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
