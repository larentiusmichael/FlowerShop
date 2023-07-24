using Microsoft.AspNetCore.Mvc;
using FlowerShop.Models;
using Microsoft.EntityFrameworkCore;
using FlowerShop.Data;

namespace FlowerShop.Controllers
{
    public class FlowersController : Controller
    {
        private readonly FlowerShopContext _context;

        //create constructor for linking your db to this file
        public FlowersController(FlowerShopContext context)
        {
            _context = context; //for referring which db you want
        }

        //display
        public async Task<IActionResult> Index(string ? msg, string ? searchString)
        {
            List<Flower> flowerlist = await _context.FlowerTable.ToListAsync();
            if(! string.IsNullOrEmpty(searchString))
            {
                flowerlist = flowerlist.Where(s=> s.FlowerName.Contains(searchString)).ToList();
            }
            ViewBag.msg = msg;
            return View(flowerlist);
        }

        //insert
        public IActionResult InsertForm() //first loading page
        {
            return View();
        }

        //submission page
        [HttpPost]
        [ValidateAntiForgeryToken]  //avoid cross-site attack

        public async Task<IActionResult> InsertForm(Flower flower)
        {
            if(ModelState.IsValid)  //if the form no issue, then add to table
            {
                _context.FlowerTable.Add(flower);
                await _context.SaveChangesAsync();  //commit to add the data
                return RedirectToAction("Index", new {msg = "Insert Successfully!"});
            }
            return View(flower);    //error then keep the current flower info for editing
        }

        //delete
        public async Task<IActionResult> deletepage(int ? FlowerID)
        {
            if (FlowerID == null)
            {
                return NotFound();
            }

            var flower = await _context.FlowerTable.FindAsync(FlowerID);

            if (flower == null)
            {
                return NotFound();
            }

            try
            {
                _context.FlowerTable.Remove(flower);

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { msg = "Flower with ID " + FlowerID + " is deleted!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new { msg = "Flower with ID " + FlowerID + " unable delete! Error: " + ex.Message });
            }
        }

        //edit form - show the current data
        public async Task<IActionResult> editpage (int ? FlowerID)
        {
            if (FlowerID == null)
            {
                return NotFound();
            }

            var flower = await _context.FlowerTable.FindAsync(FlowerID);

            if (flower == null)
            {
                return NotFound();
            }

            return View(flower);
        }

        //edit action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> editflower(Flower flower)
        {
            if(ModelState.IsValid)
            {
                _context.FlowerTable.Update(flower);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { msg = "Update Successfully!" });
            }

            return View("editpage", flower);
        }
    }
}
