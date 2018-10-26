using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;



namespace Scraper
{
    public partial class Form1 : Form
    {
        List<Painting> paintings = new List<Painting>();
        private static IWebDriver driver;

        public Form1()
        {
            InitializeComponent();

            // Initialize webdriver
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.wikiart.org/en/rembrandt/all-works/text-list");
            progressBar1.Step = 1;
            progressBar1.Value = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Can use dialog.FileName
                    using (Stream stream = dialog.OpenFile())
                    {
                        // Save data
                    }
                }
            }
        }

        /// <summary>
        /// Finds the paintings on the main page of the specific artist and changes the label
        /// associated with it to show the number of paintings found. Saves to global memory as
        /// Painting object. 
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            var listOfPaintings = driver.FindElements(By.XPath("/html/body/div[2]/div[1]/section/main/ul/li"));
            var listOfPaintingUrls = driver.FindElements(By.XPath("/html/body/div[2]/div[1]/section/main/ul/li/a"));

            if (listOfPaintings.Count > 0)
            {
                progressBar1.Maximum = listOfPaintings.Count;

                for (int i = 0; i < listOfPaintings.Count; i++)
                {
                    string[] dataLi = listOfPaintings[i].Text.Split(',');
                    // dataLi[0] is the name of the painting, dataLi[1] minus a space character is the year of the painting
                    string year = "";

                    // Check to see if there is a space at the head of the array or a question mark. If so, remove it
                    if (dataLi[1].Length == 5 || dataLi[1].Contains('?'))
                        year = dataLi[1].Substring(1, dataLi[1].Length - 1);
                    else
                        year = dataLi[1];
                    Console.WriteLine(listOfPaintingUrls[i].Text);
                    paintings.Add(new Painting(i, "Rembrandt", "15-06-1606", "4-10-1669", dataLi[0], year, listOfPaintingUrls[i].GetAttribute("href")));
                    LogPaintings(paintings);
                    progressBar1.Value = i;
                }
                label4.Text = "Number of Paintings: " + listOfPaintings.Count;
            }
            else
                MessageBox.Show("Something went wrong with finding the appropriate paintings! Contact Petar :)");
        }

        private void LogPaintings(List<Painting> list)
        {
            Console.WriteLine("name;type;businesstype;industrytype");
            foreach (var p in paintings)
                Console.WriteLine(p.id + ";" + p.artist + ";" + p.artistBirthDate + ";" + p.artistDeathDate + ";" +
                                  p.namePainting + ";" + p.yearPainting + ";" + p.url);
        }

        /// <summary>
        /// Saves the images found in the paintings list to a folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (paintings.Count > 0)
            {
                //foreach (var p in paintings)
                // {
                // }

                // As a test, navigate to the url of the first painting
                driver.Navigate().GoToUrl(paintings[0].url);
                var imageSrc = driver
                    .FindElement(By.XPath("/html/body/div[2]/div[1]/section[1]/main/div[2]/aside/div[1]/img"))
                    .GetAttribute("src");

                // Create a new folder for the run
                string path = @"C:\" + DateTime.Now.ToString("dd-MM-yyyy h-mm-tt");
                Directory.CreateDirectory(path);

                // Create an HTTP web request to feed the binary stream reader
                byte[] imageBytes;
                HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(imageSrc);
                WebResponse imageResponse = imageRequest.GetResponse();

                Stream responseStream = imageResponse.GetResponseStream();

                using (BinaryReader br = new BinaryReader(responseStream))
                {
                    imageBytes = br.ReadBytes(500000);
                    br.Close();
                }
                responseStream.Close();
                imageResponse.Close();
                string imageFinalFileName = path + '\\' + paintings[0].id + ".jpg";
                FileStream fs = new FileStream(imageFinalFileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                try
                {
                    bw.Write(imageBytes);
                }
                finally
                {
                    fs.Close();
                    bw.Close();
                }

                label5.Text = "Saved to: " + path;
            }
            else
            {
                MessageBox.Show("No paintings in working memory, click on 'Find Paintings' first.");
            }

        }
    }

    class Painting
    {
        public int id;
        public string artist;
        public string artistBirthDate;
        public string artistDeathDate;
        public string namePainting;
        public string yearPainting;
        public string url;

        public Painting(int id, string artist, string artistBirthDate, string artistDeathDate, string namePainting,
            string yearPainting, string url)
        {
            this.id = id;
            this.artist = artist;
            this.artistBirthDate = artistBirthDate;
            this.artistDeathDate = artistDeathDate;
            this.namePainting = namePainting;
            this.yearPainting = yearPainting;
            this.url = url;
        }
    }
}
