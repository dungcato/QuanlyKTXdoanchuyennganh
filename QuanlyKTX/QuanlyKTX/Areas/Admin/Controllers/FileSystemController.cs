using elFinder.NetCore;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/Admin/el-finder-file-system")]
    public class FileSystemController : Controller
    {
        IWebHostEnvironment _env;

        public FileSystemController(IWebHostEnvironment env) => _env = env;

        // URL để client-side kết nối đến backend
        // /el-finder-file-system/connector
        [Route("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            var result = await connector.ProcessAsync(Request);
            if (result is JsonResult)
            {
                var json = result as JsonResult;
                return Content(JsonSerializer.Serialize(json.Value), json.ContentType);
            }
            return result;
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(
                Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            // Thư mục gốc để lưu file upload
            string rootDirectory = Path.Combine(_env.WebRootPath, "files");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(rootDirectory))
                Directory.CreateDirectory(rootDirectory);

            var root = new RootVolume(
                rootDirectory,
                $"{uri.Scheme}://{uri.Host}:{uri.Port}/files/",
                $"{uri.Scheme}://{uri.Host}:{uri.Port}/Admin/el-finder-file-system/thumb/")
            {
                // Không cho phép xóa thư mục gốc
                IsReadOnly = false,
                IsLocked = false,
                Alias = "Files",
                MaxUploadSizeInKb = 2048, // 2MB
            };

            driver.AddRoot(root);

            return new Connector(driver)
            {
                // Giới hạn loại file được upload
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}