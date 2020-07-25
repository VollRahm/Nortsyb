using Newtonsoft.Json;
using Nortsyb.Lib.Internal;
using Nortsyb.Lib.Internal.Types;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Nortsyb.Lib
{
    public class NortsybClient
    {
        private HttpClient httpClient = new HttpClient();
        private WebSocketClient webSocket = new WebSocketClient();
        private string Token { get; set; }

        public event SocketMessageHandler MessageRecieved;

        public NortsybClient()
        {
            httpClient.BaseAddress = new Uri("http://localhost:3000");
        }

        public async Task<bool> LoginAsync(string _username, string _password)
        {
            bool httpSuccess = false;
            bool wsSuccess = false;

            var request = BuildJsonRequest(new { username = _username, password = _password }, HttpMethod.Post, Endpoints.AUTH);
            

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Token = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()).token;
                httpSuccess = true;
            }

            CancellationTokenSource ct = new CancellationTokenSource();
            ct.CancelAfter(10000);
            var wsResponse = await webSocket.ConnectAsync(new Uri("ws://localhost:3001"), ct.Token);
            if (wsResponse == System.Net.WebSockets.WebSocketState.Open)
            {
                webSocket.StartListening(new CancellationTokenSource().Token, 2048);
                webSocket.Message += new SocketMessageHandler((o, ev) =>
                {
                    if (ev.StartsWith("auth:success")) wsSuccess = true;
                });
                await webSocket.Send("auth:" + Token);
                await Task.Delay(30);
            }
            if (wsSuccess)
            {
                webSocket.Message += new SocketMessageHandler((o, ev) => MessageRecieved?.Invoke(o, ev));
            }
            return httpSuccess && wsSuccess;
        }

        public async Task<IReadOnlyList<User>> ListUsers()
        {
            if (Token == null) throw new Exception("Not logged in yet");

            var request = BuildJsonRequest(new { token = Token }, HttpMethod.Post, Endpoints.LIST_USERS);
            var response = await httpClient.SendAsync(request);
            if(response.StatusCode  == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var users = JsonConvert.DeserializeObject<List<User>>(await response.Content.ReadAsStringAsync());
                    return users.AsReadOnly();
                }
                catch
                {
                    return new List<User>().AsReadOnly();
                }
            }
            else { return new List<User>().AsReadOnly(); }
        }

        public async Task<bool> SendMessage(int _id, string _message)
        {
            if (Token == null) throw new Exception("Not logged in yet");

            var request = BuildJsonRequest(new { token = Token, id = _id, message = _message }, HttpMethod.Post, Endpoints.SEND);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK) return true;
            else return false;
        }

        private HttpRequestMessage BuildJsonRequest(object jsonObj, HttpMethod method, string endpoint)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
            var str = JsonConvert.SerializeObject(jsonObj);
            request.Content = new StringContent(str, Encoding.UTF8, "application/json");
            return request;
        }
    }
}
