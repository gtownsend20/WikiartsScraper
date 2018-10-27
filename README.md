### Requirements
* Google Chrome  
* Windows 7+  

Tested with Google Chrome 70.0.3538.77+ (default browser)
### Building as Release
NuGet sync for Selenium packages

Open the solution in Visual Studio, build as release, navigate to the project's folder, `bin/Release` will contain the executable.

### Adding an artist
The dropdown list contains a dictionary with a key-value pair for every artist. Here, the key is the full name of the artist and the value is the URL to the `all-works/text-list` page of that artist. Adding a new key-value pair here will allow you to scrape the paintings of that artist. 
```C#
SortedDictionary<string, string> userCache = new SortedDictionary<string, string>
{
    {"Rembrandt van Rijn", "https://www.wikiart.org/en/rembrandt/all-works/text-list"},
    {"Ferdinand Bol", "https://www.wikiart.org/en/ferdinand-bol/all-works/text-list"},
    {"Gerrit Dou", "https://www.wikiart.org/en/gerrit-dou/all-works/text-list"},
    {"Carel Fabritius", "https://www.wikiart.org/en/carel-fabritius/all-works/text-list" },
    {"Nicolaes Maes", "https://www.wikiart.org/en/nicolaes-maes/all-works/text-list"},
    {"Samuel Dirksz van Hoogstraten", "https://www.wikiart.org/en/samuel-dirksz-van-hoogstraten/all-works/text-list" }
};
```
## Usage
1. Choose the artist you want images and/or information from in the dropdown list.
2. Click on the 'Find Pages' button to load all of the `all-works/text-list` page in a data structure (working memory only). Wait for this process to finish before clicking on any other buttons. Since no background worker is implemented here, it can seem that the program is unresponsive during this period.
3. Clicking on the 'Save images found' button saves the images of this artist to `C:\CurrentDateTime` folder by going to each individual page and downloading the image. You can see this happening live in the Chrome tab opened by selenium
4. Clicking on the 'Save data of images in csv' button opens a dialog where you can choose the location where the `.csv` will be stored. The structure of this .csv is as follows

| id | artist     | namepainting                   | yearpainting | url                                                                       |
|----|------------|--------------------------------|--------------|---------------------------------------------------------------------------|
| 59 | Gerrit Dou | The Lady at Her Dressing Table | 1667         | https://www.wikiart.org/en/gerrit-dou/the-lady-at-her-dressing-table-1667 |
| 60 | Gerrit Dou | Violin player                  | 1667         | https://www.wikiart.org/en/gerrit-dou/violin-player-1667                  |
| 61 | Gerrit Dou | A Hermit Praying               | 1670         | https://www.wikiart.org/en/gerrit-dou/a-hermit-praying-1670               |

Note that the name of the images stored in `C:\CurrentDateTime` correspond to the `id` in the table (e.g. the painting 'Violin player' will be called `60.jpg`.  

This means that each artist will have a seperate incrementing id from `0` to `n`

### Future Work
* URL "prediction" to eliminate seperate navigation to each artist's painting if there is no desire to save additonal information/features from that painting.
* CLI 
* Ability to queue a set of artists for scraping to overcome the `0` to `n` behaviour mentioned above. 
