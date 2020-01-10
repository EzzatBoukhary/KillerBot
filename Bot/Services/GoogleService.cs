using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;

namespace Bot.Services.Google
{
	public static class GoogleService
	{
		/// <summary>
		/// Searches Google
		/// </summary>
		/// <param name="search"></param>
		/// <param name="appName"></param>
		/// <returns></returns>
		public static Search Search(string search, string appName)
		{
			try
			{

				Search googleSearch;

				using (CustomsearchService google = new CustomsearchService(new BaseClientService.Initializer
				{
					ApiKey = "AIzaSyAJePXxgf3gQxS6iWMNS_bUNceF7FfiS9o",
					ApplicationName = appName,
				}))
				{
					CseResource.ListRequest searchListRequest = google.Cse.List(search);
					searchListRequest.Cx = "010406102614525898105:acg6fgpfbjq";


                    googleSearch = searchListRequest.Execute();
				}

				return googleSearch;
			}
			catch
			{
				return null;
			}
		}
	}
}