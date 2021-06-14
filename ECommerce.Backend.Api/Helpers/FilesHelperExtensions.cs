using System.IO;

namespace ECommerce.Backend.Api.Helpers
{
    public class FilesHelperExtensions
    {
        public static void DeleteImage(string fileName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }
}
