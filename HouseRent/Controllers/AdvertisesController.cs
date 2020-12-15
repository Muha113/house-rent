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
using Microsoft.AspNetCore.Hosting; 
using HouseRent.Services;
using Microsoft.Extensions.Configuration;

namespace HouseRent.Controllers
{
    public class AdvertisesController : Controller
    {
        private const string AdString = "ID,Heading,UserMail,Phone,PostTime,RentDate,Address,YoutubeLink,FlatSize,FlatType,Category,Rent,FlatDetails,UtilitiesBill,OtherBill";
        private const string CmntString = "ID,AdvertiseID,Commenter,CommentTime,Anonymous,CommentText";
        private readonly HouseRentContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AdvertisesController(HouseRentContext context, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        public FileContentResult GetImg(int id)
        {
            var image = _context.Image.Find(id).FlatImage;
            return image != null ? new FileContentResult(image, "image/png") : null;
        }

        public string YTlink(string link)
        {
            if (link == null)
                return null;
            try
            {
                int youtu = link.IndexOf("youtu.be");
                
                if (youtu != -1)
                {
                    link = link.Substring(link.IndexOf("be/") + 3, 11);
                }
                else
                {
                    link = link.Substring(link.IndexOf("?v=") + 3, 11);
                }

                return "https://www.youtube.com/embed/" + link;
            }
            catch
            {
                return null;
            }
        }

        // GET: Advertises
        public async Task<IActionResult> Index(DateTime rentFrom, DateTime rentTo, string area, string type, string rent, string category)
        {
            var Add = from a in _context.Advertise
                      select a;

            Add = Add.Where(a => a.ConfirmationStatus == Advertise.StatusConfirmed);

            if (rentFrom != DateTime.MinValue && rentTo != DateTime.MinValue)
            {
                Add = Add.Where(a => !RangesIntersects(a.RentRanges, rentFrom, rentTo));
            }

            if (!String.IsNullOrEmpty(area))
            {
                Add = Add.Where(a => a.Address.Contains(area));
            }
            if (!String.IsNullOrEmpty(type))
            {
                Add = Add.Where(t => t.FlatType.Contains(type));
            }
            if (!String.IsNullOrEmpty(category))
            {
                Add = Add.Where(c => c.Category.Contains(category));
            }

            if (!String.IsNullOrEmpty(rent))
            {
                int low = 1000 * Int32.Parse(rent.Substring(0, 2));
                int up = 1000 * Int32.Parse(rent.Substring(4, 2));
                Add = Add.Where(r => r.Rent >= low
                    && r.Rent <= up);
            }

            return View(await Add.ToListAsync());
        }

        public async Task<IActionResult> MyRequests()
        {
            int currentUserID = Int32.Parse(HttpContext.Session.GetString("sId"));
            if (currentUserID == 0)
            {
                return RedirectToAction("Login", "Users");
            }

            var resp = from a in _context.AdvertiseRequest select a;

            Dictionary<string, Dictionary<string, List<AdvertiseRequest>>> result = new Dictionary<string, Dictionary<string, List<AdvertiseRequest>>>();

            resp = resp.Where(e => e.From == currentUserID || e.To == currentUserID);
            foreach (var x in resp)
            {
                var responseAdv = from b in _context.Advertise select b;
                responseAdv = responseAdv.Where(e => e.ID == x.AdvID);

                var tmp = await responseAdv.ToListAsync();
                x.Adv = tmp[0];

                string key;
                if (currentUserID == x.From)
                {
                    key = AdvertiseRequest.RequestOrderFrom;
                }
                else
                {
                    key = AdvertiseRequest.RequestOrderTo;
                }

                if (!result.ContainsKey(x.Type))
                    result[x.Type] = new Dictionary<string, List<AdvertiseRequest>>();

                if (!result[x.Type].ContainsKey(key))
                    result[x.Type][key] = new List<AdvertiseRequest>();
                
                result[x.Type][key].Add(x);
            }

            ViewBag.StatusArray = new List<string>() { Advertise.StatusConfirmed, Advertise.StatusDeclined, Advertise.StatusPending };

            return View(result);
        }

        public async Task<IActionResult> MyPosts()
        {
            string usermail = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(usermail))
            {
                return RedirectToAction("Login", "Users");
            }
            var mypost = from mp in _context.Advertise
                         where mp.UserMail == usermail
                         select mp;
            return View(await mypost.ToListAsync());
        }

        // GET: Advertises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string tempUser = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(tempUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                var userexist = await _context.User.SingleOrDefaultAsync(u => u.Email == tempUser);
                if (userexist == null)
                {
                    return RedirectToAction("Logout", "Users");
                }
            }

            if (id == null)
            {
                return NotFound();
            }

            var advertise = await _context.Advertise
                .SingleOrDefaultAsync(m => m.ID == id);
            if (advertise == null)
            {
                return NotFound();
            }

            advertise.YoutubeLink = YTlink(advertise.YoutubeLink);

            var images = from i in _context.Image
                         where i.AdvertiseID == id
                         select i;
            advertise.Images = await images.ToListAsync();

            var comments = from c in _context.Comment
                           where c.AdvertiseID == id
                           select c;
            advertise.Comments = await comments.ToListAsync();

            var reviews = from r in _context.Review
                          where r.AdvertiseID == id
                          select r;
            advertise.Reviews = await reviews.ToListAsync();

            var compliments = from c in _context.Compliment
                              where c.AdvertiseID == id
                              select c;

            advertise.Compliments = await compliments.ToListAsync();

            return View(advertise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoReview(Review review)
        {
            string usr = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(usr))
            {
                return RedirectToAction("Login", "Users");
            }
            review.Reviewer = usr;

            var Reviewed = await _context.Review.SingleOrDefaultAsync(r => r.Reviewer == usr && r.AdvertiseID == review.AdvertiseID);

            if (Reviewed == null)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
            }
            else
            {
                Reviewed.ReviewStar = review.ReviewStar;
                _context.Update(Reviewed);
                await _context.SaveChangesAsync();

            }

            return RedirectToAction("Details", new { id = review.AdvertiseID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoCompliment(Compliment compliment)
        {
            string usr = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(usr))
            {
                return RedirectToAction("Login", "Users");
            }
            compliment.Reviewer = usr;

            var Complimented = await _context.Compliment.SingleOrDefaultAsync(c => c.Reviewer == usr && c.AdvertiseID == compliment.AdvertiseID);

            if (Complimented == null)
            {
                _context.Add(compliment);
                await _context.SaveChangesAsync();
            }
            else
            {
                Complimented.Cleanness = compliment.Cleanness;
                Complimented.Comfort = compliment.Comfort;
                Complimented.PriceQuality = compliment.PriceQuality;
                Complimented.Staff = compliment.Staff;
                _context.Update(Complimented);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = compliment.AdvertiseID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoComment([Bind(CmntString)] Comment comment)
        {
            string usr = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(usr))
            {
                return RedirectToAction("Login", "Users");
            }
            comment.CommentTime = DateTime.Now;
            comment.Commenter = usr;
            _context.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = comment.AdvertiseID });

        }

        public async Task<IActionResult> DeleteComment(int id)
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("sEmail")))
            {
                return RedirectToAction("Login", "Users");
            }
            var comment = await _context.Comment.SingleOrDefaultAsync(m => m.ID == id);

            if (HttpContext.Session.GetString("sEmail") == comment.Commenter)
            {
                _context.Comment.Remove(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = comment.AdvertiseID });
        }

        public IActionResult ChangeConfirmationStatus(AdvertiseRequest advReq)
        {
            var tt = from a in _context.AdvertiseRequest select a;
            tt = tt.Where(e => e.ID == advReq.ID);
            var advRequest = tt.ToList()[0];

            var qq = from a in _context.Advertise select a;
            qq = qq.Where(e => e.ID == advReq.AdvID);
            var adv = qq.ToList()[0];

            if (advReq.Type == "ToPlace")
            {
                adv.ConfirmationStatus = advReq.Status;

                _context.Advertise.Update(adv);

                _context.SaveChanges(); 
            }
            else
            {
                advRequest.Status = advReq.Status;

                _context.AdvertiseRequest.Update(advRequest);

                _context.SaveChanges();
            }

            return RedirectToAction("MyRequests", "Advertises");
        }

        // GET: Advertises/Create
        public async Task<IActionResult> Create()
        {
            string tempUser = HttpContext.Session.GetString("sEmail");

            if (String.IsNullOrEmpty(tempUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                var userexist = await _context.User.SingleOrDefaultAsync(u => u.Email == tempUser);
                if (userexist == null)
                {
                    return RedirectToAction("Logout", "Users");
                }
            }
            return View();
        }

        // POST: Advertises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(AdString)] Advertise advertise, List<IFormFile> imgs)
        {
            if (ModelState.IsValid)
            {
                advertise.PostTime = DateTime.Now;
                advertise.UserMail = HttpContext.Session.GetString("sEmail");

                var to = from a in _context.User select a;
                to = to.Where(e => e.Role == "admin");

                AdvertiseRequest req = new AdvertiseRequest();
                req.Adv = advertise;
                req.From = Int32.Parse(HttpContext.Session.GetString("sId"));
                req.Type = AdvertiseRequest.RequestToPlace;
                
                if (to.Count() == 0)
                {
                    advertise.ConfirmationStatus = Advertise.StatusConfirmed;
                    _context.Add(req);
                }
                else
                {
                    advertise.ConfirmationStatus = Advertise.StatusPending;
                    foreach (var x in to)
                    {
                        req.To = x.ID;
                        _context.Add(req);
                    }
                }

                _context.Add(advertise);
                await _context.SaveChangesAsync();
                foreach (var img in imgs)
                {

                    if (img.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            img.CopyTo(ms);
                            var fileBytes = ms.ToArray();

                            Image image = new Image();
                            image.AdvertiseID = advertise.ID;
                            image.FlatImage = fileBytes;
                            _context.Add(image);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(advertise);
        }

        // GET: Advertises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string tempUser = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(tempUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                var userexist = await _context.User.SingleOrDefaultAsync(u => u.Email == tempUser);
                if (userexist == null)
                {
                    return RedirectToAction("Logout", "Users");
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var advertise = await _context.Advertise.SingleOrDefaultAsync(m => m.ID == id);
            if (advertise == null)
            {
                return NotFound();
            }
            else
            {
                if (advertise.UserMail != HttpContext.Session.GetString("sEmail"))
                {
                    return RedirectToAction("Index", "Advertises");
                }
            }
            return View(advertise);
        }

        // POST: Advertises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(AdString)] Advertise advertise)
        {
            if (id != advertise.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(advertise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertiseExists(advertise.ID))
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
            return View(advertise);
        }

        // GET: Advertises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            string tempUser = HttpContext.Session.GetString("sEmail");
            if (String.IsNullOrEmpty(tempUser))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var userexist = await _context.User.SingleOrDefaultAsync(u => u.Email == tempUser);
                if (userexist == null)
                {
                    return RedirectToAction("Logout", "Users");
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var advertise = await _context.Advertise
                .SingleOrDefaultAsync(m => m.ID == id);
            if (advertise == null)
            {
                return NotFound();
            }
            else
            {
                if (advertise.UserMail != HttpContext.Session.GetString("sEmail")
                    && HttpContext.Session.GetString("sRole") != "admin")
                {
                    return RedirectToAction("Index", "Advertises");
                }
            }

            return View(advertise);
        }

        // POST: Advertises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertise = await _context.Advertise.SingleOrDefaultAsync(m => m.ID == id);
            _context.Advertise.Remove(advertise);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ActionName("GetRentInfo")]
        public string GetRentInfo(int id)
        {
            var adv = _context.Advertise.Include(a => a.RentRanges).First(a => a.ID == id);
            var ranges = adv.RentRanges;
            var json = "[";
            foreach (var range in ranges)
            {
                var title = range.Status == RentStatus.Pending ? "Pending" : "Rented";
                var start = range.RentFrom.ToString("yyyy-MM-dd");
                var end = range.RentTo.AddDays(1).ToString("yyyy-MM-dd");
                json += $"{{\"title\": \"{title}\", \"start\": \"{start}\", \"end\": \"{end}\"}},";
            }
            if (ranges.Count() != 0) 
            {
                json = json.Substring(0, json.Length - 1);
            }
            json += "]";
            return json;
        }

        [HttpPost, ActionName("Rent")]
        public async Task<IActionResult> Rent(int id, DateTime rentFrom, DateTime rentTo)
        {
            var Add = _context.Advertise.Include(a => a.RentRanges).First(async => async.ID == id);

            if (rentFrom == DateTime.MinValue || rentTo == DateTime.MinValue)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (RangesIntersects(Add.RentRanges, rentFrom, rentTo))
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var rentRange = new RentRange { RentFrom = rentFrom, RentTo = rentTo, Status = RentStatus.Pending };
            var req = new AdvertiseRequest();

            req.From = Int32.Parse(HttpContext.Session.GetString("sId"));
            req.Type = AdvertiseRequest.RequestToBook;
            req.Status = Advertise.StatusPending;
            req.AdvID = id;

            var to_id = from a in _context.User select a;

            to_id = to_id.Where(e => e.Email == Add.UserMail);

            req.To = to_id.ToList()[0].ID;

            _context.AdvertiseRequest.Add(req);

            Add.RentRanges.Add(rentRange);
            _context.SaveChanges();

            var task = Task.Run(async () =>
            {
                using (var es = _emailService.SendEmailAsync(Add.UserMail, "You have a request to rent your house", $"Your house {Add.Address} want to be rented"))
                {
                    await es;
                }
            });

            var task2 = Task.Run(async () =>
            {
                using (var es = _emailService.SendEmailAsync(HttpContext.Session.GetString("sEmail"), "Booking request", $"Your booking request on hose {Add.Address} handling"))
                {
                    await es;
                }
            });

            return RedirectToAction(nameof(Index));
        }

        private bool AdvertiseExists(int id)
        {
            return _context.Advertise.Any(e => e.ID == id);
        }

        private bool RangesIntersects(List<RentRange> rentRanges, DateTime rentFrom, DateTime rentTo)
        {
            var a = rentRanges.Where(r =>
                        (r.RentFrom <= rentFrom && r.RentTo >= rentTo) ||
                        (r.RentFrom >= rentFrom && rentTo >= r.RentFrom) ||
                        (r.RentTo >= rentFrom && rentTo >= r.RentTo) ||
                        (r.RentFrom >= rentFrom && r.RentTo <= rentTo)
                    );
            return a.Count() != 0;
        }
    }
}
