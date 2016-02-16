using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


class DocumentMetadata
{
    public string Title { get; set; }

    public string Content { get; set; }
    
    public string Format { get; set; }
    
    public string Charset { get; set; }

    public string LocaleCode { get; set; }
    
    public string ProjectId { get; set; }
}


class Lingotek : IDisposable
{
	protected HttpClient Client { get; set; }
	protected String[] Headers { get; set; }

	private String host;
	public String Host 
	{ 
		get { return this.host; }
		set
		{
			this.host = value;
			this.Client.BaseAddress = new Uri(this.host);
		} 
	}
	
	private String accessToken;
	public String AccessToken 
	{ 
		get { return this.accessToken; }
		set 
		{ 
			this.accessToken = value;
			this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.accessToken);
			this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		}
	}

	public String CommunityId { get; set; }
	
	public String ProjectId { get; set; }


	public Lingotek() 
	{
		this.Client = new HttpClient();
	}

	public async Task requestCommunity() 
	{
 		HttpResponseMessage response = await this.Client.GetAsync("/api/community");

    	if (response.IsSuccessStatusCode)
    	{
    		string content = await response.Content.ReadAsStringAsync();
        	Console.WriteLine("\t{0}" + Environment.NewLine + "\t{1}", response.StatusCode, content);
    	}
	}

	public async Task requestProject() 
	{
 		HttpResponseMessage response = await this.Client.GetAsync(String.Format("/api/project/{0}", this.ProjectId));
    	if (response.IsSuccessStatusCode)
    	{
    		string jsonText = await response.Content.ReadAsStringAsync();
			JavaScriptSerializer jsonSerialization = new JavaScriptSerializer();
			var data = jsonSerialization.Deserialize<Dictionary<string,dynamic>>(jsonText);
        	Console.WriteLine("\t{0} ({1})", data["properties"]["title"], data["properties"]["id"]);
    	}
	}

	public async Task<string> createDocument(DocumentMetadata document) 
	{
 		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/document");
		
		var keyValues = new List<KeyValuePair<string, string>>();
		keyValues.Add(new KeyValuePair<string, string>("title", document.Title));
		keyValues.Add(new KeyValuePair<string, string>("content", document.Content));
		keyValues.Add(new KeyValuePair<string, string>("format", document.Format));
		keyValues.Add(new KeyValuePair<string, string>("charset", document.Charset));
		keyValues.Add(new KeyValuePair<string, string>("locale_code", document.LocaleCode));
		keyValues.Add(new KeyValuePair<string, string>("project_id", document.ProjectId));

		request.Content = new FormUrlEncodedContent(keyValues);	

		var documentId = String.Empty;
		await this.Client.SendAsync(request)
	      .ContinueWith(responseTask =>
	      {
	          Console.WriteLine("\tResponse status: {0}", responseTask.Result.StatusCode);
   			  JavaScriptSerializer jsonSerialization = new JavaScriptSerializer();
			  var data = jsonSerialization.Deserialize<Dictionary<string,dynamic>>(responseTask.Result.Content.ReadAsStringAsync().Result);
			  documentId = data["properties"]["id"];
			  Console.WriteLine("\tDocument ID: {0}", documentId);
	      });
	     return documentId;
	}

	public async Task<bool> checkImportStatus(String documentId) 
	{
 		HttpResponseMessage response = await this.Client.GetAsync(String.Format("/api/document/{0}", documentId));
 		var result = false;
    	if (response.IsSuccessStatusCode)
    	{
    		result = true;
    	}
    	return result;
	}

	public async Task requestTranslation(String documentId, String locale) 
	{
 		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, String.Format("/api/document/{0}/translation", documentId));

		var keyValues = new List<KeyValuePair<string, string>>();
		keyValues.Add(new KeyValuePair<string, string>("locale_code", locale));

		request.Content = new FormUrlEncodedContent(keyValues);	

		await this.Client.SendAsync(request)
	      .ContinueWith(responseTask =>
	      {
	          Console.WriteLine("\t{0} => {1} ({2})", documentId, locale, responseTask.Result.StatusCode);
	      });
	}

	public async Task<int> checkTranslationStatus(String documentId, String locale) 
	{
		var result = 0;
 		await this.Client.GetAsync(String.Format("/api/document/{0}/status", documentId))
 			.ContinueWith(responseTask =>
 			{
   			  JavaScriptSerializer jsonSerialization = new JavaScriptSerializer();
			  var data = jsonSerialization.Deserialize<Dictionary<string,dynamic>>(responseTask.Result.Content.ReadAsStringAsync().Result);
			  result = data["properties"]["progress"];
		    });
    	return result;
	}

	public async Task downloadTranslation(String documentId, String locale) 
	{
		this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
		this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
 		await this.Client.GetAsync(String.Format("/api/document/{0}/content?locale_code={1}", documentId, locale))
 			.ContinueWith(responseTask =>
 			{
			  Console.WriteLine("\tDocument: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
		    });
	}

	public async Task delete(String documentId) 
	{
 		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, String.Format("/api/document/{0}", documentId));
		await this.Client.SendAsync(request)
	      .ContinueWith(responseTask =>
	      {
	          Console.WriteLine("\tDelete document: {0} => {1}", documentId, responseTask.Result.StatusCode);
	      });

	}


	public void Dispose() 
	{
		this.Client.Dispose();
	}


}

public class Executable
{
    static public void Main ()
    {
		Console.OutputEncoding = Encoding.UTF8;

    	using (Lingotek lingotek = new Lingotek() 
    	{ 
    		Host = "https://cms.lingotek.com",
    		AccessToken = "b068b8d9-c35b-3139-9fe2-e22ee7998d9f",  // sandbox token
    		CommunityId = "f49c4fca-ff93-4f01-a03e-aa36ddb1f2b8",  // sandbox community
    		ProjectId = "103956f4-17cf-4d79-9d15-5f7b7a88dee2",  // sandbox project
		}) 
		{
			// Let's read a community info.
 			Console.WriteLine("COMMUNITY");
 			lingotek.requestCommunity().Wait();

			// Let's read a project info.
 			Console.WriteLine("PROJECT");
 			lingotek.requestProject().Wait();

			// Create a document 			
 			Console.WriteLine("DOCUMENT");

 			// This is the document that we need to translate.
 			Dictionary<string, string> document = new Dictionary<string, string>();
 			document.Add("title", "This is my demo document");
 			document.Add("body", "This is the body of my demo document");

 			// This is the document metadata needed for the TMS.
 			DocumentMetadata metadata = new DocumentMetadata() { 
 				Title = "This is my demo document", 
 				Content = JsonConvert.SerializeObject(document).ToString(),
 				Format = "json",
 				Charset = "utf-8",
 				LocaleCode = "en-US",
 				ProjectId = lingotek.ProjectId,
 			};
 			var documentId = lingotek.createDocument(metadata).Result;

 			// Check the status of our document.
 			Console.WriteLine("IMPORT STATUS");
 			bool imported = false;
			var import_status_message = "\t{0} | check status: => {1}";
 			for (int i = 0; i < 30 && !imported; i++) 
 			{
 				System.Threading.Thread.Sleep(3000);
 				var result = lingotek.checkImportStatus(documentId).Result;
 				if (result) 
 				{
 					imported = true;
		 			Console.WriteLine(import_status_message, i, "imported!");
 				}
 				else 
 				{
		 			Console.WriteLine(import_status_message, i, "importing...");
 				}
			}
			if (!imported) 
			{
	 			Console.WriteLine("\tDocument never completed imported.");
	 			Environment.Exit(2);
			}

 			// Request a translation of our document.
 			Console.WriteLine("REQUEST TRANSLATION");
 			var locale = "zh-CN";
 			lingotek.requestTranslation(documentId, locale).Wait();


 			// Check the status of our document translation.
 			Console.WriteLine("TRANSLATION STATUS");
 			bool translated = false;
			var translation_status_message = "\t{0} | progress: => {1}%";
 			for (int i = 0; i < 50 && !translated; i++) 
 			{
 				System.Threading.Thread.Sleep(3000);
 				var status = lingotek.checkTranslationStatus(documentId, locale).Result;
	 			Console.WriteLine(translation_status_message, i, status);
 				if (status == 100) 
 				{
 					translated = true;
 				}
			}
			if (!translated) 
			{
	 			Console.WriteLine("\tDocument never completed translation.");
	 			Environment.Exit(3);
			}

 			// Download the translation of our document.
 			Console.WriteLine("DOWNLOAD TRANSLATION");
 			lingotek.downloadTranslation(documentId, locale).Wait();

 			// Delete document from the server.
 			Console.WriteLine("CLEANUP");
 			lingotek.delete(documentId).Wait();
		}
    }
}
