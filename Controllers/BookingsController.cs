using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
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

        // GET: Bookings
        [Authorize(Roles = "Staff")]
        public ActionResult Chart()
        {
            var bookings = db.Bookings.Include(b => b.Event);
            IEnumerable<ChartModel> eventBookingMap = db.Database.SqlQuery<ChartModel>("select name as EventName, coalesce(bookingCount,0) as NoOfBookings from Event a left join (select eventId, count(*) as bookingCount from Booking group by EventId) b on a.Id = b.EventId;");

            return View(eventBookingMap.ToList());
            

            //var eventBookingMap = from s in db.Bookings
            //                      group s by s.EventId into bookingGroup
            //                      join st in db.Events on bookingGroup.Where(b) equals st.Id

                                    
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

        // GET: Bookings/Rate/5
        public ActionResult Rate(int? id)
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
        public ActionResult Rate([Bind(Include = "BookingId,EventId,CustomerId,EventDate,BookingDate,Status,NumberOfPeople,Remarks,Rating")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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

            var countOfBookings = db.Bookings.Include(b => b.Event).Where(b => b.BookingDate.CompareTo(DateTime.Now) > 1 && b.Status.Equals("Confirmed")).Count();
            if (ModelState.IsValid)
            {
                booking.Status = "Confirmed";
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

        public List<String> getUnavailableDays()
        {
            var unavailableDates = new List<String>();
            foreach (var eachBooking in db.Bookings)
            {
                var count = 1;
                foreach (var temp in db.Bookings)
                {
                    if (eachBooking.BookingId != temp.BookingId && eachBooking.EventDate.CompareTo(temp.EventDate) == 0)
                    {
                        count += 1;
                    }
                }
                if (count >= 5 && !unavailableDates.Contains(String.Format("{0:MM/dd/yyyy}", eachBooking.EventDate)))
                {
                    unavailableDates.Add(String.Format("{0:MM/dd/yyyy}", eachBooking.EventDate));
                }
            }
            return unavailableDates;
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
            //var unavailableDates = new List<DateTime>();
            
            ViewBag.UnavailableDates = getUnavailableDays();
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
            var countOfEvents = db.Bookings.Where(b => booking.EventDate.CompareTo(b.EventDate) == 0);
            if (countOfEvents.Count() >= 5)
            {
                ModelState.AddModelError("EventDate", "Please select another date as we're fully booked for the selected day.");
            }
            else
            {
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
            }
            
            ViewBag.UnavailableDates = getUnavailableDays();
            ViewBag.Event = db.Events.Find(booking.EventId);
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

            ViewBag.UnavailableDates = getUnavailableDays();
            ViewBag.Event = db.Events.Find(db.Bookings.Find(id).EventId);
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
            booking.Status = "Pending";
            var countOfEvents = db.Bookings.Where(b => booking.EventDate.CompareTo(b.EventDate) == 0);
            if (countOfEvents.Count() >= 5)
            {
                ModelState.AddModelError("EventDate", "Please select another date as we're fully booked for the selected day.");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.UnavailableDates = getUnavailableDays();
            ViewBag.Event = db.Events.Find(booking.EventId);
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
