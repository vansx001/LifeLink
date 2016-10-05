﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LifeLink.Models;
using Microsoft.AspNet.Identity;
using GoogleMaps.LocationServices;
using RestSharp;
using RestSharp.Authenticators;
using static System.Net.WebRequestMethods;

namespace LifeLink.Controllers
{
    public class AddressesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Addresses
        public ActionResult Index()
        {
            var address = db.Address.Include(a => a.AspNetUsers);
            return View(address.ToList());
        }

        // GET: Addresses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Address.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // GET: Addresses/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AddressId,FirstName,LastName,Address1,Address2,City,ZipCode,PhoneNumber,Latitude,Longitude,UserId")] Address address)
        {
            var UserId = User.Identity.GetUserId();
            var email = (from x in db.Users where (x.Id == UserId) select x).FirstOrDefault();
            

            if (ModelState.IsValid)
            {
                var location = address.Address1 + address.City;
                var locationService = new GoogleLocationService();
                var point = locationService.GetLatLongFromAddress(location);
                var latitude = point.Latitude;
                var longitude = point.Longitude;
                address.Latitude = latitude;
                address.Longitude = longitude;
                
                db.Address.Add(address);
                db.SaveChanges();
                SendSimpleMessage(email.Email, address.FirstName);

                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", address.UserId);
            return View(address);
        }



        public static IRestResponse SendSimpleMessage(string email, string name)
        {
            RestClient client = new RestClient();
            client.BaseUrl =  new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                   new HttpBasicAuthenticator("api",
                                              "key-6b04081c6bcdb7dbbeba3a38cf108514");
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                                "sandbox8fc8526133914962ac4338c2fe382486.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandbox8fc8526133914962ac4338c2fe382486.mailgun.org>");
            request.AddParameter("to", email);
            request.AddParameter("subject", "Hello {0}"+ name);
            request.AddParameter("text", "Congratulations {0}! Thank you for registering with LifeLink. LifeLink is a web app,"+
                                 " that builds and cultivates the relationship between blood donors and their local donation center. We simplify"+
                                 " and enrich the donation experience every step of the way. As a donor you can quickly and easily determine if"+
                                 " you are qualified to donate through our simple questionnaire. This site is available in several different"+
                                 " languages for your convenience.\n\nBest Regards,\n\nThe LifeLink Team"+ name);
            request.Method = Method.POST;

            IRestResponse response = client.Execute(request);
            return response;
        }



        // GET: Addresses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Address.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", address.UserId);
            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AddressId,Address1,Address2,City,ZipCode,Latitude,Longitude,UserId")] Address address)
        {
            if (ModelState.IsValid)
            {
                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", address.UserId);
            return View(address);
        }

        // GET: Addresses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Address.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Address address = db.Address.Find(id);
            db.Address.Remove(address);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
