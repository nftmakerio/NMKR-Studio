using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NMKR.Shared.Functions
{
    public static class ImageResize
    {
        public static void Resize(string srcPath, string dstPath, int width, int height)
        {
            using Image image = Image.Load(srcPath);
            image.Mutate(x => x.Resize(width, height));

            image.Save(dstPath);
        }

    }
}
