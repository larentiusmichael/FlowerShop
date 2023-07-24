// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FlowerShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlowerShop.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<FlowerShopUser> _userManager;
        private readonly SignInManager<FlowerShopUser> _signInManager;

        public IndexModel(
            UserManager<FlowerShopUser> userManager,
            SignInManager<FlowerShopUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(20, ErrorMessage = "3 - 20 length size")]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }

            [Required]
            [Range(18, 100, ErrorMessage = "This website only open for 18 years old above")]
            [Display(Name = "Customer Age")]
            public int CustomerAge { get; set; }

            [Required]
            [Display(Name = "Customer Address")]
            public string CustomerAddress { get; set; }

            [Required]
            [Display(Name = "Customer Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime CustomerDOB { get; set; }
          
        }

        private async Task LoadAsync(FlowerShopUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel  //load the data from the table to the form
            {
                PhoneNumber = phoneNumber,
                CustomerName = user.CustomerName,
                CustomerAge = user.CustomerAge,
                CustomerAddress = user.CustomerAddress,
                CustomerDOB = user.CustomerDOB
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if(Input.CustomerName != user.CustomerName)
            {
                user.CustomerName = Input.CustomerName;
            }
            if (Input.CustomerAddress != user.CustomerAddress)
            {
                user.CustomerAddress = Input.CustomerAddress;
            }
            if (Input.CustomerAge != user.CustomerAge)
            {
                user.CustomerAge = Input.CustomerAge;
            }
            if (Input.CustomerDOB != user.CustomerDOB)
            {
                user.CustomerDOB = Input.CustomerDOB;
            }

            await _userManager.UpdateAsync(user);   //update command

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
