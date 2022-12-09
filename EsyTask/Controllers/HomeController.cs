using EsyTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EsyTask.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;

        private readonly ILogger<HomeController> _logger;

        public static ApplicationDbContext dbContext =
            new(new DbContextOptions<ApplicationDbContext>());
        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager)
        {

            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (dbContext.Products != null)
                ViewData["Products"] = dbContext.Products.ToList();
            else
                ViewData["Products"] = new List<Product>();
            return View();
        }

        public IActionResult AddView()
        {
            return View("Add");
        }

        public IActionResult Add(Product product, string userName)
        {
            product.Id = Guid.NewGuid().ToString();
            dbContext.Products.Add(product);
            ChangeInfo changeInfo = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                Info = "Create"
            };
            var changes = new Changes()
            {
                OldValue = "Null",
                NewValue = JsonSerializer.Serialize(product)
            };

            changeInfo.Changes.Add(changes);
            dbContext.ChangeInfo.Add(changeInfo);
            dbContext.SaveChanges();
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(model.UserName);
                if (user.Status == TaskStatus.RanToCompletion)
                {
                    await _signInManager.PasswordSignInAsync(user.Result, model.Password, false, true);
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            var need = _signInManager.IsSignedIn(User);
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    UserName = model.Name,
                    Password = model.Password
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Task.WaitAll(_signInManager.SignInAsync(user, false));
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        public IActionResult Delete(Product product, string userName)
        {
            var deleteProduct = dbContext.Products.First(x => x.Id == product.Id);
            dbContext.Products.Remove(deleteProduct);
            ChangeInfo changeInfo = new()
            {
                UserName = userName,
                Info = "Delete"
            };
            var changes = new Changes()
            {
                FieldName = "Creating object",
                OldValue = JsonSerializer.Serialize(product),
                NewValue = "Null"
            };

            changeInfo.Changes.Add(changes);
            dbContext.ChangeInfo.Add(changeInfo);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult EditView(Product product)
        {
            return View("Edit", product);
        }

        public IActionResult Edit(Product product, string replaceId, string username)
        {
            var replaceProduct = dbContext.Products.First(x => x.Id == replaceId);
            var changeInfo = new ChangeInfo();
            if (replaceProduct.Name != product.Name)
            {
                replaceProduct.Name = product.Name;
                var changes = new Changes()
                {
                    FieldName = "Name",
                    OldValue = replaceProduct.Name,
                    NewValue = product.Name
                };
                changeInfo.UserName = username;
                changeInfo.Changes.Add(changes);
            }

            if (replaceProduct.Quantity != product.Quantity)
            {
                replaceProduct.Name = product.Name;
                var changes = new Changes()
                {
                    FieldName = "Name",
                    OldValue = replaceProduct.Name,
                    NewValue = product.Name
                };
                changeInfo.Changes.Add(changes);
            }

            if (replaceProduct.Price != product.Price)
            {
                replaceProduct.Name = product.Name;
                var changes = new Changes()
                {
                    FieldName = "Name",
                    OldValue = replaceProduct.Name,
                    NewValue = product.Name
                };
                changeInfo.Changes.Add(changes);
            }

            replaceProduct.Quantity = product.Quantity;
            replaceProduct.Price = product.Price;
            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
