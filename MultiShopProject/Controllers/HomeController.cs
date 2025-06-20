using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShopProject.Context;
using MultiShopProject.ViewModels.Home;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using Bigon.WebUI.Models.Entities;

namespace MultiShopProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext context;
        private readonly IConfiguration configuration;

        public HomeController(DataContext _context,IConfiguration configuration)
        {
            context = _context;
            this.configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {
                CarouselItems = await context.CarouselItems.ToListAsync(),
                CardItems = await context.CardItems.ToListAsync(),
                Sponsors = await context.Sponsors.ToListAsync(),
                Categories = await context.Categories
                              .Include(m => m.Products)
                              .ToListAsync(),

                Products = await context.Products
                            .Include(m => m.Category)
                            .ToListAsync()
            };
            return View(homeVM);
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            bool isEmail = Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            if (!isEmail)
            {
                return Json(new
                {
                    error = true,
                    message = $"'{email}'email telebelerini odemir!"
                });
            }

            var subscriber = await context.Subscribers.FirstOrDefaultAsync(m => m.Email.Equals(email));

            if (subscriber != null && subscriber.Approved)
            {
                return Json(new
                {
                    error = true,
                    message = $"'{email}'bu e-poct adresine artiq abunelik tetbiq edilib!"
                });
            }
            else if (subscriber != null && !subscriber.Approved)
            {
                return Json(new
                {
                    error = false,
                    message = $"'{email}'bu e-poct adresini tesdiqlemesiniz!"
                });
            }
            subscriber = new Subscriber();
            subscriber.Email = email;
            subscriber.CreatedAt = DateTime.Now;
            await context.Subscribers.AddAsync(subscriber);
            await context.SaveChangesAsync();
            string displayName = configuration["emailAccount:displayName"];
            string host = configuration["emailAccount:smtpServer"];
            int smtpPort = Convert.ToInt32(configuration["emailAccount:smtpPort"]);
            string userName = configuration["emailAccount:userName"];
            string password = configuration["emailAccount:password"];


            using (SmtpClient client = new SmtpClient(host, smtpPort))
            using (MailMessage message = new MailMessage())
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(userName, password);

                message.Subject = "Bigon Service";
                message.To.Add(subscriber.Email);
                message.IsBodyHtml = true;
                message.From = new MailAddress(userName, displayName);
                string url = "https://localhost:7219/home/subscribe-approve?token=blablabla"; ;
                message.Body = $"Abuneliyinizi tesdiq etmek ucun <a href=\"{url}\">linklə</a> davam edin!";
                await client.SendMailAsync(message);
            }
            return Json(new
            {
                error = false,
                message = $"Abuneliyinizi tesdiq etmek ucun '{email}'bu e-poct adresine daxil olub size gonderilen linke kecid edin!"
            });
        }
        [HttpPost]
        public IActionResult SubscribeApprove(string email)
        {
            return Content("deyekki abune oldunuz");
        }

    }
    }

