using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Advertising.Mobile.UI;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;

namespace CoinConverter
{

    public partial class MainPage : PhoneApplicationPage
    {

        
        private Dictionary<String, float> currencies;
        private List<String> filter;


        public void downloadComplete(Object sender, DownloadStringCompletedEventArgs e)
        {
            String result;
            Boolean web;

            try
            {
                result = e.Result;
                web = true;
            }
            catch (WebException we)
            {
                result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><gesmes:Envelope xmlns:gesmes=\"http://www.gesmes.org/xml/2002-08-01\" xmlns=\"http://www.ecb.int/vocabulary/2002-08-01/eurofxref\"><gesmes:subject>Reference rates</gesmes:subject><gesmes:Sender><gesmes:name>European Central Bank</gesmes:name></gesmes:Sender><Cube><Cube time='2011-04-19'><Cube currency='USD' rate='1.4302'/><Cube currency='JPY' rate='118.23'/><Cube currency='BGN' rate='1.9558'/><Cube currency='CZK' rate='24.125'/><Cube currency='DKK' rate='7.4576'/><Cube currency='GBP' rate='0.87800'/><Cube currency='HUF' rate='266.88'/><Cube currency='LTL' rate='3.4528'/><Cube currency='LVL' rate='0.7093'/><Cube currency='PLN' rate='3.9785'/><Cube currency='RON' rate='4.0885'/><Cube currency='SEK' rate='8.9210'/><Cube currency='CHF' rate='1.2842'/><Cube currency='NOK' rate='7.7635'/><Cube currency='HRK' rate='7.3580'/><Cube currency='RUB' rate='40.4869'/><Cube currency='TRY' rate='2.1936'/><Cube currency='AUD' rate='1.3622'/><Cube currency='BRL' rate='2.2647'/><Cube currency='CAD' rate='1.3694'/><Cube currency='CNY' rate='9.3399'/><Cube currency='HKD' rate='11.1240'/><Cube currency='IDR' rate='12422.61'/><Cube currency='ILS' rate='4.9137'/><Cube currency='INR' rate='63.6220'/><Cube currency='KRW' rate='1557.28'/><Cube currency='MXN' rate='16.7398'/><Cube currency='MYR' rate='4.3271'/><Cube currency='NZD' rate='1.8154'/><Cube currency='PHP' rate='61.973'/><Cube currency='SGD' rate='1.7835'/><Cube currency='THB' rate='42.992'/><Cube currency='ZAR' rate='9.7827'/></Cube></Cube></gesmes:Envelope>";
                web = false;
            }

            IsolatedStorageFileStream file = new IsolatedStorageFileStream("coin.xml", FileMode.OpenOrCreate, IsolatedStorageFile.GetUserStoreForApplication());

            if ((file.Length <= 0) || web)
            {
                using (StreamWriter stream = new StreamWriter(file))
                {
                    stream.Write(result);
                    stream.Close();
                }
            }

            file.Close();

            updateUi();
        }

        public MainPage()
        {
            
            InitializeComponent();
            currencies = new Dictionary<String, float>();
            currencies.Add("EUR", 1);
            filter = new List<String>()
            {
                "usd","eur","jpy","gbp","sek"
            };

            WebClient web = new WebClient();
            web.DownloadStringCompleted += new DownloadStringCompletedEventHandler(downloadComplete);
            web.DownloadStringAsync(new Uri("http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"));
            
        }

        public void updateUi()
        {

            IsolatedStorageFileStream file = new IsolatedStorageFileStream("coin.xml", FileMode.Open, IsolatedStorageFile.GetUserStoreForApplication());
            XDocument doc = XDocument.Load(file);
            file.Close();
            
            XElement root = (from cur in doc.Descendants("{http://www.ecb.int/vocabulary/2002-08-01/eurofxref}Cube") select cur).FirstOrDefault();
            XElement subroot = root.Elements().FirstOrDefault();
            
            foreach (XElement currency in subroot.DescendantNodes())
            {
                String name = currency.Attribute("currency").Value;
                if (!filter.Contains(name.ToLower()))
                {
                    continue;
                }

                float value;
                if (float.TryParse(currency.Attribute("rate").Value, out value))
                {
                    currencies.Add(name, value);
                }
            }



            for (int i = 0; i < currencies.Count; i++)
            {
                String fromName = currencies.ElementAt(i).Key.ToString();
                String fromValue = currencies.ElementAt(i).Value.ToString();

                PanoramaItem temp = new PanoramaItem() { Header = fromName, Name = "panoramaItem" + fromName};

                Grid innerGrid = new Grid() { Name = "innerGrid" + fromName};
                innerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80)});
                innerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(290)});
                innerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Auto)});


                TextBox tempTextBox = new TextBox() { Name = fromName };
                tempTextBox.TextChanged += new TextChangedEventHandler(textChangedHandler);
                InputScopeNameValue numbersOnly = InputScopeNameValue.TelephoneNumber;
                tempTextBox.InputScope = new InputScope()
                {
                    Names = {new InputScopeName() {NameValue = numbersOnly}}
                };
                
                
                innerGrid.Children.Add(tempTextBox);
                Grid.SetRow(tempTextBox, 0);

                ScrollViewer tempScroll = new ScrollViewer() { Name = "scroll" + fromName };
                Grid tempGrid = new Grid();
                int rowIndex = 0;
                for (int j = 0; j < currencies.Count; j++)
                {
                    if (i != j)
                    {
                        tempGrid.RowDefinitions.Add(new RowDefinition());
                        StackPanel tempStack = new StackPanel()
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal
                        };
                        Image tempImage = new Image()
                        {
                            Source = new BitmapImage(new Uri("../images/" + currencies.ElementAt(j).Key.ToString() + ".png", UriKind.Relative)),
                            Stretch=Stretch.Fill,
                            Width= 70,
                            Height=45,
                            Margin = new Thickness(10)
                        };
                        tempStack.Children.Add(tempImage);

                        TextBlock tempText = new TextBlock() { Name = "textBlockFrom" + fromName + "For" + currencies.ElementAt(j).Key.ToString(), Text = currencies.ElementAt(j).Key.ToString() + " = ...", FontSize = 42 };
                        tempStack.Children.Add(tempText);
                        tempGrid.Children.Add(tempStack);
                        Grid.SetRow(tempStack, rowIndex);
                        
                        rowIndex++;
                    }
                }
                tempScroll.Content = tempGrid;

                innerGrid.Children.Add(tempScroll);
                Grid.SetRow(tempScroll, 1);

                temp.Content = innerGrid;
                RootView.Items.Add(temp);


                AdControl.TestMode = false;
                //ApplicationID = "4fd1b245-b648-44de-add5-1a6408f4618a", AdUnitID = "10016135", 
                //AdModel = Contextual, RotationEnabled = true
                AdControl adControl = new AdControl("4fd1b245-b648-44de-add5-1a6408f4618a", // ApplicationID
                                                    "10016135",    // AdUnitID
                                                    AdModel.Contextual, // AdModel
                                                    true);         // RotationEnabled
                // Make the AdControl size large enough that it can contain the image
                adControl.Width = 300;
                adControl.Height = 80;

                innerGrid.Children.Add(adControl);
                Grid.SetRow(adControl, rowIndex++);
            }
        }

        private void textChangedHandler(Object sender, TextChangedEventArgs e)
        {
            String fromName = (((TextBox)sender).Name).ToString().ToUpper();
            float fromValue;
            if (float.TryParse(((TextBox)sender).Text, out fromValue))
            {
                foreach (KeyValuePair<String, float> currency in currencies)
                {
                    String toName = currency.Key.ToString().ToUpper();
                    if (!fromName.Equals(toName))
                    {
                        TextBlock temp = (TextBlock)RootView.FindName("textBlockFrom" + fromName + "For" + toName);
                        temp.Text = currency.Key + " = " + (fromValue * ((float)currency.Value/(float)currencies[fromName])).ToString();
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<String, float> currency in currencies)
                {
                    String toName = currency.Key.ToString().ToUpper();
                    if (!fromName.Equals(toName))
                    {
                        TextBlock temp = (TextBlock)RootView.FindName("textBlockFrom" + fromName + "For" + toName);
                        temp.Text = currency.Key + " = Invalid";

                    }
                }
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    
    }
}