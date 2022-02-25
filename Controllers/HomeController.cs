using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public MainDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        public string ReturnUrl { get; set; }
        public string ProviderDisplayName { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string StaticMessage { get; set; }
        public HomeController(
            ILogger<HomeController> logger, 
            MainDbContext context,
            SignInManager<User> signInManager, 
            UserManager<User> userManager,
            IUserStore<User> userStore)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public class InputModelLogin
        {
            [Required(ErrorMessage = "Bạn chưa nhập email")]
            [EmailAddress(ErrorMessage = "Địa chỉ email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Bạn chưa nhập mật khẩu")]
            [StringLength(50, MinimumLength = 5, ErrorMessage = "Chiều dài không hợp lệ")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewData["login"] = ExternalLogins;

            return View();
        }

        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> LoginAsync(InputModelLogin InputLogin,string? returnUrl = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewData["login"] = ExternalLogins;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(InputLogin.Email,
                           InputLogin.Password, InputLogin.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["success"] = $"Đăng nhập thành công";
                    if(returnUrl != null)
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["danger"] = $"Error : Tài khoản hoặc mật khẩu không chính xác";
                }
            }
            else Console.WriteLine("---------------------------------");

            return View();
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Bạn chưa nhập tên")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Bạn chưa nhập email")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Bạn chưa nhập mật khẩu")]
            [StringLength(50, MinimumLength = 5, ErrorMessage = "Chiều dài không hợp lệ")]
            public string Password { get; set; }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new InputModel());
        }

        [HttpPost,ActionName("Register")]
        public async Task<IActionResult> RegisterAsync(InputModel Input)
        {
            if (ModelState.IsValid)
            {
                var check = _context.Users.Where(u => u.Email == Request.Form["Email"].ToString()).FirstOrDefault();
                if (check == null)
                {
                    var user = new User {UserName = Input.Email, Email = Input.Email, Name = Input.Name};
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        TempData["success"] = $"Đăng kí tài khoản thành công {user.UserName}";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["success"] = $"Tài khoản chưa được tạo";
                    }
                }
                else
                {
                    TempData["success"] = $"Tài khoản đã tồn tại";
                }
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success"] = $"Đăng xuất thành công";

            return RedirectToAction("Index");
        }
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }

        public IActionResult DenyAccess()
        {
            TempData["message"] = $"Bạn không có quyền truy cập vào trang này";

            return View();
        }

        public class InputModel2
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        [BindProperty]
        public InputModel2 Input { get; set; }

        [HttpPost]
        public IActionResult LoginAuthen(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            // var redirectUrl = Url.Page(".", pageHandler: "Callback", values: new { returnUrl });
            var redirectUrl = Url.Action("Callback", "Home", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                TempData["message"] = $"Error from external provider: {remoteError}";
                return RedirectToAction("Login","Home", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["message"] = "Error loading external login information.";
                return RedirectToAction("Login", "Home", new { ReturnUrl = returnUrl });
            }
            Console.WriteLine("1111111111111111111111111111111111111");
            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                Console.WriteLine("222222222222222222222222222222222222");
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                TempData["message"] = "Login success";
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                TempData["message"] = "Is Locked";
                return RedirectToAction("Index","Home");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel2
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var resultLogin = await _userManager.CreateAsync(user);
                if (resultLogin.Succeeded)
                {
                    resultLogin = await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    TempData["message"] = "Success";
                }
                else TempData["message"] = "Error : ...";
                return RedirectToAction("Index", "Home");
            }
        }
        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}