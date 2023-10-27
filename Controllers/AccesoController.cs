using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login.Data;
using Login.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Login.Controllers
{
    public class AccesoController : Controller
    {
        private readonly LoginContext _context;

        public AccesoController(LoginContext context)
        {
            _context = context;
        }
        static string cadena = "Data Source=(local);Initial Catalog=DB_ACCESO;Integrated Security=true";
        // GET: Acceso
        public ActionResult Login()
        {
            return View();
        }


        public ActionResult Registrar()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Registrar(Usuario oUsuario)
        {
            bool registrado;
            string mensaje;

            if (oUsuario.Clave == oUsuario.ConfirmarClave)
            {

                oUsuario.Clave = ConvertirSha256(oUsuario.Clave);
            }
            else
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            using (SqlConnection cn = new SqlConnection(cadena))
            {

                SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", cn);
                cmd.Parameters.AddWithValue("Correo", oUsuario.Correo);
                cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                cmd.ExecuteNonQuery();

                registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
                mensaje = cmd.Parameters["Mensaje"].Value.ToString();


            }

            ViewData["Mensaje"] = mensaje;

            if (registrado)
            {
                return RedirectToAction("Login", "Acceso");
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult Login(Usuario oUsuario)
        {
            oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

            using (SqlConnection cn = new SqlConnection(cadena))
            {

                SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", cn);
                cmd.Parameters.AddWithValue("Correo", oUsuario.Correo);
                cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                oUsuario.UsuarioID = Convert.ToInt32(cmd.ExecuteScalar().ToString());

            }

            if (oUsuario.UsuarioID != 0)
            {
                _context.Add(oUsuario);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }
        }



        public static string ConvertirSha256(string texto)
        {
            //using System.Text;
            //USAR LA REFERENCIA DE "System.Security.Cryptography"

            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }

}


/* private readonly LoginContext _context;

       public UsuariosController(LoginContext context)
       {
           _context = context;
       }

       // GET: Usuarios
       public async Task<IActionResult> Index()
       {
             return _context.Usuario != null ? 
                         View(await _context.Usuario.ToListAsync()) :
                         Problem("Entity set 'LoginContext.Usuario'  is null.");
       }

       // GET: Usuarios/Details/5
       public async Task<IActionResult> Details(int? id)
       {
           if (id == null || _context.Usuario == null)
           {
               return NotFound();
           }

           var usuario = await _context.Usuario
               .FirstOrDefaultAsync(m => m.UsuarioID == id);
           if (usuario == null)
           {
               return NotFound();
           }

           return View(usuario);
       }

       // GET: Usuarios/Create
       public IActionResult Create()
       {
           return View();
       }

       // POST: Usuarios/Create
       // To protect from overposting attacks, enable the specific properties you want to bind to.
       // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Create([Bind("UsuarioID,Correo,Clave,ConfirmarClave")] Usuario usuario)
       {
           if (ModelState.IsValid)
           {
               _context.Add(usuario);
               await _context.SaveChangesAsync();
               return RedirectToAction(nameof(Index));
           }
           return View(usuario);
       }

       // GET: Usuarios/Edit/5
       public async Task<IActionResult> Edit(int? id)
       {
           if (id == null || _context.Usuario == null)
           {
               return NotFound();
           }

           var usuario = await _context.Usuario.FindAsync(id);
           if (usuario == null)
           {
               return NotFound();
           }
           return View(usuario);
       }

       // POST: Usuarios/Edit/5
       // To protect from overposting attacks, enable the specific properties you want to bind to.
       // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Edit(int id, [Bind("UsuarioID,Correo,Clave,ConfirmarClave")] Usuario usuario)
       {
           if (id != usuario.UsuarioID)
           {
               return NotFound();
           }

           if (ModelState.IsValid)
           {
               try
               {
                   _context.Update(usuario);
                   await _context.SaveChangesAsync();
               }
               catch (DbUpdateConcurrencyException)
               {
                   if (!UsuarioExists(usuario.UsuarioID))
                   {
                       return NotFound();
                   }
                   else
                   {
                       throw;
                   }
               }
               return RedirectToAction(nameof(Index));
           }
           return View(usuario);
       }

       // GET: Usuarios/Delete/5
       public async Task<IActionResult> Delete(int? id)
       {
           if (id == null || _context.Usuario == null)
           {
               return NotFound();
           }

           var usuario = await _context.Usuario
               .FirstOrDefaultAsync(m => m.UsuarioID == id);
           if (usuario == null)
           {
               return NotFound();
           }

           return View(usuario);
       }

       // POST: Usuarios/Delete/5
       [HttpPost, ActionName("Delete")]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> DeleteConfirmed(int id)
       {
           if (_context.Usuario == null)
           {
               return Problem("Entity set 'LoginContext.Usuario'  is null.");
           }
           var usuario = await _context.Usuario.FindAsync(id);
           if (usuario != null)
           {
               _context.Usuario.Remove(usuario);
           }

           await _context.SaveChangesAsync();
           return RedirectToAction(nameof(Index));
       }

       private bool UsuarioExists(int id)
       {
         return (_context.Usuario?.Any(e => e.UsuarioID == id)).GetValueOrDefault();
       }*/