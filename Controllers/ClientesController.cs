using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class ClientesController : Controller
    {
        // -----------ANTES COMUNICACIÓN DIRECTA CON LA BASE DE DATOS A TRAVÉS DEL CONTEXTO DE EF CORE
        /*private readonly BurgiplotContext _context;
        public ClientesController(BurgiplotContext context)
        {
            _context = context;
        }*/

        // AHORA COMUNICACIÓN A TRAVÉS DEL SERVICIO
        private readonly IClienteService _clienteService;
        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        //------------------------------------INDEX--------------------------------------
        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // ---------------------ANTES CONSULTA DIRECTA

            /*var query = _context.Clientes.AsQueryable().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();

                query = query.Where(c =>
                    EF.Functions.Like(c.Nombre, $"%{s}%") ||
                    EF.Functions.Like(c.Apellido, $"%{s}%") ||
                    (c.Dirección!= null && EF.Functions.Like(c.Dirección, $"%{s}%"))
                );
            }

            var clientes = await query.OrderBy(c => c.Apellido)
                .ThenBy(c=>c.Nombre).ToListAsync();*/

            // AHORA USO EL SERVICIO
            var clientes = await _clienteService.GetAllClientesAsync(searchString);
            return View(clientes);
        }

        //----------------------------------DETALLES-------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            //var cliente = await _context.Clientes.FirstOrDefaultAsync(m => m.Id == id);
            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        //------------------------------------CREAR--------------------------------------
        // GET(MOSTRAR FORMULARIO)
        public IActionResult Create()
        {
            return View();
        }

        // POST(VALIDAR Y RECIBIR DATOS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (!ModelState.IsValid) return View(cliente);
            
            //------ ANTES : verificacion de unicidad de DNI
            /* if (await _context.Clientes.AnyAsync(c => c.DNI == cliente.DNI))
            {
                ModelState.AddModelError(nameof(Cliente.DNI), "Ya existe un cliente con este DNI.");
                return View(cliente);
            }
           
            try
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)*/
            try
            {
                await _clienteService.CreateClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) when (ex.Message== "Ya existe un cliente con este DNI.")
            {
                ModelState.AddModelError(nameof(Cliente.DNI), "Ya existe un cliente con este DNI.");
                return View(cliente);
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {

                    case 50011: // Error de trigger personalizado
                        ModelState.AddModelError(nameof(Cliente.DNI), "DNI incorrecto; debe tener 7 u 8 digitos.");
                        break;
                    case 50010:
                        ModelState.AddModelError(nameof(Cliente.CUIT_CUIL), "CUIT/CUIL incorrecto; debe tener 11 digitos");
                        break;
                    case 2627: // Violación de restricción de clave única
                    case 2601: // unique
                        var m = sqlEx.Message;
                        if (m.Contains("UX_Cliente_DNI"))
                            ModelState.AddModelError(nameof(Cliente.DNI), "Ya existe un cliente con este DNI.");

                        else if (m.Contains("UX_Cliente_CUIT_CUIL"))
                            ModelState.AddModelError(nameof(Cliente.CUIT_CUIL), "Ya existe un cliente con este CUIT/CUIL.");
                        else
                            ModelState.AddModelError(string.Empty, "Registro duplicado (índice único).");
                        break;

                    default:
                        ModelState.AddModelError(string.Empty, $"Error de base de datos ({sqlEx.Number}).Detalle: {sqlEx.Message}");
                        break;
                }
                return View(cliente);
            }
        }

        //--------------------------------------EDITAR--------------------------------------
        // GET(MOSTRAR FORMULARIO CON DATOS DEL CLIENTE)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // POST(VALIDAR Y ACTUALIZAR DATOS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();
            if (!ModelState.IsValid) return View(cliente);
            
            try
            {
                // _context.Update(cliente);
                //await _context.SaveChangesAsync();
                await _clienteService.UpdateClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) when (ex.Message == "Ya existe un cliente con este DNI.")
            {
                ModelState.AddModelError(nameof(Cliente.DNI), "Ya existe un cliente con este DNI.");
                return View(cliente);
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!_clienteService.Clientes.Any(e => e.Id == id))
                    return NotFound();
                //throw;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 50011: // Error de trigger personalizado
                        ModelState.AddModelError(nameof(Cliente.DNI), "DNI incorrecto; debe tener 7 u 8 digitos.");
                        break;
                    case 50010:
                        ModelState.AddModelError(nameof(Cliente.CUIT_CUIL), "CUIT/CUIL incorrecto; debe tener 11 digitos");
                        break;
                    case 2627: // Violación de restricción de clave única
                    case 2601: // Violación de restricción de clave única (índice único)
                        ModelState.AddModelError(nameof(Cliente.CUIT_CUIL), "El CUIT/CUIL ya existe en la base de datos.");
                        break;
                    
                    default:
                        ModelState.AddModelError(string.Empty, $"Error de base de datos ({sqlEx.Number}).Detalle: {sqlEx.Message}");
                        break;
                }
                return View(cliente);
            }
            
        }

        //-------------------------------------ELIMINAR--------------------------------------
        // GET(MOSTRAR FORMULARIO CON DATOS)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _clienteService.GetClienteByIdAsync(id.Value);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // POST(VALIDAR Y ELIMINAR DATOS)(coincidir nombres sino renombrar #ojo scaffold)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // -----------ANTES ELIMINACIÓN DIRECTA
            /*var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }*/

            // AHORA A TRAVÉS DEL SERVICIO
            await _clienteService.DeleteClienteAsync(id);

            return RedirectToAction(nameof(Index));
        }

    }
}
