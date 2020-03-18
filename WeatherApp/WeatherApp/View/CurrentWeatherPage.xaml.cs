using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using WeatherApp.Model;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeatherApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentWeatherPage : ContentPage
    {
        public CurrentWeatherPage()
        {
            InitializeComponent();
            GetCoordinates();
            
        }
        private string Location { get; set; } = "q=Berlin"; //kan antingen bli apicall med zip=zipcode,countrycode. eller q=City,Country.
        public double Latitue { get; set; }
        public double Longitude { get; set; }

        private async void GetCoordinates()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best,TimeSpan.FromSeconds(60));
                var location = await Geolocation.GetLocationAsync(request);
                if(location != null)
                {
                    Latitue = location.Latitude;
                    Longitude = location.Longitude;
                    Location = await GetCity(location);

                    GetWeatherInfo(); //Hämtar info för specific plats.
                }
                
            }
            catch (Exception ex)
            {
                
               await DisplayAlert("Coordinate info", ex.Message, "OK");
            }

        }

        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            var currentPlace = places?.FirstOrDefault();

            if(currentPlace != null)
            {
                if(currentPlace.Locality != null)
                {
                    return $"q={currentPlace.Locality},{currentPlace.CountryName}"; 
                }
                else
                {
                    return $"zip={currentPlace.PostalCode},{currentPlace.CountryCode}"; //Ifall den inte hittar stad, ta ut stad med postkod.
                }   
            }
            else
            {
                return null;
            }
        }

        private async void GetWeatherInfo()
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?{Location}&appid=3b8cb75e995b3b49d89d03b976762a90&units=metric";
            var result = await ApiCaller.Get(url);

            if (result.Success)
            {
                try
                {
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                    pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";
                    windTxt.Text = $"{weatherInfo.wind.speed} m/s";
                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";


                    var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); //Skapar datetime utifrån unixstämpeln från apiet.
                    date = date.AddSeconds(weatherInfo.dt).ToUniversalTime();
                    //var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                    dateTxt.Text = date.ToString("dddd, MMM dd").ToUpper();

                    GetForecast(); //Hämtar forecast efter man laddat currentweather.
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather info", ex.Message, "OK");
                }

            }
            else
            {
                await DisplayAlert("Weather Info", "No Information found", "OK"); //Knapp med errormeddlande.
            }
        }
        private async void GetForecast()
        {
            var url = $"http://api.openweathermap.org/data/2.5/forecast?{Location}&appid=3b8cb75e995b3b49d89d03b976762a90&units=metric";
            var result = await ApiCaller.Get(url);

            if (result.Success)
            {
                try
                {
                    var forcastInfo = JsonConvert.DeserializeObject<ForecastInfo>(result.Response);

                    List<List> allList = new List<List>();

                    foreach (var list in forcastInfo.list)
                    {
                        //var date = DateTime.ParseExact(list.dt_txt, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
                        var date = DateTime.Parse(list.dt_txt);

                        if (date > DateTime.Now && date.Hour == 12 && date.Minute == 0 && date.Second == 0) //Hour är 12 för jag vill ha vädret mitt på dagen.
                            allList.Add(list);
                    }

                    dayOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dddd");
                    dateOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dd MMM");
                    iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                    tempOneTxt.Text = allList[0].main.temp.ToString("0");

                    dayTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dddd");
                    dateTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dd MMM");
                    iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                    tempTwoTxt.Text = allList[1].main.temp.ToString("0");

                    dayThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dddd");
                    dateThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dd MMM");
                    iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                    tempThreeTxt.Text = allList[2].main.temp.ToString("0");

                    dayFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dddd");
                    dateFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dd MMM");
                    iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                    tempFourTxt.Text = allList[3].main.temp.ToString("0");


                }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather Info", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No forecast information found", "OK");
            }
        }

    }
}