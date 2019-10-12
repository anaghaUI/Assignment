using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Assignment.Models;
using FIT5032_Week08A.Utils;
using Microsoft.AspNet.Identity;

namespace Assignment.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private EventEntity db = new EventEntity();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Event);
            if (User.IsInRole("Staff"))
            {
                return View(bookings.ToList());
            }
            else
            {
                var userId = User.Identity.GetUserId();
                var existingBookings = bookings.Where(s => s.CustomerId ==
                userId).ToList();
                return View(existingBookings);
            }
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Confirm/5
        public ActionResult Confirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Confirm/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public ActionResult Confirm(int bookingId)
        {
            Booking booking = db.Bookings.Find(bookingId);
            booking.Status = "Confirmed";
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();

                try
                {
                    String toEmail = db.AspNetUsers.Find(booking.CustomerId).Email;
                    String subject = "Booking Confimation";
                    String contents = "<div><h2>Your booking has been confirmed!</h2>" +
                        "<b>Event Details</b>" +
                        "<p>Booking ID: " + booking.BookingId + "</p>" +
                        "<p>Event: " + booking.Event.Name + "</p>" +
                        "<p>Event Date: " + booking.EventDate + "</p>" +
                        "<p>Number of People: " + booking.NumberOfPeople + "</p>" +
                        "<p>Status: <b>CONFIRMED</b></p>";

                    EmailSender es = new EmailSender();
                    es.SendSingleMail(toEmail, subject, contents);

                    ViewBag.Result = "Email has been send.";

                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.EventId = new SelectList(db.Events, "Id", "Name", booking.EventId);
                    return View(booking);
                }
            }
            ViewBag.EventId = new SelectList(db.Events, "Id", "Name", booking.EventId);
            return View(booking);
        }


        // GET: Bookings/Create
        [Authorize(Roles = "Customer")]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //ViewBag.EventId = new SelectList(db.Events, "Id", "Name");
            ViewBag.Event = db.Events.Find(id);
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult Create([Bind(Include = "BookingId,EventId,CustomerId,EventDate,BookingDate,Status,NumberOfPeople,Remarks")] Booking booking)
        {
            booking.CustomerId = User.Identity.GetUserId();
            booking.BookingDate = DateTime.Today;
            booking.Status = "Pending";
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return RedirectToAction("Index");
            }

            ViewBag.EventId = new SelectList(db.Events, "Id", "Name", booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Customer")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult Edit([Bind(Include = "BookingId,EventId,CustomerId,EventDate,BookingDate,Status,NumberOfPeople,Remarks")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EventId = new SelectList(db.Events, "Id", "Name", booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles = "Customer")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
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
