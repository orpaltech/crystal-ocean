using System;
using System.IO;
using FreeImageAPI;

namespace CrystalOcean.Core.Images
{
    public abstract class ImageFile
    {
        public ImageFile()
        {	
        }        

        public void OpenFile(String fileName)
        {
            if (!File.Exists(fileName))
			{
				Console.WriteLine(fileName + " does not exist. Aborting.");
				return;
			}

            FIBITMAP dib = new FIBITMAP();

            dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_JPEG, 
                                fileName, 
                                FREE_IMAGE_LOAD_FLAGS.JPEG_ACCURATE);

            // Check if the handle is null which means the bitmap could not be loaded.
			if (dib.IsNull)
			{
				Console.WriteLine("Loading bitmap failed. Aborting.");
				// Check whether there was an error message.
				return;
			}

            //FreeImage.Save(FREE_IMAGE_FORMAT.FIF_TIFF, dib, outFileName, FREE_IMAGE_SAVE_FLAGS.TIFF_DEFLATE);
        }
    }
}