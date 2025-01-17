using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace c_sharp
{
  public class Truv
  {

    private static string accessToken = null;
    private string clientId = Environment.GetEnvironmentVariable("API_CLIENT_ID");
    private string clientSecret = Environment.GetEnvironmentVariable("API_SECRET");
    private string productType = Environment.GetEnvironmentVariable("API_PRODUCT_TYPE");
    private readonly HttpClient client;

    public Truv()
    {
      client = new HttpClient();
      client.DefaultRequestHeaders.Add("X-Access-Client-Id", clientId);
      client.DefaultRequestHeaders.Add("X-Access-Secret", clientSecret);
    }

    public async Task<string> SendRequest(string endpoint, string content = "", string method = "POST")
    {
      var request = new HttpRequestMessage
      {
        RequestUri = new Uri("https://prod.truv.com/v1/" + endpoint),
        Method = method == "POST" ? HttpMethod.Post : HttpMethod.Get,
        Content = new StringContent(content, Encoding.UTF8, "application/json"),
      };
      var response = await client.SendAsync(request);
      return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetBridgeToken()
    {
      var account = productType == "fas" || productType == "deposit_switch" ? "\"account\": { \"account_number\": \"16002600\", \"account_type\": \"checking\", \"routing_number\": \"123456789\", \"bank_name\": \"TD Bank\" }," : "";
      Console.WriteLine("TRUV: Requesting bridge token from https://prod.truv.com/v1/bridge-tokens");
      var body = "{ \"product_type\": \"" + productType + "\"," +
                 account +
                 " \"tracking_info\": \"1337\"," +
                 " \"client_name\": \"Truv Quickstart\"" +
                 "}";
      return await SendRequest("bridge-tokens/", body);
    }

    public async Task<string> GetAccessToken(string publicToken)
    {
      Console.WriteLine("TRUV: Exchanging a public_token for an access_token from https://prod.truv.com/v1/link-access-tokens");
      Console.WriteLine("TRUV: Public Token - {0}", publicToken);
      var response = await SendRequest("link-access-tokens/", "{\"public_token\": \"" + publicToken + "\" }");
      var parsedResponse = JsonDocument.Parse(response);
      Truv.accessToken = parsedResponse.RootElement.GetProperty("access_token").GetString();
      return response;
    }

    public async Task<string> GetEmploymentInfoByToken(string accessToken)
    {
      if(accessToken == null)
        accessToken = Truv.accessToken;
      Console.WriteLine("TRUV: Requesting employment verification data using an access_token from https://prod.truv.com/v1/verifications/employments");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("verifications/employments/", "{\"access_token\": \"" + accessToken + "\" }");
    }

    public async Task<string> GetIncomeInfoByToken(string accessToken)
    {
      if(accessToken == null)
        accessToken = Truv.accessToken;
      Console.WriteLine("TRUV: Requesting income verification data using an access_token from https://prod.truv.com/v1/verifications/incomes");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("verifications/incomes/", "{\"access_token\": \"" + accessToken + "\" }");
    }

    public async Task<string> CreateRefreshTask()
    {
      Console.WriteLine("TRUV: Requesting a data refresh using an access_token from https://prod.truv.com/v1/refresh/tasks");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("refresh/tasks/", "{\"access_token\": \"" + accessToken + "\" }");
    }

    public async Task<string> GetRefreshTask(string taskId)
    {
      Console.WriteLine("TRUV: Requesting a refresh task using a task_id from https://prod.truv.com/v1/refresh/tasks/{task_id}");
      Console.WriteLine("TRUV: Task ID - {0}", taskId);
      return await SendRequest($"refresh/tasks/{taskId}", "", "GET");
    }

    public async Task<string> GetEmployeeDirectoryByToken(string accessToken)
    {
      if(accessToken == null)
        accessToken = Truv.accessToken;
      Console.WriteLine("TRUV: Requesting employee directory data using an access_token from https://prod.truv.com/v1/administrators/directories");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("administrators/directories/", "{\"access_token\": \"" + accessToken + "\" }");
    }

    public async Task<string> RequestPayrollReport(string accessToken, string startDate, string endDate)
    {
      if(accessToken == null)
        accessToken = Truv.accessToken;
      Console.WriteLine("TRUV: Requesting a payroll report be created using an access_token from https://prod.truv.com/v1/administrators/payrolls");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      var body = "{ \"access_token\": \"" + accessToken + "\"," +
                 " \"start_date\": \"" + startDate + "\"," +
                 " \"end_date\": \"" + endDate + "\"" +
                 "}";
      var response = await SendRequest("administrators/payrolls/", body);
      var parsedResponse = JsonDocument.Parse(response);
      return parsedResponse.RootElement.GetProperty("payroll_report_id").GetString();
    }

    public async Task<string> GetPayrollById(string reportId)
    {
      Console.WriteLine("TRUV: Requesting a payroll report using a report_id from https://prod.truv.com/v1/administrators/payrolls/{report_id}");
      Console.WriteLine("TRUV: Report ID - {0}", reportId);
      return await SendRequest($"administrators/payrolls/{reportId}", "", "GET");
    }

    public async Task<string> GetFundingSwitchStatusByToken(string accessToken)
    {
      Console.WriteLine("TRUV: Requesting funding switch update data using an access_token from https://prod.truv.com/v1/account-switches");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest($"account-switches", "{\"access_token\": \"" + accessToken + "\" }", "POST");
    }

    public async Task<string> CompleteFundingSwitchFlowByToken(string accessToken, float first_micro, float second_micro)
    {
      Console.WriteLine("TRUV: Completing funding switch flow with a Task refresh using an access_token from https://prod.truv.com/v1/refresh/tasks");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("refresh/tasks/", "{\"access_token\": \"" + accessToken + "\", \"settings\": { \"micro_deposits\": [" + first_micro.ToString() + ", " + second_micro.ToString() + "] } }");
    }

    public async Task<string> GetDepositSwitchByToken(string accessToken)
    {
      Console.WriteLine("TRUV: Requesting direct deposit switch data using an access_token from https://prod.truv.com/v1/deposit-switches");
      Console.WriteLine("TRUV: Access Token - {0}", accessToken);
      return await SendRequest("deposit-switches/", "{\"access_token\": \"" + accessToken + "\" }");
    }
  }

  public class AccessTokenResponse
  {
    public List<string> access_tokens { get; set; }
  }
}