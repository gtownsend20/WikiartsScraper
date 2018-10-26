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

            // Rembrandt van Rijn, Ferdinand bol, gerrit dou, carel fabritius, nicolaes maes en Samuel van hoogstraten
            SortedDictionary<string, string> userCache = new SortedDictionary<string, string>
            {
                {"Rembrandt van Rijn", "https://www.wikiart.org/en/rembrandt/all-works/text-list"},
                {"Ferdinand Bol", "https://www.wikiart.org/en/ferdinand-bol/all-works/text-list"},
                {"Gerrit Dou", "https://www.wikiart.org/en/gerrit-dou/all-works/text-list"},
                {"Carel Fabritius", "https://www.wikiart.org/en/carel-fabritius/all-works/text-list" },
                {"Nicolaes Maes", "https://www.wikiart.org/en/nicolaes-maes/all-works/text-list"},
                {"Samuel Dirksz van Hoogstraten", "https://www.wikiart.org/en/samuel-dirksz-van-hoogstraten/all-works/text-list" }
            };

            comboBox1.DataSource = new BindingSource(userCache, null);
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
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
                    paintings.Add(new Painting(i, comboBox1.Text, dataLi[0], year, listOfPaintingUrls[i].GetAttribute("href")));
                    LogPaintings(paintings);
                    progressBar1.Value = i;
                }
                label4.Text = "Number of Paintings: " + listOfPaintings.Count;
            }
            else
                MessageBox.Show("Something went wrong with finding the appropriate paintings! Contact Petar :)");
        }


        /// <summary>
        /// Provies a csv-like console writeline loop for the list of Paintings currently in working
        /// memory.
        /// </summary>
        /// <param name="list">The list to be printed</param>
        private void LogPaintings(List<Painting> list)
        {
            Console.WriteLine("id;artist;namepainting;yearpainting;url");
            foreach (var p in paintings)
                Console.WriteLine(p.id + ";" + p.artist + ";" + p.namePainting + ";" + p.yearPainting + ";" + p.url);
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
                // Create a new folder for the run
                string path = @"C:\" + DateTime.Now.ToString("dd-MM-yyyy h-mm-tt");
                Directory.CreateDirectory(path);

                foreach (var p in paintings)
                {
                    // As a test, navigate to the url of the first painting
                    driver.Navigate().GoToUrl(p.url);
                    var imageSrc = driver
                        .FindElement(By.XPath("/html/body/div[2]/div[1]/section[1]/main/div[2]/aside/div[1]/img"))
                        .GetAttribute("src");

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
                    string imageFinalFileName = path + '\\' + p.id + ".jpg";
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
                }

                label5.Text = "Saved to: " + path;
            }
            else
                MessageBox.Show("No paintings in working memory, click on 'Find Paintings' first.");
        }
  
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Retrieve the value from the currently selected key-value dictionary in the combobox
            // and nagivate to that webpage.
            var selectedUrl = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
            driver.Navigate().GoToUrl(selectedUrl);
            label1.Text = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
        }
    }

    class Painting
    {
        public int id;
        public string artist;
        public string namePainting;
        public string yearPainting;
        public string url;

        public Painting(int id, string artist, string namePainting,
            string yearPainting, string url)
        {
            this.id = id;
            this.artist = artist;
            this.namePainting = namePainting;
            this.yearPainting = yearPainting;
            this.url = url;
        }
    }
}
