using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoSoVatChatController : Controller
    {
        private readonly KtxthongminhContext _context;

        public CoSoVatChatController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH VẬT TƯ / CSVC (DẠNG DASHBOARD THỐNG KÊ)
        [HttpGet]
        public async Task<IActionResult> Index(string searchTen, string searchToaNha, string searchPhong, string searchTinhTrang)
        {
            try
            {
                var query = _context.VatTus.Include(v => v.Phong).AsQueryable();

                if (!string.IsNullOrEmpty(searchTen))
                    query = query.Where(c => c.TenVatTu.Contains(searchTen) || c.MaTaiSan.Contains(searchTen));

                if (!string.IsNullOrEmpty(searchToaNha))
                    query = query.Where(c => c.PhongId != null && c.PhongId.StartsWith(searchToaNha));

                if (!string.IsNullOrEmpty(searchPhong))
                    query = query.Where(c => c.PhongId != null && c.PhongId.Contains(searchPhong));

                if (!string.IsNullOrEmpty(searchTinhTrang))
                    query = query.Where(c => c.TrangThai == searchTinhTrang);

                var danhSach = await query.OrderByDescending(c => c.VatTuId).ToListAsync();

                ViewBag.SearchTen = searchTen;
                ViewBag.SearchToaNha = searchToaNha;
                ViewBag.SearchPhong = searchPhong;
                ViewBag.SearchTinhTrang = searchTinhTrang;

                return View(danhSach);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi khi tải danh sách Vật tư: " + ex.Message;
                return View(new List<VatTu>());
            }
        }

        // 2. FORM THÊM MỚI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // Truyền danh sách Phòng xuống để làm Dropdown
            ViewBag.PhongList = _context.Phongs.ToList();
            ViewBag.AutoMaTaiSan = "TS-" + DateTime.Now.ToString("yyMMddHHmm");
            return View();
        }

        // 3. XỬ LÝ THÊM MỚI (POST)
        [HttpPost]
        public async Task<IActionResult> Create(VatTu vt)
        {
            try
            {
                _context.VatTus.Add(vt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm mới tài sản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu dữ liệu: " + ex.Message;
                ViewBag.PhongList = _context.Phongs.ToList();
                return View(vt);
            }
        }

        // 4. FORM SỬA (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return NotFound();

            ViewBag.PhongList = await _context.Phongs.ToListAsync();
            return View(vt);
        }

        // 5. XỬ LÝ SỬA (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(VatTu vt)
        {
            try
            {
                var dbObj = await _context.VatTus.FindAsync(vt.VatTuId);
                if (dbObj == null) return NotFound();

                dbObj.TenVatTu = vt.TenVatTu;
                dbObj.LoaiVatTu = vt.LoaiVatTu;
                dbObj.PhongId = vt.PhongId;
                dbObj.NgayTrangBi = vt.NgayTrangBi;
                dbObj.HanBaoHanh = vt.HanBaoHanh;
                dbObj.TrangThai = vt.TrangThai;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật tài sản {dbObj.TenVatTu} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                ViewBag.PhongList = _context.Phongs.ToList();
                return View(vt);
            }
        }

        // 6. XÓA TÀI SẢN (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt != null)
            {
                _context.VatTus.Remove(vt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài sản khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}