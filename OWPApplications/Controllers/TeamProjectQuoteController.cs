using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;

namespace OWPApplications.Controllers
{
    public class TeamProjectQuoteController : Controller
    {
        private readonly DbHandler _db;

        public TeamProjectQuoteController(DbHandler dbHandler)
        {
            _db = dbHandler;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}