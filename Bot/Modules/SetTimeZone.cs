/*using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Discord.Commands;
using Newtonsoft.Json;
using Ofl.Google.Maps.TimeZone;

namespace Bot
{ 
   public class SetTimeZone : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalUserAccounts _globalUserAccounts;

        public SetTimeZone(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }

        [Command("MyCity")]
        public async Task SetMyCity([Remainder] string city)
        {
            var timeZone = ConvertCityToTimeZoneName(city);
            if (timeZone.Result == "error")
            {
                await ReplyAsync("Something went wrong... Try to check your spelling");
                return;
            }

            var account = _globalUserAccounts.GetById(Context.User.Id);
            account.TimeZone = $"{timeZone.Result}";
            _globalUserAccounts.SaveAccounts(Context.User.Id);

            await ReplyAsync($"We saved it. Your TimeZone is **{timeZone.Result}**");
        }


        public async Task<string> ConvertCityToTimeZoneName(string location)
        {
            TimeZoneResponse response = new TimeZoneResponse();
            var plusName = location.Replace(" ", "+");
            var address = "http://maps.google.com/maps/api/geocode/json?address=" + plusName + "&sensor=false";
            var result =  await Global.SendWebRequest(address);
            var latLongResult = JsonConvert.DeserializeObject<dynamic>(result);

            if (latLongResult.status == "OK")
            {
                string string1 = latLongResult.results[0].geometry.location.lat.ToString().Replace(",", ".");
                string string2 = latLongResult.results[0].geometry.location.lng.ToString().Replace(",", ".");

                var timeZoneRespontimeZoneRequest = "https://maps.googleapis.com/maps/api/timezone/json?location=" + 
                                                    string1 + "," +
                                                    string2 +
                                                    "&sensor=false&timestamp=1362209227";

                var timeZoneResponseString = await Global.SendWebRequest(timeZoneRespontimeZoneRequest);  //new System.Net.WebClient().DownloadString(timeZoneRespontimeZoneRequest);
                var timeZoneResult = JsonConvert.DeserializeObject<dynamic>(timeZoneResponseString);

                if (timeZoneResult.status == "OK")
                {

                    response.TimeZoneId = timeZoneResult.timeZoneId;
                    return response.TimeZoneId;
                }
            }
            return "error";
        }

    }
} */
