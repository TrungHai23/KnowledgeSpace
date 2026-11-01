using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeSpace.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(RoleManager<IdentityRole> rolesManager)
        {
            _roleManager = rolesManager;
        }

        // URL: POST: http://localhost5001/api/roles
        [HttpPost]

        public async Task<IActionResult> PostRole(RoleVM roleVm)
        {
            var role = new IdentityRole()
            {
                Id = roleVm.Id,
                Name = roleVm.Name,
                NormalizedName = roleVm.Name.ToUpper()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleVm);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        // URL: GETALL: http://localhost5001/api/roles
        [HttpGet]
        public async Task<IActionResult> GetRoles(string id)
        {
            var roles = await _roleManager.Roles.Select(r => new RoleVM()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();
            
            return Ok(roles);
        }

        // URL: GETALL: http://localhost5001/api/roles/?filter={filter}&pageIndex={pageIndex}&pageSize={pageSize}
        [HttpGet]
        public async Task<IActionResult> GetRoles(string filter, int pageIndex, int pageSize)
        {
            var query = _roleManager.Roles; 
            if(!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
            }

            var totalRecords = await query.CountAsync();
            var item = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(r => new RoleVM()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();
            var pagination = new Pagination<RoleVM>
            {
               Items =  item,
               TotalRecords = totalRecords
            };

            return Ok(pagination);
        }

        // URL: GET: http://localhost5001/api/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var roleVm = new RoleVM()
            {
                Id = role.Id,
                Name = role.Name
            };
            return Ok(roleVm);
        }

        // URL: PUT: http://localhost5001/api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, [FromBody] RoleVM roleVm)
        {
            if (id != roleVm.Id)
            {
                return BadRequest();
            }
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        // URL: DELETE: http://localhost5001/api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                var rolevm = new RoleVM()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}
