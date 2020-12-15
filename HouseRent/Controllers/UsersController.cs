using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HouseRent.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using HouseRent.Services;
using Microsoft.Extensions.Configuration;

namespace HouseRent.Controllers
{
    public class UsersController : Controller
    {
        private const string userString = "ID,Name,Contact,Email,Password,Address,Role,Avatar";
        private readonly HouseRentContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public UsersController(HouseRentContext context, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        public FileContentResult GetImg(int id)
        {
            var image = _context.User.Find(id).Avatar;

            return image != null ? new FileContentResult(image, "image/png") : null;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            if(HttpContext.Session.GetString("sRole") != "admin")
            {
                return RedirectToAction("Index", "Home");
            } 
            return View(await _context.User.ToListAsync());
        }       


        public IActionResult Login()
        {
            if(!String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind(userString)] User user)
        {
            var usr = await _context.User
                       .SingleOrDefaultAsync(u => u.Email.ToUpper() == user.Email.ToUpper()
                       && u.Password == user.Password);
            if (usr == null)
            {
                HttpContext.Session.SetString("loginFailed", "Email & Password didn't matched!");
                return View();
            }
            HttpContext.Session.SetString("sName", usr.Name);
            HttpContext.Session.SetString("sEmail", usr.Email);
            HttpContext.Session.SetString("sRole", usr.Role);
            HttpContext.Session.SetString("sId", usr.ID.ToString());
            HttpContext.Session.Remove("loginFailed");

            return RedirectToAction("Index", "Advertises");

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("sName");
            HttpContext.Session.Remove("sEmail");
            HttpContext.Session.Remove("sRole");
            HttpContext.Session.Remove("sId");
            HttpContext.Session.Remove("loginFailed");
            HttpContext.Session.Remove("userExist");

            return RedirectToAction(nameof(Login));
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .SingleOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                if(user.Email != HttpContext.Session.GetString("sEmail")
                    && HttpContext.Session.GetString("sRole") != "admin")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(userString)] User user, IFormFile img)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(img != null)
                    {
                        if (img.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                img.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                user.Avatar = fileBytes;
                            }
                        }
                    }
                    
                    user.Role = "normal";
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("sName", user.Name);
                    HttpContext.Session.SetString("sEmail", user.Email);
                    HttpContext.Session.SetString("sRole", user.Role);
                    HttpContext.Session.SetString("sId", user.ID.ToString());
                    HttpContext.Session.Remove("userExist");
                    var task = Task.Run(async () =>
                    {
                        using (var es = _emailService.SendEmailAsync(user.Email, "You have been registered in HouseRent.", $"Login: {user.Email}\nPassword: {user.Password}"))
                        {
                            await es;
                        }
                    });
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    HttpContext.Session.SetString("userExist", user.Email+" Already Exist. Go to Login Page.");
                    return View(user);
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.SingleOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                if (user.Email != HttpContext.Session.GetString("sEmail"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(userString)] User user)
        {
            if (id != user.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .SingleOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                if (user.Email != HttpContext.Session.GetString("sEmail")
                    && HttpContext.Session.GetString("sRole") != "admin")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.SingleOrDefaultAsync(m => m.ID == id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            if(HttpContext.Session.GetString("sEmail") == user.Email)
            {
                return RedirectToAction(nameof(Logout));
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.ID == id);
        }
    }
}
