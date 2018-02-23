using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace DownloadAsyncImages
{ 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            textbox.Text = "http://www.belltur.ru/images/STRANI/OAE/a9f90822e2980b724f0b03d85f8a0b76.png\nhttps://fullhdpictures.com/wp-content/uploads/2016/04/Dubai-HD-Wallpaper.jpg\nhttps://fullhdpictures.com/wp-content/uploads/2015/11/Eiffel-Tower-Wallpaper-HD.jpg\nhttp://free4kwallpaper.com/wp-content/uploads/2016/01/Black-and-White-2016-Plumeria-4K-Wallpapers.jpg\nhttp://www.setwalls.ru/download.php?file=201304/2560x1440/setwalls.ru-18792.jpg\nhttps://wallpaperscraft.com/image/flare_sunset_macro_pink_berries_33860_3840x2160.jpg\nhttps://getbg.net/upload/full/www.GetBg.net_Nature___Sundown_Sunset_on_the_lake_071145_.jpg";
        }

        //если нажали на кнопку "start"
        private void start_Click(object sender, RoutedEventArgs e)
        {
            //проверяем на пустоту текстбокс
            if(string.IsNullOrWhiteSpace(textbox.Text))
            {
                log.Foreground = Brushes.Red;
                log.Content = "Пустое текстовое поле";
                return;
            }
            else log.Foreground = Brushes.Black;

            var imgs = textbox.Text.Replace(" ","").Split('\n');
            //создаем будущий список валидных картинок
            List<string> _images = new List<string>();

            //техническая переменная
            int temp = 0; long totalLength = 0;
            //получаем размер файлов
            foreach (var img in imgs)
            {
                if (string.IsNullOrWhiteSpace(img))
                    continue;
                temp++;
                WebRequest req = WebRequest.Create(img);
                req.Method = "HEAD";
                try
                {
                    using (WebResponse resp = req.GetResponse())
                    {
                        long ContentLength; //размер файла в битах
                        if (long.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                        {
                            if (img.Contains(".jpg") || img.Contains(".gif") || img.Contains(".jpeg") || img.Contains(".png"))
                            {
                                totalLength += ContentLength;
                                _images.Add(img); //добовляем валидные картинки в список
                                log.Content += "Img_" + temp + ": " + ConvertSize(ContentLength, "MB") + " Мб. "; //служебная инфа - показывает, сколько в Мб файл весит
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message); //обрабатываем ошибки если есть (например 404, файл/страница не найдена)
                }
            }

            if(_images.Count > 0)
            {
                progress_bar.Maximum = totalLength;
                temp = 0;
                foreach (var img in _images)
                {
                    temp++;
                    WebClient downloader = new WebClient();
                    downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted); // привязываем эвент когда файл загрузится
                    downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged); // привязываем эвент когда файл загружается (в процессе)

                    if (!Directory.Exists(@"C:\my_work\"))
                        Directory.CreateDirectory(@"C:\my_work\");

                    downloader.DownloadFileAsync(new Uri(img), @"C:\my_work\img_" + temp + "." + img.Split('.').Last()); //загружаем ассинхронно
                }
            } 
        }
        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) // событие по которому передается в прогресс бар кол-во процентов загруженной информации
        {
            progress_bar.Value += e.BytesReceived; // progress_bar - прогресс бар
            log.Foreground = Brushes.Black;
            log.Content = "Загружено " + (progress_bar.Value * 100) / progress_bar.Maximum + " %";
        } 

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) // событие по окончанию загрузки (п.с. как пример, в рабочем варианте использовать MessageBox лучше не стоит, т.к. он блокирует основной поток (форма зависает, пока не нажмёте кнопку "ок")
        {
            if (e.Error != null)
            {
                log.Foreground = Brushes.Red;
                log.Content = e.Error.Message;
            } 
            else
            {
                if(progress_bar.Maximum == progress_bar.Value)
                {
                    log.Foreground = Brushes.Black;
                    log.Content = "Загружено!!!"; 
                } 
            }    
        }
        public static double ConvertSize(double bytes, string type)
        {
            try
            {
                const int CONVERSION_VALUE = 1024;
                //determine what conversion they want
                switch (type)
                {
                    case "BY":
                        //convert to bytes (default)
                        return bytes; 
                    case "KB":
                        //convert to kilobytes
                        return (bytes / CONVERSION_VALUE); 
                    case "MB":
                        //convert to megabytes
                        return Math.Round((bytes / CalculateSquare(CONVERSION_VALUE)), 2);
                    case "GB":
                        //convert to gigabytes
                        return (bytes / CalculateCube(CONVERSION_VALUE)); 
                    default:
                        //default
                        return bytes; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
         
        public static double CalculateSquare(Int32 number)
        {
            return Math.Pow(number, 2);
        }
        
        public static double CalculateCube(Int32 number)
        {
            return Math.Pow(number, 3);
        }

    }
}
