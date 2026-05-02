using Microsoft.EntityFrameworkCore;

namespace QuanlyKTX.Models
{
    // Lớp này kế thừa từ KTXContext (file mà bạn ông đã đổi tên)
    // Nó hoạt động như một "cầu nối", giúp các Controller cũ vẫn chạy bình thường
    // mà không cần phải đi sửa (Replace All) hàng loạt file.
    public partial class KtxthongminhContext : KTXContext
    {
        public KtxthongminhContext()
        {
        }

        public KtxthongminhContext(DbContextOptions<KTXContext> options)
            : base(options)
        {
        }
    }
}