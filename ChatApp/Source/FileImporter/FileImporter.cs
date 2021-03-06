using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChatApp
{
    public enum FileType
    {
        Image,
        Text
    }

    // static class to import different types of files into the app
    class FileImporter
    {
        public static void ImportFile(FileType fileType, out string filePath)
        {
            filePath = null;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = fileType == FileType.Image ? "jpg (*.jpg)|*.jpg;*.png" : "txt (*.txt)|*.txt";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                Console.WriteLine(openFileDialog.FileName);
            }
        }

        public static BitmapImage CacheImage(string filePath) // Required so image file isn't locked.
        {
            if (filePath == null)
                return null;

            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(filePath);
            img.EndInit();

            return img;
        }

        public static BitmapImage CacheImage(byte[] imgData)
        {
            if (imgData == null)
                return null;
        
            BitmapImage img = new BitmapImage();
        
            using (MemoryStream stream = new MemoryStream(imgData))
            {
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = stream;
                img.EndInit();
            }
        
            return img;
        }

        public static BitmapImage ImportImage()
        {
            ImportFile(FileType.Image, out string filePath);
            return CacheImage(filePath);
        }

        public static string ImportText()
        {
            ImportFile(FileType.Text, out string filePath);
            return File.ReadAllText(filePath);
        }
    }
}
