using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChachaWilka.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        public LoginController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Registro()
        {
            ViewBag.Roles = _context.roles.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(UsuarioVM usuarioVM)
        {
            ViewBag.Roles = _context.roles.ToList();
            if (usuarioVM.Contraseña != usuarioVM.RepiteContraseña)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }
            Usuario usuarios = new Usuario()
            {
                Nombre = usuarioVM.Nombre,
                Correo = usuarioVM.Correo,
                Contraseña = usuarioVM.Contraseña,
                Id_Rol = usuarioVM.Id_Rol
            };
            await _context.usuarios.AddAsync(usuarios);
            await _context.SaveChangesAsync();
            if (usuarios.Id_Usuario != 0)
            {
                return RedirectToAction("Login", "Usuario");
            }
            ViewData["Mensaje"] = "El usuario no se pudo crear";
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            Usuario? usuarios = await _context.usuarios.Include(r => r.Rol).FirstOrDefaultAsync(c => c.Correo == loginVM.Correo && c.Contraseña == loginVM.Contraseña);
            if (usuarios == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View();
            }
            //CLAIMS
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarios.Id_Usuario.ToString()),
                new Claim(ClaimTypes.Name, usuarios.Nombre),
                new Claim(ClaimTypes.Email, usuarios.Correo),
                new Claim(ClaimTypes.Role, usuarios.Rol.Nombre_Rol)
            };
            //Identidadd
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //crear usuario autenticado
            var principal = new ClaimsPrincipal(identidad);
            //COOKIES
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            //SESIONES
            HttpContext.Session.SetInt32("IdUsuario", usuarios.Id_Usuario);
            HttpContext.Session.SetString("NombreUsuario", usuarios.Nombre);
            HttpContext.Session.SetString("CorreoUsuario", usuarios.Correo);
            HttpContext.Session.SetString("RolUsuario", usuarios.Rol.Nombre_Rol);
            //Redirección
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Usuario");
        }
    }
}
