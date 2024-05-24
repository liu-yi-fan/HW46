using HW46.Models;
using HW46.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW46.Controllers
{
    public class apiController : Controller
    {
        private readonly MyDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public apiController(MyDBContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult cities()
        {
            var cities = _db.Addresses.Select(c => c.City).Distinct();
            return Json(cities);
        }

        public IActionResult area(string seleCity)
        {
            var road = _db.Addresses.Where(c => c.City == seleCity).Select(r => r.SiteId).Distinct();
            return Json(road);
        }

        public IActionResult road(string seleArea)
        {
            var road = _db.Addresses.Where(c => c.SiteId == seleArea).Select(r => r.Road);
            return Json(road);
        }

        public IActionResult Avatar(int id = 1)
        {
            Member? member = _db.Members.Find(id);
            if (member != null)
            {
                byte[] img = member.FileData;
                if (img != null)
                {
                    return File(img, "image/jpeg");
                }
            }
            return NotFound();
        }

        public IActionResult CheckAccount(string name)
        {
            var member = _db.Members.Any(m => m.Name == name);

            return Content(member.ToString(), "text/plain", System.Text.Encoding.UTF8);
        }

        public IActionResult Register(Member member, IFormFile avatar)
        {
            if (string.IsNullOrEmpty(member.Name))
            {
                member.Name = "guest";
            }

            //取得上傳檔案的資訊
            //string info = $"{avatar.FileName} - {avatar.Length} - {avatar.ContentType}";
            //string info = _hostEnvironment.ContentRootPath;

            //檔案上傳寫進資料夾
            //todo1 判斷檔案是否存在
            //todo2 限制上傳檔案的大小跟類型 

            //實際路徑
            //string uploadPath = @"C:\Users\User\Documents\workspace\MSIT158Site\wwwroot\uploads\a.png";
            //WebRootPath：C: \Users\User\Documents\workspace\MSIT158Site\wwwroot
            //ContentRootPath：C:\Users\User\Documents\workspace\MSIT158Site
            string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", avatar.FileName);
            string info = uploadPath;
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                avatar.CopyTo(fileStream);
            }

            //檔案上傳轉成二進位
            byte[]? imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                avatar.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }

            //寫進資料庫
            member.FileName = avatar.FileName;
            member.FileData = imgByte;
            _db.Members.Add(member);
            _db.SaveChanges();

            return Content(info, "text/plain", System.Text.Encoding.UTF8);
            // return Content($"Hello {member.Name}，{member.Age} 歲了，電子郵件是 {member.Email}", "text/html", System.Text.Encoding.UTF8);
        }


        [HttpPost]
        public IActionResult Spots([FromBody] SearchDTO searchDTO)
        {
            //根據分類編號搜尋景點資料
            var spots = searchDTO.categoryId == 0 ? _db.SpotImagesSpots : _db.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);

            //根據關鍵字搜尋景點資料(title、desc)
            if (!string.IsNullOrEmpty(searchDTO.keyword))
            {
                spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) || s.SpotDescription.Contains(searchDTO.keyword));
            }

            //排序
            switch (searchDTO.sortBy)
            {
                case "spotTitle":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default:
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }

            //總共有多少筆資料
            int totalCount = spots.Count();
            //每頁要顯示幾筆資料
            int pageSize = searchDTO.pageSize;   //searchDTO.pageSize ?? 9;
            //目前第幾頁
            int page = searchDTO.page;

            //計算總共有幾頁
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);

            //分頁
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


            //包裝要傳給client端的資料
            SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            spotsPaging.TotalCount = totalCount;
            spotsPaging.TotalPages = totalPages;
            spotsPaging.SpotsResult = spots.ToList();


            return Json(spotsPaging);
        }


    }
}

