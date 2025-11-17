using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ClienteService : IClienteService
    {
        private readonly BurgiplotContext _context;
        public ClienteService(BurgiplotContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetAllClientesAsync(string? searchString)
        {
            var query = _context.Clientes.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(c =>
                    c.Nombre.StartsWith(s) ||
                    c.Apellido.StartsWith(s) ||
                    (c.Dirección != null && c.Dirección.StartsWith(s))

                );
            }
            return await query.OrderBy(c => c.Apellido)
                              .ThenBy(c => c.Nombre)
                              .ToListAsync();
        }
        public async Task<Cliente?> GetClienteByIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }
        private async Task<bool> DniDuplicadoAsync(string dni, int? excludeId = null)
        {
            var query = _context.Clientes.Where(c => c.DNI == dni);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        public async Task CreateClienteAsync(Cliente cliente)
        {
            if (await DniDuplicadoAsync(cliente.DNI))
            {
                throw new InvalidOperationException("Ya existe un cliente con este DNI.");
            }
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateClienteAsync(Cliente cliente)
        {
            if (await DniDuplicadoAsync(cliente.DNI, cliente.Id))
            {
                throw new InvalidOperationException("Ya existe un cliente con este DNI.");
            }
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteClienteAsync(int id)
        {
            var rowsAffected = await _context.Clientes.Where(c=> c.Id ==id).ExecuteDeleteAsync();
           
        }
    }
}
