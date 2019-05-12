using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Bot
{
    public class WeatherReportCurrent : ModuleBase
    {
        public class WeatherDataCurrent
        {

            public class Coord
            {

                [JsonProperty("lon")]
                public double Lon { get; set; }

                [JsonProperty("lat")]
                public double Lat { get; set; }
            }
            public class Weather
            {
                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("main")]
                public string Main { get; set; }

                [JsonProperty("description")]
                public string Description { get; set; }

                [JsonProperty("icon")]
                public string Icon { get; set; }
            }


            public class Main
            {

                [JsonProperty("temp")]
                public double Temp { get; set; }

                [JsonProperty("pressure")]
                public int Pressure { get; set; }

                [JsonProperty("humidity")]
                public int Humidity { get; set; }

                [JsonProperty("temp_min")]
                public double TempMin { get; set; }

                [JsonProperty("temp_max")]
                public double TempMax { get; set; }
            }

            public class Wind
            {

                [JsonProperty("speed")]
                public double Speed { get; set; }

                [JsonProperty("deg")]
                public double Deg { get; set; }
            }

            public class Clouds
            {

                [JsonProperty("all")]
                public int All { get; set; }
            }

            public class Sys
            {

                [JsonProperty("type")]
                public int Type { get; set; }

                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("message")]
                public double Message { get; set; }

                [JsonProperty("country")]
                public string Country { get; set; }

                [JsonProperty("sunrise")]
                public int Sunrise { get; set; }

                [JsonProperty("sunset")]
                public int Sunset { get; set; }
            }

            public class WeatherReportCurrent
            {

                [JsonProperty("coord")]
                public Coord Coord { get; set; }

                [JsonProperty("weather")]
                public Weather[] Weather { get; set; }

                [JsonProperty("base")]
                public string Base { get; set; }

                [JsonProperty("main")]
                public Main Main { get; set; }

                [JsonProperty("visibility")]
                public int Visibility { get; set; }

                [JsonProperty("wind")]
                public Wind Wind { get; set; }

                [JsonProperty("clouds")]
                public Clouds Clouds { get; set; }

                [JsonProperty("dt")]
                public int Dt { get; set; }

                [JsonProperty("sys")]
                public Sys Sys { get; set; }

                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("cod")]
                public int Cod { get; set; }

            }
        }

        public async Task<String> GetWeatherAsync(string city)
        {
            var httpClient = new HttpClient();
            string URL = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=79369521cfbd42ac3256cdc1d20c395b";
            var response = await httpClient.GetAsync(URL);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        [Command("weather")]
        [Summary("Shows weather info about a certain city.")]
        public async Task WeatherAsync([Remainder] string city = null)
        {
            WeatherDataCurrent.WeatherReportCurrent weather;
            weather = JsonConvert.DeserializeObject<WeatherDataCurrent.WeatherReportCurrent>(GetWeatherAsync(city).Result);
            //Loading classes
            double longi = weather.Coord.Lon;
            double lati = weather.Coord.Lat;    
            double Tempi = weather.Main.Temp;
            var Pressurei = weather.Main.Pressure;
            var Humiditi = weather.Main.Humidity;
            double Tempmi = weather.Main.TempMin;
            double Tempmx = weather.Main.TempMax;
            var WindS = weather.Wind.Speed;
            double degW = weather.Wind.Deg;
            var Cloud = weather.Clouds.All;
            var ID = weather.Sys.Id;
            var test = weather.Weather[0].Icon;
            var desc = weather.Weather[0].Description;
            var c = weather.Name;
          
            //Done
            var embed = new EmbedBuilder();
            embed.Title = ($"Weather Report for {city}");
            embed.ThumbnailUrl = "http://openweathermap.org/img/w/" + weather.Weather[0].Icon + ".png\n";
            var application = await Context.Client.GetApplicationInfoAsync();
            embed.WithColor(new Color(0x4900ff))

                 .AddField(y =>

                 {

                     y.Name = "Name:";
                     y.Value = c;
                     y.IsInline = true;
                 })
                 .AddField(y =>

                 {

                     y.Name = "Coordinates:";
                     y.Value = ($"Longitude: {longi} \nLatitude: {lati}");
                     y.IsInline = true;
                 })
                
                 .AddField(y =>

                 {

                     y.Name = "City ID #";
                     y.Value = ID;
                     y.IsInline = true;
                 })

    .AddField(y =>

    {

        y.Name = "Current Temperature:";
        y.Value = ($" {Tempi - 273} C \n{ 1.8 * (Tempi - 273) + 32} F");
        y.IsInline = true;
    })
     .AddField(y =>

     {

         y.Name = "Low Temperature:";
         y.Value = ($" {Tempmi - 273} C \n{ 1.8 * (Tempmi - 273) + 32} F");
         y.IsInline = true;
     })
      .AddField(y =>

      {

          y.Name = "High Temperature:";
          y.Value = ($" {Tempmx - 273} C \n{ 1.8 * (Tempmx - 273) + 32} F");
          y.IsInline = true;
      })
       .AddField(y =>

       {

           y.Name = "Wind:";
           y.Value = ($"Wind Deg: {degW} \nWind Speed: {WindS}m/s");
           y.IsInline = true;
       })

        .AddField(y =>

        {

            y.Name = "Humidity:";
            y.Value = ($"{Humiditi}% ");
            y.IsInline = true;
        })
            .AddField(y =>

             {

                 y.Name = "Pressure:";
                 y.Value = ($"{Pressurei}hpa");
                 y.IsInline = true;
             })
             
               .AddField(y =>

               {

                   y.Name = "Description:";
                   y.Value = desc;
                   y.IsInline = true;
               })
             .AddField(y =>

              {

                  y.Name = "Clouds:";
                  y.Value = ($"{Cloud}% ");
                  y.IsInline = true;
              });

            await ReplyAsync("", false, embed.Build());
        }
    }
}

    
